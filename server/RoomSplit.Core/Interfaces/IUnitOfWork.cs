using RoomSplit.Core.Entities;

namespace RoomSplit.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Room> Rooms { get; }
    IRepository<RoomMember> RoomMembers { get; }
    IRepository<Expense> Expenses { get; }
    IRepository<ExpenseSplit> ExpenseSplits { get; }
    IRepository<RoomTask> RoomTasks { get; }
    IRepository<TaskAssignment> TaskAssignments { get; }
    IRepository<Transaction> Transactions { get; }
    IRepository<Budget> Budgets { get; }
    Task<int> SaveChangesAsync();
}
