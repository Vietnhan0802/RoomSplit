using RoomSplit.Core.Enums;

namespace RoomSplit.Core.Entities;

public class SharedExpense : BaseEntity
{
    public Guid RoomId { get; set; }
    public Guid PaidByUserId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public SharedExpenseCategory Category { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public SplitType SplitType { get; set; } = SplitType.Equal;
    public string? ReceiptImageUrl { get; set; }
    public string? Note { get; set; }

    // Navigation properties
    public Room Room { get; set; } = null!;
    public User PaidBy { get; set; } = null!;
    public ICollection<ExpenseSplit> Splits { get; set; } = new List<ExpenseSplit>();
}
