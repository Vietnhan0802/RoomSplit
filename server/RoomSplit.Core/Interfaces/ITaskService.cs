using Microsoft.AspNetCore.Http;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;

namespace RoomSplit.Core.Interfaces;

public interface ITaskService
{
    // Task generation
    Task GenerateAssignmentsForTemplateAsync(Guid taskTemplateId, DateTime startDate, DateTime endDate);
    Task RegenerateAssignmentsForTemplateAsync(Guid taskTemplateId);
    Task GenerateAllActiveTemplatesAsync();

    // Rotation logic
    Task AdvanceRotationAsync(Guid taskTemplateId);
    Guid GetNextAssigneeUserId(TaskTemplate template);

    // Assignment operations
    Task<TaskAssignment> CompleteAssignmentAsync(Guid assignmentId, Guid userId, string? note, IFormFile? proofImage);
    Task<TaskAssignment> SkipAssignmentAsync(Guid assignmentId, Guid userId, string? reason);
    Task<TaskAssignment> SwapAssignmentAsync(Guid assignmentId, Guid fromUserId, Guid toUserId);

    // Overdue management
    Task MarkOverdueAssignmentsAsync();

    // Validation
    Task<bool> ValidateRotationOrderAsync(Guid roomId, List<Guid> userIds);
}
