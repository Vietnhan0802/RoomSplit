using Microsoft.EntityFrameworkCore;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Interfaces;
using RoomSplit.Infrastructure.Data;

namespace RoomSplit.Infrastructure.Repositories;

public class SharedExpenseRepository : Repository<SharedExpense>, ISharedExpenseRepository
{
    public SharedExpenseRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<SharedExpense>> GetByRoomIdAsync(Guid roomId)
    {
        return await _dbSet
            .Where(e => e.RoomId == roomId)
            .Include(e => e.PaidBy)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<SharedExpense?> GetWithSplitsAsync(Guid expenseId)
    {
        return await _dbSet
            .Include(e => e.Splits)
                .ThenInclude(s => s.User)
            .Include(e => e.PaidBy)
            .FirstOrDefaultAsync(e => e.Id == expenseId);
    }

    public async Task<IEnumerable<SharedExpense>> GetByRoomIdWithSplitsAsync(Guid roomId)
    {
        return await _dbSet
            .Where(e => e.RoomId == roomId)
            .Include(e => e.Splits)
                .ThenInclude(s => s.User)
            .Include(e => e.PaidBy)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalByRoomAsync(Guid roomId)
    {
        return await _dbSet
            .Where(e => e.RoomId == roomId)
            .SumAsync(e => e.Amount);
    }
}
