namespace RoomSplit.API.DTOs.Transaction;

public record CreateTransactionDto(
    int Type,
    decimal Amount,
    int Category,
    string? Description,
    DateTime TransactionDate,
    string? Note);
