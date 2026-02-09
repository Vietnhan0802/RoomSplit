namespace RoomSplit.API.DTOs.Transaction;

public record TransactionDto(
    Guid Id,
    string Type,
    decimal Amount,
    string Description,
    string? IncomeCategory,
    string? ExpenseCategory,
    DateTime Date,
    string? ImageUrl,
    string? Note,
    string? Tags,
    DateTime CreatedAt);
