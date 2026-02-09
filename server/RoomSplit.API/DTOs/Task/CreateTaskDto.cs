namespace RoomSplit.API.DTOs.Task;

public record CreateTaskDto(
    string Title,
    string? Description,
    string? Icon,
    int FrequencyType,
    int FrequencyValue,
    DateTime StartDate,
    List<Guid> RotationOrder);
