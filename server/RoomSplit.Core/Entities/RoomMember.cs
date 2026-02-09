using RoomSplit.Core.Enums;

namespace RoomSplit.Core.Entities;

public class RoomMember : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RoomId { get; set; }
    public Guid UserId { get; set; }
    public RoomRole Role { get; set; } = RoomRole.Member;
    public string? NickName { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Room Room { get; set; } = null!;
    public User User { get; set; } = null!;
}
