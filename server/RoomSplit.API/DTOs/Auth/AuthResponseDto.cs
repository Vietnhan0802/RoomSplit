namespace RoomSplit.API.DTOs.Auth;

public record AuthResponseDto(string Token, UserDto User);

public record UserDto(Guid Id, string FullName, string Email, string? AvatarUrl);
