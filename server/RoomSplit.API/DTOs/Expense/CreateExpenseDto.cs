namespace RoomSplit.API.DTOs.Expense;

public record CreateExpenseDto(
    string Description,
    decimal Amount,
    int Category,
    int SplitType,
    DateTime ExpenseDate,
    string? Note,
    List<ExpenseSplitInputDto>? Splits);

public record ExpenseSplitInputDto(Guid UserId, decimal? Amount, decimal? Percentage);
