using Microsoft.AspNetCore.Http;
using RoomSplit.Core.Entities;

namespace RoomSplit.Core.Interfaces;

public interface IAuthService
{
    string GenerateJwtToken(User user);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    Task<RefreshToken> CreateRefreshTokenAsync(User user, string ipAddress);
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token, string ipAddress, string reason);
    Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task UpdateProfileAsync(User user, string? fullName, IFormFile? avatarFile);
}
