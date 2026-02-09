using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;

namespace RoomSplit.Core.Interfaces;

public interface ITaskRepository : IRepository<TaskTemplate>
{
    Task<IEnumerable<TaskTemplate>> GetByRoomIdAsync(Guid roomId);
    Task<TaskTemplate?> GetWithAssignmentsAsync(Guid taskTemplateId);
    Task<IEnumerable<TaskAssignment>> GetAssignmentsByUserAsync(Guid userId);
    Task<IEnumerable<TaskAssignment>> GetAssignmentsByRoomAsync(Guid roomId);
    Task<IEnumerable<TaskAssignment>> GetAssignmentsByStatusAsync(Guid roomId, TaskCompletionStatus status);
}
