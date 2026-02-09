using RoomSplit.API.DTOs.Auth;

namespace RoomSplit.API.DTOs.Task;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    string Frequency,
    bool IsRotating,
    int CurrentRotationIndex,
    List<TaskAssignmentDto> Assignments,
    DateTime CreatedAt);

public record TaskAssignmentDto(
    Guid Id,
    UserDto AssignedTo,
    DateTime DueDate,
    string Status,
    DateTime? CompletedAt);
