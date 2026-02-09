using RoomSplit.API.DTOs.Auth;

namespace RoomSplit.API.DTOs.Expense;

public record ExpenseDto(
    Guid Id,
    string Description,
    decimal Amount,
    string Category,
    string SplitType,
    DateTime Date,
    string? ReceiptImageUrl,
    string? Note,
    UserDto PaidBy,
    List<ExpenseSplitDto> Splits,
    DateTime CreatedAt);

public record ExpenseSplitDto(Guid Id, UserDto User, decimal Amount, bool IsSettled, DateTime? SettledAt);
