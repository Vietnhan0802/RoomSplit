namespace RoomSplit.Core.Entities;

public class ExpenseSplit : BaseEntity
{
    public Guid ExpenseId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public decimal? Percentage { get; set; }
    public bool IsPaid { get; set; } = false;

    // Navigation properties
    public Expense Expense { get; set; } = null!;
    public User User { get; set; } = null!;
}
