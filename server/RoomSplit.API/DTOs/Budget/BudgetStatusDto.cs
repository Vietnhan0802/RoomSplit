namespace RoomSplit.API.DTOs.Budget;

public record BudgetStatusDto(
    Guid Id,
    string ExpenseCategory,
    decimal MonthlyLimit,
    decimal SpentAmount,
    int Month,
    int Year,
    decimal DailyAverageSpent,
    decimal ProjectedMonthEnd,
    bool IsOverBudget,
    decimal RemainingAmount,
    double PercentUsed);
