using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Interfaces;

namespace RoomSplit.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;

    public AuthService(
        IConfiguration configuration,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        IFileService fileService)
    {
        _configuration = configuration;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _fileService = fileService;
    }

    public string GenerateJwtToken(User user)
    {
        return _jwtService.GenerateAccessToken(user);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(User user, string ipAddress)
    {
        var token = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = _jwtService.HashToken(token),
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
            CreatedByIp = ipAddress
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        // Return unhashed token to client
        refreshToken.Token = token;
        return refreshToken;
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token)
    {
        var hashedToken = _jwtService.HashToken(token);
        var refreshTokens = await _unitOfWork.RefreshTokens.FindAsync(rt => rt.Token == hashedToken);
        var refreshToken = refreshTokens.FirstOrDefault();

        if (refreshToken == null || !refreshToken.IsActive)
            return null;

        return refreshToken;
    }

    public async Task RevokeRefreshTokenAsync(string token, string ipAddress, string reason)
    {
        var hashedToken = _jwtService.HashToken(token);
        var refreshTokens = await _unitOfWork.RefreshTokens.FindAsync(rt => rt.Token == hashedToken);
        var refreshToken = refreshTokens.FirstOrDefault();

        if (refreshToken == null || !refreshToken.IsActive)
            return;

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.RevokedReason = reason;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<(string accessToken, string refreshToken)> RefreshTokenAsync(
        string refreshToken,
        string ipAddress)
    {
        var validatedToken = await ValidateRefreshTokenAsync(refreshToken);
        if (validatedToken == null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = await _unitOfWork.Users.GetByIdAsync(validatedToken.UserId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        // Revoke old refresh token
        await RevokeRefreshTokenAsync(refreshToken, ipAddress, "Replaced by new token");

        // Update old token's ReplacedByToken field
        var hashedOldToken = _jwtService.HashToken(refreshToken);
        var oldTokens = await _unitOfWork.RefreshTokens.FindAsync(rt => rt.Token == hashedOldToken);
        var oldToken = oldTokens.FirstOrDefault();

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = await CreateRefreshTokenAsync(user, ipAddress);

        // Update ReplacedByToken field
        if (oldToken != null)
        {
            oldToken.ReplacedByToken = _jwtService.HashToken(newRefreshToken.Token);
            await _unitOfWork.SaveChangesAsync();
        }

        return (newAccessToken, newRefreshToken.Token);
    }

    public async Task UpdateProfileAsync(User user, string? fullName, IFormFile? avatarFile)
    {
        if (!string.IsNullOrEmpty(fullName))
        {
            user.FullName = fullName;
        }

        if (avatarFile != null)
        {
            // Delete old avatar if exists
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                await _fileService.DeleteFileAsync(user.AvatarUrl);
            }

            // Upload new avatar
            user.AvatarUrl = await _fileService.UploadFileAsync(avatarFile, "avatars");
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }
}
