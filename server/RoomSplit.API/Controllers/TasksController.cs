using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomSplit.API.DTOs;
using RoomSplit.API.DTOs.Task;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;
using RoomSplit.Core.Interfaces;
using System.Security.Claims;

namespace RoomSplit.API.Controllers;

[Authorize]
[ApiController]
[Route("api/rooms/{roomId}/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TasksController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TaskDto>>>> GetTasks(Guid roomId)
    {
        var tasks = await _unitOfWork.TaskTemplates.GetByRoomIdAsync(roomId);
        return Ok(ApiResponse<List<TaskDto>>.Ok(_mapper.Map<List<TaskDto>>(tasks)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskDto>>> CreateTask(Guid roomId, CreateTaskDto dto)
    {
        var userId = GetUserId();
        var task = new TaskTemplate
        {
            RoomId = roomId,
            Title = dto.Title,
            Description = dto.Description,
            Icon = dto.Icon,
            FrequencyType = (TaskFrequency)dto.FrequencyType,
            FrequencyValue = dto.FrequencyValue,
            RotationOrder = JsonSerializer.Serialize(dto.RotationOrder),
            StartDate = dto.StartDate,
            CreatedByUserId = userId
        };

        await _unitOfWork.TaskTemplates.AddAsync(task);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTasks), new { roomId },
            ApiResponse<TaskDto>.Ok(_mapper.Map<TaskDto>(task)));
    }

    [HttpPost("{taskId}/complete")]
    public async Task<ActionResult<ApiResponse>> CompleteAssignment(Guid roomId, Guid taskId)
    {
        var userId = GetUserId();
        var assignments = await _unitOfWork.TaskAssignments.FindAsync(
            a => a.TaskTemplateId == taskId && a.AssignedToUserId == userId
                && a.Status == TaskCompletionStatus.Pending);

        var assignment = assignments.FirstOrDefault();
        if (assignment == null)
            return NotFound(ApiResponse.Fail("No pending assignment found."));

        assignment.Status = TaskCompletionStatus.Completed;
        assignment.CompletedAt = DateTime.UtcNow;
        assignment.CompletedByUserId = userId;
        await _unitOfWork.TaskAssignments.UpdateAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Task completed."));
    }
}
