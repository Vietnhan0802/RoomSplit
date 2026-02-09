namespace RoomSplit.API.DTOs.Budget;

public record CreateBudgetDto(int ExpenseCategory, decimal MonthlyLimit, int Month, int Year);
