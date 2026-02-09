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
    public IRepository<Expense> Expenses { get; }
    public IRepository<ExpenseSplit> ExpenseSplits { get; }
    public IRepository<RoomTask> RoomTasks { get; }
    public IRepository<TaskAssignment> TaskAssignments { get; }
    public IRepository<Transaction> Transactions { get; }
    public IRepository<Budget> Budgets { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new Repository<User>(context);
        Rooms = new Repository<Room>(context);
        RoomMembers = new Repository<RoomMember>(context);
        Expenses = new Repository<Expense>(context);
        ExpenseSplits = new Repository<ExpenseSplit>(context);
        RoomTasks = new Repository<RoomTask>(context);
        TaskAssignments = new Repository<TaskAssignment>(context);
        Transactions = new Repository<Transaction>(context);
        Budgets = new Repository<Budget>(context);
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
