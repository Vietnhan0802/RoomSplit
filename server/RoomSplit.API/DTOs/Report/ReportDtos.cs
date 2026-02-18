using RoomSplit.API.DTOs.Transaction;

namespace RoomSplit.API.DTOs.Report;

public record ReportOverviewDto(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetSavings,
    decimal SavingsPercent,
    decimal PrevMonthIncome,
    decimal PrevMonthExpense,
    decimal IncomeChangePercent,
    decimal ExpenseChangePercent,
    List<TransactionDto> TopExpenses);

public record CategoryBreakdownItemDto(
    string Category,
    decimal Amount,
    int Count,
    decimal Percentage,
    List<TransactionDto> Transactions);

public record MonthlyTrendItemDto(
    int Month,
    int Year,
    decimal Income,
    decimal Expense,
    decimal Savings);

public record DailySpendingItemDto(
    DateTime Date,
    decimal Amount);

public record DailySpendingResponseDto(
    List<DailySpendingItemDto> Days,
    decimal DailyAverage,
    DateTime? MaxDay,
    decimal MaxAmount);

public record CategoryComparisonItemDto(
    string Category,
    decimal PrevMonthAmount,
    decimal CurrentMonthAmount,
    decimal AmountChange,
    decimal PercentChange);
