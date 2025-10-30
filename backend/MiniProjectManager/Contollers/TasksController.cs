using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniProjectManager.DTOs;
using MiniProjectManager.Services;
using System.Security.Claims;

namespace MiniProjectManager.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ISchedulerService _schedulerService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(
        ITaskService taskService,
        ISchedulerService schedulerService,
        ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _schedulerService = schedulerService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    [HttpGet("projects/{projectId}/tasks")]
    public async Task<IActionResult> GetTasksByProject(int projectId)
    {
        try
        {
            var userId = GetUserId();
            var tasks = await _taskService.GetTasksByProjectAsync(projectId, userId);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return StatusCode(500, new { message = "An error occurred while retrieving tasks" });
        }
    }

    [HttpPost("projects/{projectId}/tasks")]
    public async Task<IActionResult> CreateTask(int projectId, [FromBody] CreateTaskDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var task = await _taskService.CreateTaskAsync(projectId, dto, userId);

            if (task == null)
                return NotFound(new { message = "Project not found" });

            return CreatedAtAction(nameof(GetTasksByProject), new { projectId }, task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, new { message = "An error occurred while creating the task" });
        }
    }

    [HttpPut("tasks/{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var task = await _taskService.UpdateTaskAsync(id, dto, userId);

            if (task == null)
                return NotFound(new { message = "Task not found" });

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task");
            return StatusCode(500, new { message = "An error occurred while updating the task" });
        }
    }

    [HttpPatch("tasks/{id}/toggle")]
    public async Task<IActionResult> ToggleTaskCompletion(int id)
    {
        try
        {
            var userId = GetUserId();
            var task = await _taskService.ToggleTaskCompletionAsync(id, userId);

            if (task == null)
                return NotFound(new { message = "Task not found" });

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling task completion");
            return StatusCode(500, new { message = "An error occurred while updating the task" });
        }
    }

    [HttpDelete("tasks/{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _taskService.DeleteTaskAsync(id, userId);

            if (!result)
                return NotFound(new { message = "Task not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task");
            return StatusCode(500, new { message = "An error occurred while deleting the task" });
        }
    }

    [HttpPost("projects/{projectId}/schedule")]
    public async Task<IActionResult> GenerateSchedule(int projectId, [FromBody] ScheduleRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var schedule = await _schedulerService.GenerateScheduleAsync(projectId, dto, userId);

            if (schedule == null)
                return NotFound(new { message = "Project not found" });

            return Ok(schedule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating schedule");
            return StatusCode(500, new { message = "An error occurred while generating the schedule" });
        }
    }
}