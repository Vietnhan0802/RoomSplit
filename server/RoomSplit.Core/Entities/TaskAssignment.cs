using RoomSplit.Core.Enums;

namespace RoomSplit.Core.Entities;

public class TaskAssignment : BaseEntity
{
    public Guid RoomTaskId { get; set; }
    public Guid AssignedToUserId { get; set; }
    public DateTime DueDate { get; set; }
    public TaskCompletionStatus Status { get; set; } = TaskCompletionStatus.Pending;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public RoomTask RoomTask { get; set; } = null!;
    public User AssignedTo { get; set; } = null!;
}
