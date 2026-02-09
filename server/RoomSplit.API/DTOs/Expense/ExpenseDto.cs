using RoomSplit.API.DTOs.Auth;

namespace RoomSplit.API.DTOs.Expense;

public record ExpenseDto(
    Guid Id,
    string Description,
    decimal Amount,
    string Category,
    string SplitType,
    DateTime ExpenseDate,
    string? ReceiptUrl,
    string? Note,
    bool IsSettled,
    UserDto PaidBy,
    List<ExpenseSplitDto> Splits,
    DateTime CreatedAt);

public record ExpenseSplitDto(Guid Id, UserDto User, decimal Amount, decimal? Percentage, bool IsPaid);
