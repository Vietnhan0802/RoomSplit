namespace RoomSplit.API.DTOs.Task;

public record TaskAssignmentFilterDto(
    DateTime? StartDate,
    DateTime? EndDate,
    Guid? AssignedToUserId,
    int? Status
);
