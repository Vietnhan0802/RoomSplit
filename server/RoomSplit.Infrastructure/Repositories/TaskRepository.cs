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

    public async Task<IEnumerable<TaskTemplate>> GetActiveTemplatesAsync()
    {
        return await _dbSet
            .Where(t => t.IsActive)
            .Include(t => t.CreatedBy)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskAssignment>> GetAssignmentsByDateRangeAsync(
        Guid roomId,
        DateTime startDate,
        DateTime endDate,
        Guid? assignedToUserId = null,
        TaskCompletionStatus? status = null)
    {
        var query = _context.TaskAssignments
            .Where(a => a.RoomId == roomId && a.DueDate >= startDate && a.DueDate <= endDate);

        if (assignedToUserId.HasValue)
            query = query.Where(a => a.AssignedToUserId == assignedToUserId.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .Include(a => a.TaskTemplate)
            .Include(a => a.AssignedTo)
            .Include(a => a.CompletedBy)
            .OrderBy(a => a.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskAssignment>> GetTodayAssignmentsAsync(Guid roomId, DateTime today)
    {
        var startOfDay = today.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        return await _context.TaskAssignments
            .Where(a => a.RoomId == roomId && a.DueDate >= startOfDay && a.DueDate <= endOfDay)
            .Include(a => a.TaskTemplate)
            .Include(a => a.AssignedTo)
            .Include(a => a.CompletedBy)
            .OrderBy(a => a.Status)
            .ThenBy(a => a.AssignedTo!.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskAssignment>> GetMyAssignmentsAsync(Guid roomId, Guid userId)
    {
        return await _context.TaskAssignments
            .Where(a => a.RoomId == roomId && a.AssignedToUserId == userId)
            .Include(a => a.TaskTemplate)
            .Include(a => a.AssignedTo)
            .OrderBy(a => a.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskAssignment>> GetPendingOverdueAsync(DateTime currentDate)
    {
        return await _context.TaskAssignments
            .Where(a => a.Status == TaskCompletionStatus.Pending && a.DueDate < currentDate)
            .Include(a => a.TaskTemplate)
            .Include(a => a.AssignedTo)
            .ToListAsync();
    }

    public async Task DeleteFutureAssignmentsAsync(Guid taskTemplateId, DateTime fromDate)
    {
        var futureAssignments = await _context.TaskAssignments
            .Where(a => a.TaskTemplateId == taskTemplateId && a.DueDate >= fromDate)
            .ToListAsync();

        _context.TaskAssignments.RemoveRange(futureAssignments);
    }
}
