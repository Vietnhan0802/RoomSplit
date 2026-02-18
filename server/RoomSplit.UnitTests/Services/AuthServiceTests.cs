using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Interfaces;
using RoomSplit.Infrastructure.Services;
using RoomSplit.UnitTests.Helpers;

namespace RoomSplit.UnitTests.Services;

public class AuthServiceTests
{
    private readonly AuthService _sut;
    private readonly MockUnitOfWork _mocks;
    private readonly Mock<IJwtService> _jwtService;
    private readonly Mock<IFileService> _fileService;
    private readonly Mock<IConfiguration> _configuration;

    public AuthServiceTests()
    {
        _mocks = MockUnitOfWorkFactory.Create();
        _jwtService = new Mock<IJwtService>();
        _fileService = new Mock<IFileService>();
        _configuration = new Mock<IConfiguration>();

        _configuration.Setup(c => c["Jwt:RefreshTokenExpiryDays"]).Returns("7");

        _sut = new AuthService(
            _configuration.Object,
            _jwtService.Object,
            _mocks.UnitOfWork.Object,
            _fileService.Object);
    }

    #region HashPassword & VerifyPassword

    [Fact]
    public void HashPassword_ReturnsNonEmptyHash()
    {
        var hash = _sut.HashPassword("password123");

        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe("password123");
    }

    [Fact]
    public void HashPassword_SameInput_ReturnsDifferentHashes()
    {
        var hash1 = _sut.HashPassword("password123");
        var hash2 = _sut.HashPassword("password123");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _sut.HashPassword("password123");

        _sut.VerifyPassword("password123", hash).Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ReturnsFalse()
    {
        var hash = _sut.HashPassword("password123");

        _sut.VerifyPassword("wrongpassword", hash).Should().BeFalse();
    }

    #endregion

    #region GenerateJwtToken

    [Fact]
    public void GenerateJwtToken_DelegatesToJwtService()
    {
        var user = new User { Id = Guid.NewGuid(), FullName = "Test", Email = "test@test.com" };
        _jwtService.Setup(j => j.GenerateAccessToken(user)).Returns("test-jwt");

        var result = _sut.GenerateJwtToken(user);

        result.Should().Be("test-jwt");
        _jwtService.Verify(j => j.GenerateAccessToken(user), Times.Once);
    }

    #endregion

    #region CreateRefreshTokenAsync

    [Fact]
    public async Task CreateRefreshTokenAsync_SavesHashedToken_ReturnsUnhashed()
    {
        var user = new User { Id = Guid.NewGuid(), FullName = "Test", Email = "test@test.com" };
        _jwtService.Setup(j => j.GenerateRefreshToken()).Returns("raw-refresh-token");
        _jwtService.Setup(j => j.HashToken("raw-refresh-token")).Returns("hashed-refresh-token");

        string? tokenAtSaveTime = null;
        Guid? savedUserId = null;
        string? savedIp = null;
        _mocks.RefreshTokens
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(rt =>
            {
                // Capture values at save time (before AuthService overwrites Token)
                tokenAtSaveTime = rt.Token;
                savedUserId = rt.UserId;
                savedIp = rt.CreatedByIp;
            })
            .ReturnsAsync((RefreshToken rt) => rt);

        var result = await _sut.CreateRefreshTokenAsync(user, "127.0.0.1");

        // The token passed to AddAsync should be the hashed version
        tokenAtSaveTime.Should().Be("hashed-refresh-token");
        savedUserId.Should().Be(user.Id);
        savedIp.Should().Be("127.0.0.1");

        // The returned token should be the raw unhashed one (AuthService overwrites it after saving)
        result.Token.Should().Be("raw-refresh-token");

        _mocks.UnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_SetsCorrectExpiry()
    {
        var user = new User { Id = Guid.NewGuid(), FullName = "Test", Email = "test@test.com" };
        _jwtService.Setup(j => j.GenerateRefreshToken()).Returns("token");
        _jwtService.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hashed");

        RefreshToken? savedToken = null;
        _mocks.RefreshTokens
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(rt => savedToken = rt)
            .ReturnsAsync((RefreshToken rt) => rt);

        await _sut.CreateRefreshTokenAsync(user, "127.0.0.1");

        savedToken!.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
    }

    #endregion

    #region ValidateRefreshTokenAsync

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithValidToken_ReturnsToken()
    {
        var token = new RefreshToken
        {
            Token = "hashed",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            UserId = Guid.NewGuid()
        };

        _jwtService.Setup(j => j.HashToken("raw-token")).Returns("hashed");
        _mocks.RefreshTokens
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { token });

        var result = await _sut.ValidateRefreshTokenAsync("raw-token");

        result.Should().NotBeNull();
        result.Should().Be(token);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithExpiredToken_ReturnsNull()
    {
        var token = new RefreshToken
        {
            Token = "hashed",
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // expired
            UserId = Guid.NewGuid()
        };

        _jwtService.Setup(j => j.HashToken("raw-token")).Returns("hashed");
        _mocks.RefreshTokens
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { token });

        var result = await _sut.ValidateRefreshTokenAsync("raw-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithRevokedToken_ReturnsNull()
    {
        var token = new RefreshToken
        {
            Token = "hashed",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            RevokedAt = DateTime.UtcNow.AddHours(-1), // revoked
            UserId = Guid.NewGuid()
        };

        _jwtService.Setup(j => j.HashToken("raw-token")).Returns("hashed");
        _mocks.RefreshTokens
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { token });

        var result = await _sut.ValidateRefreshTokenAsync("raw-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithNonExistentToken_ReturnsNull()
    {
        _jwtService.Setup(j => j.HashToken("raw-token")).Returns("hashed");
        _mocks.RefreshTokens
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken>());

        var result = await _sut.ValidateRefreshTokenAsync("raw-token");

        result.Should().BeNull();
    }

    #endregion

    #region RevokeRefreshTokenAsync

    [Fact]
    public async Task RevokeRefreshTokenAsync_WithActiveToken_SetsRevokedFields()
    {
        var token = new RefreshToken
        {
            Token = "hashed",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            UserId = Guid.NewGuid()
        };

        _jwtService.Setup(j => j.HashToken("raw-token")).Returns("hashed");
        _mocks.RefreshTokens
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { token });

        await _sut.RevokeRefreshTokenAsync("raw-token", "192.168.1.1", "Logout");

        token.RevokedAt.Should().NotBeNull();
        token.RevokedByIp.Should().Be("192.168.1.1");
        token.RevokedReason.Should().Be("Logout");
        _mocks.UnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WithInactiveToken_DoesNotSave()
    {
        _jwtService.Setup(j => j.HashToken("raw-token")).Returns("hashed");
        _mocks.RefreshTokens
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken>());

        await _sut.RevokeRefreshTokenAsync("raw-token", "192.168.1.1", "Logout");

        _mocks.UnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region RefreshTokenAsync

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ThrowsUnauthorized()
    {
        _jwtService.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hashed");
        _mocks.RefreshTokens
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken>());

        await _sut.Invoking(s => s.RefreshTokenAsync("invalid", "127.0.0.1"))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid or expired*");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokenPair()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, FullName = "Test", Email = "test@test.com" };
        var existingToken = new RefreshToken
        {
            Token = "hashed-old",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            UserId = userId
        };

        _jwtService.Setup(j => j.HashToken("old-refresh-token")).Returns("hashed-old");
        _jwtService.Setup(j => j.GenerateAccessToken(user)).Returns("new-access-token");
        _jwtService.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh-token");
        _jwtService.Setup(j => j.HashToken("new-refresh-token")).Returns("hashed-new");

        _mocks.RefreshTokens
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { existingToken });
        _mocks.RefreshTokens
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);
        _mocks.Users
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var (accessToken, refreshToken) = await _sut.RefreshTokenAsync("old-refresh-token", "127.0.0.1");

        accessToken.Should().Be("new-access-token");
        refreshToken.Should().Be("new-refresh-token");
    }

