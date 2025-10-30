using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniProjectManager.DTOs;
using MiniProjectManager.Services;
using System.Security.Claims;

namespace MiniProjectManager.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProjects()
    {
        try
        {
            var userId = GetUserId();
            var projects = await _projectService.GetAllProjectsAsync(userId);
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects");
            return StatusCode(500, new { message = "An error occurred while retrieving projects" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProjectById(int id)
    {
        try
        {
            var userId = GetUserId();
            var project = await _projectService.GetProjectByIdAsync(id, userId);

            if (project == null)
                return NotFound(new { message = "Project not found" });

            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project");
            return StatusCode(500, new { message = "An error occurred while retrieving the project" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var project = await _projectService.CreateProjectAsync(dto, userId);

            if (project == null)
                return BadRequest(new { message = "Failed to create project" });

            return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return StatusCode(500, new { message = "An error occurred while creating the project" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var project = await _projectService.UpdateProjectAsync(id, dto, userId);

            if (project == null)
                return NotFound(new { message = "Project not found" });

            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project");
            return StatusCode(500, new { message = "An error occurred while updating the project" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _projectService.DeleteProjectAsync(id, userId);

            if (!result)
                return NotFound(new { message = "Project not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project");
            return StatusCode(500, new { message = "An error occurred while deleting the project" });
        }
    }
}