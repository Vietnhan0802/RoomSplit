using RoomSplit.Core.Entities;

namespace RoomSplit.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Room> Rooms { get; }
    IRepository<RoomMember> RoomMembers { get; }
    ISharedExpenseRepository SharedExpenses { get; }
    IRepository<ExpenseSplit> ExpenseSplits { get; }
    IRepository<Settlement> Settlements { get; }
    ITaskRepository TaskTemplates { get; }
    IRepository<TaskAssignment> TaskAssignments { get; }
    ITransactionRepository Transactions { get; }
    IRepository<Budget> Budgets { get; }
    IRepository<TransactionImage> TransactionImages { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync();
}
