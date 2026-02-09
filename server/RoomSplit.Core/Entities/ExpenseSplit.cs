namespace RoomSplit.Core.Entities;

public class ExpenseSplit : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SharedExpenseId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public bool IsSettled { get; set; } = false;
    public DateTime? SettledAt { get; set; }

    // Navigation properties
    public SharedExpense SharedExpense { get; set; } = null!;
    public User User { get; set; } = null!;
}
