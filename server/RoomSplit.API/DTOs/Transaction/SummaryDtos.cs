namespace RoomSplit.API.DTOs.Transaction;

public record SummaryResponseDto(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetAmount,
    List<CategorySummaryDto> ByCategory,
    List<DailyTrendDto> DailyTrend,
    List<TransactionDto> TopExpenses,
    ComparisonDto ComparedToLastMonth);

public record CategorySummaryDto(
    string Category,
    decimal Amount,
    int Count,
    decimal Percentage);

public record DailyTrendDto(
    DateTime Date,
    decimal Income,
    decimal Expense);

public record ComparisonDto(
    decimal IncomeChangePercent,
    decimal ExpenseChangePercent);
