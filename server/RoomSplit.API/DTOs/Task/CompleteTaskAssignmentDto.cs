namespace RoomSplit.API.DTOs.Task;

public record CompleteTaskAssignmentDto(
    string? Note
);
// Note: ProofImage is handled as IFormFile parameter in controller, not in DTO
