namespace RoomSplit.API.DTOs.Expense;

public record CreateExpenseDto(
    string Description,
    decimal Amount,
    int Category,
    int SplitType,
    DateTime Date,
    string? Note,
    List<ExpenseSplitInputDto>? Splits);

public record ExpenseSplitInputDto(Guid UserId, decimal? Amount);
