using RoomSplit.Core.Enums;

namespace RoomSplit.Core.Entities;

public class Expense : BaseEntity
{
    public Guid RoomId { get; set; }
    public Guid PaidByUserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; }
    public SplitType SplitType { get; set; } = SplitType.Equal;
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public string? ReceiptUrl { get; set; }
    public string? Note { get; set; }
    public bool IsSettled { get; set; } = false;

    // Navigation properties
    public Room Room { get; set; } = null!;
    public User PaidBy { get; set; } = null!;
    public ICollection<ExpenseSplit> Splits { get; set; } = new List<ExpenseSplit>();
}
