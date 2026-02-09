using RoomSplit.Core.Enums;

namespace RoomSplit.Core.Entities;

public class TaskAssignment : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaskTemplateId { get; set; }
    public Guid RoomId { get; set; }
    public Guid AssignedToUserId { get; set; }
    public DateTime DueDate { get; set; }
    public TaskCompletionStatus Status { get; set; } = TaskCompletionStatus.Pending;
    public DateTime? CompletedAt { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public string? Note { get; set; }
    public string? ProofImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public TaskTemplate TaskTemplate { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public User AssignedTo { get; set; } = null!;
    public User? CompletedBy { get; set; }
}
