using System.ComponentModel.DataAnnotations;

namespace MiniProjectManager.DTOs;

// Auth DTOs
public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

// Project DTOs
public class CreateProjectDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
}

public class UpdateProjectDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
}

public class ProjectDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
}

public class ProjectDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TaskDto> Tasks { get; set; } = new();
}

// Task DTOs
public class CreateTaskDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;
    
    public DateTime? DueDate { get; set; }
}

public class UpdateTaskDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;
    
    public DateTime? DueDate { get; set; }
    
    public bool IsCompleted { get; set; }
}

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ProjectId { get; set; }
}

// Scheduler DTOs
public class ScheduleRequestDto
{
    [Required]
    public int AvailableHoursPerDay { get; set; }
    
    [Required]
    public List<int> WorkingDays { get; set; } = new(); // 0 = Sunday, 6 = Saturday
    
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
}

public class ScheduledTaskDto
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime SuggestedStartDate { get; set; }
    public DateTime SuggestedDueDate { get; set; }
    public int EstimatedHours { get; set; }
    public string Priority { get; set; } = "medium";
}

public class ScheduleResponseDto
{
    public List<ScheduledTaskDto> ScheduledTasks { get; set; } = new();
    public int TotalEstimatedHours { get; set; }
    public string Message { get; set; } = string.Empty;
}