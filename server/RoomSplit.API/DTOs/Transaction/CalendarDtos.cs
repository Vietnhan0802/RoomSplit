namespace RoomSplit.API.DTOs.Transaction;

public record CalendarDayDto(
    DateTime Date,
    decimal TotalIncome,
    decimal TotalExpense,
    int TransactionCount,
    bool HasImages,
    List<TransactionDto> Transactions);

public record CalendarResponseDto(
    List<CalendarDayDto> Days,
    MonthSummaryDto MonthSummary);

public record MonthSummaryDto(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetAmount,
    int TransactionCount);
