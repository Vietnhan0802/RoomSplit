namespace RoomSplit.API.DTOs.Budget;

public record BudgetDto(
    Guid Id,
    string ExpenseCategory,
    decimal MonthlyLimit,
    decimal SpentAmount,
    int Month,
    int Year);
