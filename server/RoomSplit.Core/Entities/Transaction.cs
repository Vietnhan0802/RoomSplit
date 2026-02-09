using RoomSplit.Core.Enums;

namespace RoomSplit.Core.Entities;

public class Transaction : BaseEntity
{
    public Guid UserId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public IncomeCategory? IncomeCategory { get; set; }
    public ExpenseCategory? ExpenseCategory { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? ImageUrl { get; set; }
    public string? Note { get; set; }
    public string? Tags { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<TransactionImage> Images { get; set; } = new List<TransactionImage>();
}
