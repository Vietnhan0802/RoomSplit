using Microsoft.EntityFrameworkCore;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;
using RoomSplit.Core.Interfaces;
using RoomSplit.Infrastructure.Data;

namespace RoomSplit.Infrastructure.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByUserIdAndDateRangeAsync(Guid userId, DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && t.Date >= from && t.Date <= to)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByUserIdAndTypeAsync(Guid userId, TransactionType type)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && t.Type == type)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalByUserAndTypeAsync(Guid userId, TransactionType type, DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && t.Type == type && t.Date >= from && t.Date <= to)
            .SumAsync(t => t.Amount);
    }

    public async Task<Transaction?> GetWithImagesAsync(Guid transactionId)
    {
        return await _dbSet
            .Include(t => t.Images)
            .FirstOrDefaultAsync(t => t.Id == transactionId);
    }
}
