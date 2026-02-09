using Microsoft.EntityFrameworkCore;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;
using RoomSplit.Core.Interfaces;
using RoomSplit.Infrastructure.Data;

namespace RoomSplit.Infrastructure.Repositories;

public class TaskRepository : Repository<TaskTemplate>, ITaskRepository
{
    public TaskRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<TaskTemplate>> GetByRoomIdAsync(Guid roomId)
    {
        return await _dbSet
            .Where(t => t.RoomId == roomId && t.IsActive)
            .Include(t => t.CreatedBy)
            .OrderBy(t => t.Title)
            .ToListAsync();
    }

    public async Task<TaskTemplate?> GetWithAssignmentsAsync(Guid taskTemplateId)
    {
        return await _dbSet
            .Include(t => t.Assignments)
                .ThenInclude(a => a.AssignedTo)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == taskTemplateId);
    }

    public async Task<IEnumerable<TaskAssignment>> GetAssignmentsByUserAsync(Guid userId)
    {
        return await _context.TaskAssignments
            .Where(a => a.AssignedToUserId == userId)
            .Include(a => a.TaskTemplate)
            .Include(a => a.Room)
            .OrderByDescending(a => a.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskAssignment>> GetAssignmentsByRoomAsync(Guid roomId)
    {
        return await _context.TaskAssignments
            .Where(a => a.RoomId == roomId)
            .Include(a => a.TaskTemplate)
            .Include(a => a.AssignedTo)
            .OrderByDescending(a => a.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskAssignment>> GetAssignmentsByStatusAsync(Guid roomId, TaskCompletionStatus status)
    {
        return await _context.TaskAssignments
            .Where(a => a.RoomId == roomId && a.Status == status)
            .Include(a => a.TaskTemplate)
            .Include(a => a.AssignedTo)
            .OrderByDescending(a => a.DueDate)
            .ToListAsync();
    }
}
