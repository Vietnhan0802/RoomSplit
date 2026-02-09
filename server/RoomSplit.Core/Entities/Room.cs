namespace RoomSplit.Core.Entities;

public class Room : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public User CreatedBy { get; set; } = null!;
    public ICollection<RoomMember> Members { get; set; } = new List<RoomMember>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<RoomTask> Tasks { get; set; } = new List<RoomTask>();
}
