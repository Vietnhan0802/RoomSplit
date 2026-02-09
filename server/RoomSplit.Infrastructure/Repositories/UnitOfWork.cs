using RoomSplit.Core.Entities;
using RoomSplit.Core.Interfaces;
using RoomSplit.Infrastructure.Data;

namespace RoomSplit.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private bool _disposed;

    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }
    public IRepository<RoomMember> RoomMembers { get; }
    public ISharedExpenseRepository SharedExpenses { get; }
    public IRepository<ExpenseSplit> ExpenseSplits { get; }
    public IRepository<Settlement> Settlements { get; }
    public ITaskRepository TaskTemplates { get; }
    public IRepository<TaskAssignment> TaskAssignments { get; }
    public ITransactionRepository Transactions { get; }
    public IRepository<Budget> Budgets { get; }
    public IRepository<TransactionImage> TransactionImages { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new Repository<User>(context);
        Rooms = new Repository<Room>(context);
        RoomMembers = new Repository<RoomMember>(context);
        SharedExpenses = new SharedExpenseRepository(context);
        ExpenseSplits = new Repository<ExpenseSplit>(context);
        Settlements = new Repository<Settlement>(context);
        TaskTemplates = new TaskRepository(context);
        TaskAssignments = new Repository<TaskAssignment>(context);
        Transactions = new TransactionRepository(context);
        Budgets = new Repository<Budget>(context);
        TransactionImages = new Repository<TransactionImage>(context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
