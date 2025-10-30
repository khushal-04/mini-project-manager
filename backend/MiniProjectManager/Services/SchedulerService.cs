using Microsoft.EntityFrameworkCore;
using MiniProjectManager.Data;
using MiniProjectManager.DTOs;

namespace MiniProjectManager.Services;

public interface ISchedulerService
{
    Task<ScheduleResponseDto?> GenerateScheduleAsync(int projectId, ScheduleRequestDto dto, int userId);
}

public class SchedulerService : ISchedulerService
{
    private readonly AppDbContext _context;

    public SchedulerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ScheduleResponseDto?> GenerateScheduleAsync(int projectId, ScheduleRequestDto dto, int userId)
    {
        // Verify project belongs to user
        var project = await _context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null)
            return null;

        // Get incomplete tasks
        var incompleteTasks = project.Tasks
            .Where(t => !t.IsCompleted)
            .OrderBy(t => t.DueDate ?? DateTime.MaxValue)
            .ThenBy(t => t.CreatedAt)
            .ToList();

        if (!incompleteTasks.Any())
        {
            return new ScheduleResponseDto
            {
                ScheduledTasks = new List<ScheduledTaskDto>(),
                TotalEstimatedHours = 0,
                Message = "All tasks are completed! No scheduling needed."
            };
        }

        var scheduledTasks = new List<ScheduledTaskDto>();
        var currentDate = dto.StartDate.Date;
        var totalHours = 0;

        foreach (var task in incompleteTasks)
        {
            // Estimate hours based on task complexity (simple heuristic)
            var estimatedHours = EstimateTaskHours(task.Title);
            totalHours += estimatedHours;

            // Find next available working day
            while (!dto.WorkingDays.Contains((int)currentDate.DayOfWeek))
            {
                currentDate = currentDate.AddDays(1);
            }

            // Calculate days needed for this task
            var daysNeeded = Math.Ceiling((double)estimatedHours / dto.AvailableHoursPerDay);
            var workingDaysNeeded = 0;
            var endDate = currentDate;

            while (workingDaysNeeded < daysNeeded)
            {
                if (dto.WorkingDays.Contains((int)endDate.DayOfWeek))
                {
                    workingDaysNeeded++;
                }
                if (workingDaysNeeded < daysNeeded)
                {
                    endDate = endDate.AddDays(1);
                }
            }

            // Determine priority based on due date
            var priority = DeterminePriority(task.DueDate, endDate);

            scheduledTasks.Add(new ScheduledTaskDto
            {
                TaskId = task.Id,
                Title = task.Title,
                SuggestedStartDate = currentDate,
                SuggestedDueDate = endDate,
                EstimatedHours = estimatedHours,
                Priority = priority
            });

            // Move to next day after this task
            currentDate = endDate.AddDays(1);
        }

        return new ScheduleResponseDto
        {
            ScheduledTasks = scheduledTasks,
            TotalEstimatedHours = totalHours,
            Message = $"Successfully scheduled {scheduledTasks.Count} tasks over {(scheduledTasks.Last().SuggestedDueDate - dto.StartDate).Days + 1} days."
        };
    }

    private int EstimateTaskHours(string title)
    {
        // Simple heuristic: estimate based on title length and keywords
        var baseHours = 4; // Default estimate
        
        var complexityKeywords = new[] { "implement", "develop", "design", "architecture", "integration", "complex", "refactor" };
        var simpleKeywords = new[] { "fix", "update", "change", "small", "quick", "simple" };

        var lowerTitle = title.ToLower();

        if (complexityKeywords.Any(k => lowerTitle.Contains(k)))
            baseHours = 8;
        else if (simpleKeywords.Any(k => lowerTitle.Contains(k)))
            baseHours = 2;

        // Adjust based on title length (longer descriptions = more complex)
        if (title.Length > 50)
            baseHours += 2;

        return Math.Min(baseHours, 16); // Cap at 16 hours per task
    }

    private string DeterminePriority(DateTime? dueDate, DateTime suggestedDueDate)
    {
        if (dueDate == null)
            return "medium";

        var daysUntilDue = (dueDate.Value.Date - suggestedDueDate.Date).Days;

        if (daysUntilDue < 0)
            return "high"; // Already past due date
        else if (daysUntilDue <= 2)
            return "high"; // Very tight deadline
        else if (daysUntilDue <= 7)
            return "medium";
        else
            return "low";
    }
}