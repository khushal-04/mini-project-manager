using Microsoft.EntityFrameworkCore;
using MiniProjectManager.Data;
using MiniProjectManager.DTOs;
using MiniProjectManager.Models;

namespace MiniProjectManager.Services;

public interface IProjectService
{
    Task<List<ProjectDto>> GetAllProjectsAsync(int userId);
    Task<ProjectDetailDto?> GetProjectByIdAsync(int projectId, int userId);
    Task<ProjectDto?> CreateProjectAsync(CreateProjectDto dto, int userId);
    Task<ProjectDto?> UpdateProjectAsync(int projectId, UpdateProjectDto dto, int userId);
    Task<bool> DeleteProjectAsync(int projectId, int userId);
}

public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;

    public ProjectService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectDto>> GetAllProjectsAsync(int userId)
    {
        return await _context.Projects
            .Where(p => p.UserId == userId)
            .Include(p => p.Tasks)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                TaskCount = p.Tasks.Count,
                CompletedTaskCount = p.Tasks.Count(t => t.IsCompleted)
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProjectDetailDto?> GetProjectByIdAsync(int projectId, int userId)
    {
        var project = await _context.Projects
            .Where(p => p.Id == projectId && p.UserId == userId)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync();

        if (project == null)
            return null;

        return new ProjectDetailDto
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            Tasks = project.Tasks.Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                ProjectId = t.ProjectId
            }).OrderBy(t => t.IsCompleted).ThenBy(t => t.DueDate).ToList()
        };
    }

    public async Task<ProjectDto?> CreateProjectAsync(CreateProjectDto dto, int userId)
    {
        var project = new Project
        {
            Title = dto.Title,
            Description = dto.Description,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return new ProjectDto
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            TaskCount = 0,
            CompletedTaskCount = 0
        };
    }

    public async Task<ProjectDto?> UpdateProjectAsync(int projectId, UpdateProjectDto dto, int userId)
    {
        var project = await _context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null)
            return null;

        project.Title = dto.Title;
        project.Description = dto.Description;

        await _context.SaveChangesAsync();

        return new ProjectDto
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            TaskCount = project.Tasks.Count,
            CompletedTaskCount = project.Tasks.Count(t => t.IsCompleted)
        };
    }

    public async Task<bool> DeleteProjectAsync(int projectId, int userId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null)
            return false;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return true;
    }
}