using System.Security.Claims;
using RoomSplit.Core.Entities;

namespace RoomSplit.Core.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    string HashToken(string token);
}
