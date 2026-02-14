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

    public async Task<(IEnumerable<Transaction> Items, int TotalCount)> GetPaginatedAsync(
        Guid userId,
        TransactionType? type,
        DateTime? startDate,
        DateTime? endDate,
        ExpenseCategory? expenseCategory,
        IncomeCategory? incomeCategory,
        int page,
        int pageSize,
        string sortBy,
        string order)
    {
        var query = _dbSet.Where(t => t.UserId == userId);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);

        if (expenseCategory.HasValue)
            query = query.Where(t => t.ExpenseCategory == expenseCategory.Value);

        if (incomeCategory.HasValue)
            query = query.Where(t => t.IncomeCategory == incomeCategory.Value);

        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "amount" => order.ToLower() == "asc" ? query.OrderBy(t => t.Amount) : query.OrderByDescending(t => t.Amount),
            "description" => order.ToLower() == "asc" ? query.OrderBy(t => t.Description) : query.OrderByDescending(t => t.Description),
            _ => order.ToLower() == "asc" ? query.OrderBy(t => t.Date) : query.OrderByDescending(t => t.Date),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(t => t.Images)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Transaction>> GetByUserIdAndDateRangeWithImagesAsync(
        Guid userId, DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && t.Date >= from && t.Date <= to)
            .Include(t => t.Images)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }
}
