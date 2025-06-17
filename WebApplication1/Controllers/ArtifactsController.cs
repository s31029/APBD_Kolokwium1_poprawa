using WebApplication1.Exceptions;
using WebApplication1.Models.DTOs;
using WebApplication1.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/artifacts")]
public class ArtifactsController : ControllerBase
{
    private readonly IDbService _dbService;

    public ArtifactsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpPost]
    public async Task<IActionResult> AddArtifactWithProject([FromBody] CreateArtifactWithProjectDto request)
    {
        try
        {
            await _dbService.AddArtifactWithProjectAsync(request);
            return Created("", new { message = "Artifact and project added successfully." });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}