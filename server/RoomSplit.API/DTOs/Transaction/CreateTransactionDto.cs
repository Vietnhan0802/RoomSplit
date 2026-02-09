namespace RoomSplit.API.DTOs.Transaction;

public record CreateTransactionDto(
    int Type,
    decimal Amount,
    string Description,
    int? IncomeCategory,
    int? ExpenseCategory,
    DateTime Date,
    string? ImageUrl,
    string? Note,
    string? Tags);
