namespace RoomSplit.Core.Entities;

public class Room : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string InviteCode { get; set; } = string.Empty;

    // Navigation properties
    public User CreatedBy { get; set; } = null!;
    public ICollection<RoomMember> Members { get; set; } = new List<RoomMember>();
    public ICollection<SharedExpense> SharedExpenses { get; set; } = new List<SharedExpense>();
    public ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
    public ICollection<TaskTemplate> TaskTemplates { get; set; } = new List<TaskTemplate>();
    public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
}
