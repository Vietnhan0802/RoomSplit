using RoomSplit.Core.Enums;

namespace RoomSplit.Core.Entities;

public class Budget : BaseEntity
{
    public Guid UserId { get; set; }
    public TransactionCategory Category { get; set; }
    public decimal LimitAmount { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
