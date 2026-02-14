using Microsoft.AspNetCore.Http;

namespace RoomSplit.API.DTOs.Auth;

public record UpdateProfileDto(string? FullName, IFormFile? Avatar);
