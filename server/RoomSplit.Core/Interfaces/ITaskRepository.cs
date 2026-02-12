using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;

namespace RoomSplit.Core.Interfaces;

public interface ITaskRepository : IRepository<TaskTemplate>
{
    // TaskTemplate queries
    Task<IEnumerable<TaskTemplate>> GetByRoomIdAsync(Guid roomId);
    Task<TaskTemplate?> GetWithAssignmentsAsync(Guid taskTemplateId);
    Task<IEnumerable<TaskTemplate>> GetActiveTemplatesAsync();

    // TaskAssignment queries
    Task<IEnumerable<TaskAssignment>> GetAssignmentsByUserAsync(Guid userId);
    Task<IEnumerable<TaskAssignment>> GetAssignmentsByRoomAsync(Guid roomId);
    Task<IEnumerable<TaskAssignment>> GetAssignmentsByStatusAsync(Guid roomId, TaskCompletionStatus status);
    Task<IEnumerable<TaskAssignment>> GetAssignmentsByDateRangeAsync(Guid roomId, DateTime startDate, DateTime endDate, Guid? assignedToUserId = null, TaskCompletionStatus? status = null);
    Task<IEnumerable<TaskAssignment>> GetTodayAssignmentsAsync(Guid roomId, DateTime today);
    Task<IEnumerable<TaskAssignment>> GetMyAssignmentsAsync(Guid roomId, Guid userId);
    Task<IEnumerable<TaskAssignment>> GetPendingOverdueAsync(DateTime currentDate);

    // TaskAssignment operations
    Task DeleteFutureAssignmentsAsync(Guid taskTemplateId, DateTime fromDate);
}