    #endregion

    #region UpdateProfileAsync

    [Fact]
    public async Task UpdateProfileAsync_WithFullName_UpdatesUser()
    {
        var user = new User { Id = Guid.NewGuid(), FullName = "Old Name", Email = "test@test.com" };

        await _sut.UpdateProfileAsync(user, "New Name", null);

        user.FullName.Should().Be("New Name");
        _mocks.UnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithAvatar_DeletesOldAndUploadsNew()
    {
        var user = new User { Id = Guid.NewGuid(), FullName = "Test", Email = "test@test.com", AvatarUrl = "/avatars/old.jpg" };
        var mockFile = new Mock<IFormFile>();
        _fileService.Setup(f => f.UploadFileAsync(mockFile.Object, "avatars")).ReturnsAsync("/avatars/new.jpg");

        await _sut.UpdateProfileAsync(user, null, mockFile.Object);

        _fileService.Verify(f => f.DeleteFileAsync("/avatars/old.jpg"), Times.Once);
        _fileService.Verify(f => f.UploadFileAsync(mockFile.Object, "avatars"), Times.Once);
        user.AvatarUrl.Should().Be("/avatars/new.jpg");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithAvatarNoExisting_UploadsOnly()
    {
        var user = new User { Id = Guid.NewGuid(), FullName = "Test", Email = "test@test.com", AvatarUrl = null };
        var mockFile = new Mock<IFormFile>();
        _fileService.Setup(f => f.UploadFileAsync(mockFile.Object, "avatars")).ReturnsAsync("/avatars/new.jpg");

        await _sut.UpdateProfileAsync(user, null, mockFile.Object);

        _fileService.Verify(f => f.DeleteFileAsync(It.IsAny<string>()), Times.Never);
        _fileService.Verify(f => f.UploadFileAsync(mockFile.Object, "avatars"), Times.Once);
        user.AvatarUrl.Should().Be("/avatars/new.jpg");
    }

    #endregion
}
