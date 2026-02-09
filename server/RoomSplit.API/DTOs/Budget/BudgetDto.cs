namespace RoomSplit.API.DTOs.Budget;

public record BudgetDto(
    Guid Id,
    string Category,
    decimal LimitAmount,
    decimal SpentAmount,
    int Month,
    int Year);
