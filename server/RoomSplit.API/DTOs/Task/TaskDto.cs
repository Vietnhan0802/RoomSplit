using RoomSplit.API.DTOs.Auth;

namespace RoomSplit.API.DTOs.Task;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    string? Icon,
    string FrequencyType,
    int FrequencyValue,
    int CurrentAssigneeIndex,
    DateTime StartDate,
    bool IsActive,
    List<TaskAssignmentDto> Assignments,
    DateTime CreatedAt);

public record TaskAssignmentDto(
    Guid Id,
    UserDto AssignedTo,
    DateTime DueDate,
    string Status,
    DateTime? CompletedAt,
    UserDto? CompletedBy,
    string? Note,
    string? ProofImageUrl);
