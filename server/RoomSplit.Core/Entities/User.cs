namespace RoomSplit.Core.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    // Navigation properties
    public ICollection<RoomMember> RoomMembers { get; set; } = new List<RoomMember>();
    public ICollection<Room> CreatedRooms { get; set; } = new List<Room>();
    public ICollection<SharedExpense> PaidExpenses { get; set; } = new List<SharedExpense>();
    public ICollection<ExpenseSplit> ExpenseSplits { get; set; } = new List<ExpenseSplit>();
    public ICollection<Settlement> SentSettlements { get; set; } = new List<Settlement>();
    public ICollection<Settlement> ReceivedSettlements { get; set; } = new List<Settlement>();
    public ICollection<TaskTemplate> CreatedTaskTemplates { get; set; } = new List<TaskTemplate>();
    public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
    public ICollection<TaskAssignment> CompletedTaskAssignments { get; set; } = new List<TaskAssignment>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
