using RoomSplit.API.DTOs.Auth;

namespace RoomSplit.API.DTOs.Room;

public record RoomDto(
    Guid Id,
    string Name,
    string? Description,
    string InviteCode,
    UserDto CreatedBy,
    int MemberCount,
    DateTime CreatedAt);

public record RoomDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string InviteCode,
    UserDto CreatedBy,
    List<RoomMemberDto> Members,
    DateTime CreatedAt);

public record RoomMemberDto(Guid Id, UserDto User, string Role, DateTime JoinedAt);
