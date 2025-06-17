using WebApplication1.Models.DTOs;

namespace WebApplication1.Services
{
    public interface IDbService
    {
        Task<ProjectDetailsDto> GetProjectByIdAsync(int projectId);
        Task AddArtifactWithProjectAsync(CreateArtifactWithProjectDto request);
    }
}