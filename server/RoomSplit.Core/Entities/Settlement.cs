namespace RoomSplit.Core.Entities;

public class Settlement : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RoomId { get; set; }
    public Guid PayerUserId { get; set; }
    public Guid PayeeUserId { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Room Room { get; set; } = null!;
    public User Payer { get; set; } = null!;
    public User Payee { get; set; } = null!;
}
