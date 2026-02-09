namespace RoomSplit.API.DTOs.Budget;

public record CreateBudgetDto(int Category, decimal LimitAmount, int Month, int Year);
