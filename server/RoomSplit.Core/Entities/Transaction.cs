using RoomSplit.Core.Enums;

namespace RoomSplit.Core.Entities;

public class Transaction : BaseEntity
{
    public Guid UserId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public TransactionCategory Category { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
