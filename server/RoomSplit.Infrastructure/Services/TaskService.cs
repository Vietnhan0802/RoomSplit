using System.Text.Json;
using Microsoft.AspNetCore.Http;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;
using RoomSplit.Core.Interfaces;

namespace RoomSplit.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;

    public TaskService(IUnitOfWork unitOfWork, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
    }

    public async Task GenerateAssignmentsForTemplateAsync(Guid taskTemplateId, DateTime startDate, DateTime endDate)
    {
        var template = await _unitOfWork.TaskTemplates.GetByIdAsync(taskTemplateId);
        if (template == null || !template.IsActive)
            return;

        // Parse rotation order
        var rotationOrder = JsonSerializer.Deserialize<List<Guid>>(template.RotationOrder);
        if (rotationOrder == null || rotationOrder.Count == 0)
            return;

        // Delete existing future assignments to avoid duplicates
        await _unitOfWork.TaskTemplates.DeleteFutureAssignmentsAsync(taskTemplateId, startDate);

        var assignments = new List<TaskAssignment>();
        var currentDate = startDate.Date;
        var currentIndex = template.CurrentAssigneeIndex;

        while (currentDate <= endDate)
        {
            // Get assignee from rotation order
            var assigneeId = rotationOrder[currentIndex % rotationOrder.Count];

            assignments.Add(new TaskAssignment
            {
                TaskTemplateId = taskTemplateId,
                RoomId = template.RoomId,
                AssignedToUserId = assigneeId,
                DueDate = currentDate,
                Status = TaskCompletionStatus.Pending
            });

            // Calculate next due date
            currentDate = CalculateNextDueDate(template, currentDate);

            // Don't advance index yet - only advance on completion
        }

        await _unitOfWork.TaskAssignments.AddRangeAsync(assignments);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RegenerateAssignmentsForTemplateAsync(Guid taskTemplateId)
    {
        var template = await _unitOfWork.TaskTemplates.GetByIdAsync(taskTemplateId);
        if (template == null)
            return;

        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(30);
        await GenerateAssignmentsForTemplateAsync(taskTemplateId, startDate, endDate);
    }

    public async Task GenerateAllActiveTemplatesAsync()
    {
        var templates = await _unitOfWork.TaskTemplates.GetActiveTemplatesAsync();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(30);

        foreach (var template in templates)
        {
            await GenerateAssignmentsForTemplateAsync(template.Id, startDate, endDate);
        }
    }

    public async Task AdvanceRotationAsync(Guid taskTemplateId)
    {
        var template = await _unitOfWork.TaskTemplates.GetByIdAsync(taskTemplateId);
        if (template == null)
            return;

        var rotationOrder = JsonSerializer.Deserialize<List<Guid>>(template.RotationOrder);
        if (rotationOrder == null || rotationOrder.Count == 0)
            return;

        template.CurrentAssigneeIndex = (template.CurrentAssigneeIndex + 1) % rotationOrder.Count;
        await _unitOfWork.TaskTemplates.UpdateAsync(template);
        await _unitOfWork.SaveChangesAsync();

        // Generate next assignment for the new assignee
        var nextDueDate = CalculateNextDueDate(template, DateTime.UtcNow.Date);
        var nextAssigneeId = rotationOrder[template.CurrentAssigneeIndex];

        var nextAssignment = new TaskAssignment
        {
            TaskTemplateId = taskTemplateId,
            RoomId = template.RoomId,
            AssignedToUserId = nextAssigneeId,
            DueDate = nextDueDate,
            Status = TaskCompletionStatus.Pending
        };

        await _unitOfWork.TaskAssignments.AddAsync(nextAssignment);
        await _unitOfWork.SaveChangesAsync();
    }

    public Guid GetNextAssigneeUserId(TaskTemplate template)
    {
        var rotationOrder = JsonSerializer.Deserialize<List<Guid>>(template.RotationOrder);
        if (rotationOrder == null || rotationOrder.Count == 0)
            throw new InvalidOperationException("Rotation order is empty");

        return rotationOrder[template.CurrentAssigneeIndex % rotationOrder.Count];
    }

    public async Task<TaskAssignment> CompleteAssignmentAsync(Guid assignmentId, Guid userId, string? note, IFormFile? proofImage)
    {
        var assignment = await _unitOfWork.TaskAssignments.GetByIdAsync(assignmentId);
        if (assignment == null)
            throw new KeyNotFoundException("Assignment not found");

        if (assignment.AssignedToUserId != userId)
            throw new UnauthorizedAccessException("You are not assigned to this task");

        if (assignment.Status != TaskCompletionStatus.Pending)
            throw new InvalidOperationException("Assignment is not pending");

        // Upload proof image if provided
        if (proofImage != null)
        {
            assignment.ProofImageUrl = await _fileService.UploadFileAsync(proofImage, "tasks");
        }

        assignment.Status = TaskCompletionStatus.Completed;
        assignment.CompletedAt = DateTime.UtcNow;
        assignment.CompletedByUserId = userId;
        assignment.Note = note;

        await _unitOfWork.TaskAssignments.UpdateAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        // Advance rotation
        await AdvanceRotationAsync(assignment.TaskTemplateId);

        return assignment;
    }

    public async Task<TaskAssignment> SkipAssignmentAsync(Guid assignmentId, Guid userId, string? reason)
    {
        var assignment = await _unitOfWork.TaskAssignments.GetByIdAsync(assignmentId);
        if (assignment == null)
            throw new KeyNotFoundException("Assignment not found");

        if (assignment.AssignedToUserId != userId)
            throw new UnauthorizedAccessException("You are not assigned to this task");

        if (assignment.Status != TaskCompletionStatus.Pending)
            throw new InvalidOperationException("Assignment is not pending");

        assignment.Status = TaskCompletionStatus.Skipped;
        assignment.Note = reason;
        await _unitOfWork.TaskAssignments.UpdateAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        // Advance rotation to next person
        await AdvanceRotationAsync(assignment.TaskTemplateId);

        return assignment;
    }

    public async Task<TaskAssignment> SwapAssignmentAsync(Guid assignmentId, Guid fromUserId, Guid toUserId)
    {
        var assignment = await _unitOfWork.TaskAssignments.GetByIdAsync(assignmentId);
        if (assignment == null)
            throw new KeyNotFoundException("Assignment not found");

        if (assignment.AssignedToUserId != fromUserId)
            throw new UnauthorizedAccessException("You are not assigned to this task");

        if (assignment.Status != TaskCompletionStatus.Pending)
            throw new InvalidOperationException("Assignment is not pending");

        // Swap assignee (no rotation advance)
        assignment.AssignedToUserId = toUserId;
        await _unitOfWork.TaskAssignments.UpdateAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        return assignment;
    }

    public async Task MarkOverdueAssignmentsAsync()
    {
        var overdueAssignments = await _unitOfWork.TaskTemplates.GetPendingOverdueAsync(DateTime.UtcNow);

        foreach (var assignment in overdueAssignments)
        {
            assignment.Status = TaskCompletionStatus.Overdue;
            await _unitOfWork.TaskAssignments.UpdateAsync(assignment);
        }

        if (overdueAssignments.Any())
        {
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<bool> ValidateRotationOrderAsync(Guid roomId, List<Guid> userIds)
    {
        if (userIds == null || userIds.Count == 0)
            return false;

        // Verify all users are members of the room
        foreach (var userId in userIds)
        {
            var members = await _unitOfWork.RoomMembers.FindAsync(
                rm => rm.RoomId == roomId && rm.UserId == userId && rm.IsActive);

            if (!members.Any())
                return false;
        }

        return true;
    }

    private DateTime CalculateNextDueDate(TaskTemplate template, DateTime fromDate)
    {
        return template.FrequencyType switch
        {
            TaskFrequency.Daily => fromDate.AddDays(1),
            TaskFrequency.EveryNDays => fromDate.AddDays(template.FrequencyValue),
            TaskFrequency.Weekly => GetNextWeekday(fromDate, (DayOfWeek)template.FrequencyValue),
            TaskFrequency.BiWeekly => GetNextWeekday(fromDate.AddDays(14), (DayOfWeek)template.FrequencyValue),
            TaskFrequency.Monthly => GetNextMonthDate(fromDate, template.FrequencyValue),
            _ => throw new ArgumentException($"Invalid frequency type: {template.FrequencyType}")
        };
    }

    private DateTime GetNextWeekday(DateTime fromDate, DayOfWeek targetDay)
    {
        var daysToAdd = ((int)targetDay - (int)fromDate.DayOfWeek + 7) % 7;
        if (daysToAdd == 0)
            daysToAdd = 7; // Next week if same day
        return fromDate.AddDays(daysToAdd);
    }

    private DateTime GetNextMonthDate(DateTime fromDate, int dayOfMonth)
    {
        var nextMonth = fromDate.AddMonths(1);
        var daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
        var targetDay = Math.Min(dayOfMonth, daysInMonth);
        return new DateTime(nextMonth.Year, nextMonth.Month, targetDay);
    }
}
