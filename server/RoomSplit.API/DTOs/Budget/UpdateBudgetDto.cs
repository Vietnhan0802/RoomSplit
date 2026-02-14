namespace RoomSplit.API.DTOs.Budget;

public record UpdateBudgetDto(
    int ExpenseCategory,
    decimal MonthlyLimit);
