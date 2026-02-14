using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;

namespace RoomSplit.Core.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Transaction>> GetByUserIdAndDateRangeAsync(Guid userId, DateTime from, DateTime to);
    Task<IEnumerable<Transaction>> GetByUserIdAndTypeAsync(Guid userId, TransactionType type);
    Task<decimal> GetTotalByUserAndTypeAsync(Guid userId, TransactionType type, DateTime from, DateTime to);
    Task<Transaction?> GetWithImagesAsync(Guid transactionId);

    Task<(IEnumerable<Transaction> Items, int TotalCount)> GetPaginatedAsync(
        Guid userId,
        TransactionType? type,
        DateTime? startDate,
        DateTime? endDate,
        ExpenseCategory? expenseCategory,
        IncomeCategory? incomeCategory,
        int page,
        int pageSize,
        string sortBy,
        string order);

    Task<IEnumerable<Transaction>> GetByUserIdAndDateRangeWithImagesAsync(
        Guid userId, DateTime from, DateTime to);
}
