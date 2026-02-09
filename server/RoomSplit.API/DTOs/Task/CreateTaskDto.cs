namespace RoomSplit.API.DTOs.Task;

public record CreateTaskDto(string Title, string? Description, int Frequency, bool IsRotating);
