using Microsoft.EntityFrameworkCore;
using MiniProjectManager.Data;
using MiniProjectManager.DTOs;
using MiniProjectManager.Models;

namespace MiniProjectManager.Services;

public interface ITaskService
{
    Task<TaskDto?> CreateTaskAsync(int projectId, CreateTaskDto dto, int userId);
    Task<TaskDto?> UpdateTaskAsync(int taskId, UpdateTaskDto dto, int userId);
    Task<TaskDto?> ToggleTaskCompletionAsync(int taskId, int userId);
    Task<bool> DeleteTaskAsync(int taskId, int userId);
    Task<List<TaskDto>> GetTasksByProjectAsync(int projectId, int userId);
}

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskDto?> CreateTaskAsync(int projectId, CreateTaskDto dto, int userId)
    {
        // Verify project belongs to user
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null)
            return null;

        var task = new ProjectTask
        {
            Title = dto.Title,
            DueDate = dto.DueDate,
            ProjectId = projectId,
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            ProjectId = task.ProjectId
        };
    }

    public async Task<TaskDto?> UpdateTaskAsync(int taskId, UpdateTaskDto dto, int userId)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.Project.UserId == userId);

        if (task == null)
            return null;

        task.Title = dto.Title;
        task.DueDate = dto.DueDate;
        task.IsCompleted = dto.IsCompleted;

        await _context.SaveChangesAsync();

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            ProjectId = task.ProjectId
        };
    }

    public async Task<TaskDto?> ToggleTaskCompletionAsync(int taskId, int userId)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.Project.UserId == userId);

        if (task == null)
            return null;

        task.IsCompleted = !task.IsCompleted;
        await _context.SaveChangesAsync();

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            ProjectId = task.ProjectId
        };
    }

    public async Task<bool> DeleteTaskAsync(int taskId, int userId)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.Project.UserId == userId);

        if (task == null)
            return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<TaskDto>> GetTasksByProjectAsync(int projectId, int userId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null)
            return new List<TaskDto>();

        return await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                ProjectId = t.ProjectId
            })
            .OrderBy(t => t.IsCompleted)
            .ThenBy(t => t.DueDate)
            .ToListAsync();
    }
}