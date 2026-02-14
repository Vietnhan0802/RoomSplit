namespace RoomSplit.API.DTOs.Transaction;

public record UpdateTransactionDto(
    int Type,
    decimal Amount,
    string Description,
    int? IncomeCategory,
    int? ExpenseCategory,
    DateTime Date,
    string? Note,
    string? Tags);
