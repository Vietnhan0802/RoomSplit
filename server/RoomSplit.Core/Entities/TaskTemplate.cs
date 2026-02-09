using RoomSplit.Core.Enums;

namespace RoomSplit.Core.Entities;

public class TaskTemplate : BaseEntity
{
    public Guid RoomId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public TaskFrequency FrequencyType { get; set; } = TaskFrequency.Weekly;
    public int FrequencyValue { get; set; } = 1;
    public string RotationOrder { get; set; } = "[]"; // JSON array of UserIds
    public int CurrentAssigneeIndex { get; set; } = 0;
    public DateTime StartDate { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CreatedByUserId { get; set; }

    // Navigation properties
    public Room Room { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
}
