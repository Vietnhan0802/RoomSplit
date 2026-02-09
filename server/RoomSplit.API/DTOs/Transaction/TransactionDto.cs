namespace RoomSplit.API.DTOs.Transaction;

public record TransactionDto(
    Guid Id,
    string Type,
    decimal Amount,
    string Category,
    string? Description,
    DateTime TransactionDate,
    string? Note,
    DateTime CreatedAt);
