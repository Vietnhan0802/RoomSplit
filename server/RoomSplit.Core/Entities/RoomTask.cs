using RoomSplit.Core.Enums;

namespace RoomSplit.Core.Entities;

public class RoomTask : BaseEntity
{
    public Guid RoomId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskFrequency Frequency { get; set; } = TaskFrequency.Weekly;
    public bool IsRotating { get; set; } = true;
    public int CurrentRotationIndex { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Room Room { get; set; } = null!;
    public ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
}
