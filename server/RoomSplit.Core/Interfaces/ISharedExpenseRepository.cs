using RoomSplit.Core.Entities;

namespace RoomSplit.Core.Interfaces;

public interface ISharedExpenseRepository : IRepository<SharedExpense>
{
    Task<IEnumerable<SharedExpense>> GetByRoomIdAsync(Guid roomId);
    Task<SharedExpense?> GetWithSplitsAsync(Guid expenseId);
    Task<IEnumerable<SharedExpense>> GetByRoomIdWithSplitsAsync(Guid roomId);
    Task<decimal> GetTotalByRoomAsync(Guid roomId);
}
