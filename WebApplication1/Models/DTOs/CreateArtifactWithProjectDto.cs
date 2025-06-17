using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.DTOs;

public class CreateArtifactWithProjectDto
{
    [Required]
    public ArtifactInputDto Artifact { get; set; } = null!;
    
    [Required]
    public ProjectInputDto Project { get; set; } = null!;
}
