namespace RoomSplit.API.DTOs.Task;

public record UpdateTaskTemplateDto(
    string Title,
    string? Description,
    string? Icon,
    int FrequencyType,
    int FrequencyValue,
    DateTime StartDate,
    List<Guid> RotationOrder,
    bool IsActive
);
