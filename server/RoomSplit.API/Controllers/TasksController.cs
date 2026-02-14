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
    private readonly ITaskService _taskService;

    public TasksController(IUnitOfWork unitOfWork, IMapper mapper, ITaskService taskService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _taskService = taskService;
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

        // Validate rotation order
        if (!await _taskService.ValidateRotationOrderAsync(roomId, dto.RotationOrder))
            return BadRequest(ApiResponse.Fail("Invalid rotation order: all users must be active members of the room."));

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

        // Generate assignments for the next 30 days
        var endDate = task.StartDate.AddDays(30);
        await _taskService.GenerateAssignmentsForTemplateAsync(task.Id, task.StartDate, endDate);

        return CreatedAtAction(nameof(GetTasks), new { roomId },
            ApiResponse<TaskDto>.Ok(_mapper.Map<TaskDto>(task)));
    }

    [HttpPut("{taskId}")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> UpdateTask(Guid roomId, Guid taskId, UpdateTaskTemplateDto dto)
    {
        var task = await _unitOfWork.TaskTemplates.GetByIdAsync(taskId);
        if (task == null || task.RoomId != roomId)
            return NotFound(ApiResponse.Fail("Task template not found."));

        // Validate rotation order
        if (!await _taskService.ValidateRotationOrderAsync(roomId, dto.RotationOrder))
            return BadRequest(ApiResponse.Fail("Invalid rotation order: all users must be active members of the room."));

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Icon = dto.Icon;
        task.FrequencyType = (TaskFrequency)dto.FrequencyType;
        task.FrequencyValue = dto.FrequencyValue;
        task.RotationOrder = JsonSerializer.Serialize(dto.RotationOrder);
        task.StartDate = dto.StartDate;
        task.IsActive = dto.IsActive;

        await _unitOfWork.TaskTemplates.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        // Regenerate assignments from today
        await _taskService.RegenerateAssignmentsForTemplateAsync(taskId);

        return Ok(ApiResponse<TaskDto>.Ok(_mapper.Map<TaskDto>(task)));
    }

    [HttpDelete("{taskId}")]
    public async Task<ActionResult<ApiResponse>> DeleteTask(Guid roomId, Guid taskId)
    {
        var task = await _unitOfWork.TaskTemplates.GetByIdAsync(taskId);
        if (task == null || task.RoomId != roomId)
            return NotFound(ApiResponse.Fail("Task template not found."));

        // Delete future assignments
        await _unitOfWork.TaskTemplates.DeleteFutureAssignmentsAsync(taskId, DateTime.UtcNow.Date);

        await _unitOfWork.TaskTemplates.DeleteAsync(task);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Task template deleted."));
    }

    [HttpPost("{taskId}/pause")]
    public async Task<ActionResult<ApiResponse>> PauseTask(Guid roomId, Guid taskId)
    {
        var task = await _unitOfWork.TaskTemplates.GetByIdAsync(taskId);
        if (task == null || task.RoomId != roomId)
            return NotFound(ApiResponse.Fail("Task template not found."));

        task.IsActive = false;
        await _unitOfWork.TaskTemplates.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Task template paused."));
    }

    [HttpPost("{taskId}/resume")]
    public async Task<ActionResult<ApiResponse>> ResumeTask(Guid roomId, Guid taskId)
    {
        var task = await _unitOfWork.TaskTemplates.GetByIdAsync(taskId);
        if (task == null || task.RoomId != roomId)
            return NotFound(ApiResponse.Fail("Task template not found."));

        task.IsActive = true;
        await _unitOfWork.TaskTemplates.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        // Regenerate assignments
        await _taskService.RegenerateAssignmentsForTemplateAsync(taskId);

        return Ok(ApiResponse.Ok("Task template resumed."));
    }

    [HttpGet("assignments")]
    public async Task<ActionResult<ApiResponse<List<TaskAssignmentDto>>>> GetAssignments(
        Guid roomId,
        [FromQuery] TaskAssignmentFilterDto filter)
    {
        var startDate = filter.StartDate ?? DateTime.UtcNow.Date.AddDays(-7);
        var endDate = filter.EndDate ?? DateTime.UtcNow.Date.AddDays(30);
        var status = filter.Status.HasValue ? (TaskCompletionStatus?)filter.Status.Value : null;

        var assignments = await _unitOfWork.TaskTemplates.GetAssignmentsByDateRangeAsync(
            roomId, startDate, endDate, filter.AssignedToUserId, status);

        return Ok(ApiResponse<List<TaskAssignmentDto>>.Ok(_mapper.Map<List<TaskAssignmentDto>>(assignments)));
    }

    [HttpGet("assignments/today")]
    public async Task<ActionResult<ApiResponse<List<TaskAssignmentDto>>>> GetTodayAssignments(Guid roomId)
    {
        var today = DateTime.UtcNow.Date;
        var assignments = await _unitOfWork.TaskTemplates.GetTodayAssignmentsAsync(roomId, today);
        return Ok(ApiResponse<List<TaskAssignmentDto>>.Ok(_mapper.Map<List<TaskAssignmentDto>>(assignments)));
    }

    [HttpGet("assignments/my")]
    public async Task<ActionResult<ApiResponse<List<TaskAssignmentDto>>>> GetMyAssignments(Guid roomId)
    {
        var userId = GetUserId();
        var assignments = await _unitOfWork.TaskTemplates.GetMyAssignmentsAsync(roomId, userId);
        return Ok(ApiResponse<List<TaskAssignmentDto>>.Ok(_mapper.Map<List<TaskAssignmentDto>>(assignments)));
    }

    [HttpPut("assignments/{assignmentId}/complete")]
    public async Task<ActionResult<ApiResponse<TaskAssignmentDto>>> CompleteAssignment(
        Guid roomId,
        Guid assignmentId,
        [FromForm] CompleteTaskAssignmentDto dto)
    {
        try
        {
            var userId = GetUserId();
            var assignment = await _taskService.CompleteAssignmentAsync(assignmentId, userId, dto.Note, dto.ProofImage);
            return Ok(ApiResponse<TaskAssignmentDto>.Ok(_mapper.Map<TaskAssignmentDto>(assignment)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("assignments/{assignmentId}/skip")]
    public async Task<ActionResult<ApiResponse<TaskAssignmentDto>>> SkipAssignment(
        Guid roomId,
        Guid assignmentId,
        [FromBody] SkipTaskAssignmentDto dto)
    {
        try
        {
            var userId = GetUserId();
            var assignment = await _taskService.SkipAssignmentAsync(assignmentId, userId, dto.Reason);
            return Ok(ApiResponse<TaskAssignmentDto>.Ok(_mapper.Map<TaskAssignmentDto>(assignment)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost("assignments/{assignmentId}/swap")]
    public async Task<ActionResult<ApiResponse<TaskAssignmentDto>>> SwapAssignment(
        Guid roomId,
        Guid assignmentId,
        [FromBody] SwapTaskAssignmentDto dto)
    {
        try
        {
            var userId = GetUserId();
            var assignment = await _taskService.SwapAssignmentAsync(assignmentId, userId, dto.ToUserId);
            return Ok(ApiResponse<TaskAssignmentDto>.Ok(_mapper.Map<TaskAssignmentDto>(assignment)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}
