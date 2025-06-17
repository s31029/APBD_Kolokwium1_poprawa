using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.DTOs;

public class ArtifactInputDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ArtifactId must be a positive number.")]
    public int ArtifactId { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = null!;
    
    [Required]
    public DateTime OriginDate { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "InstitutionId must be a positive number.")]
    public int InstitutionId { get; set; }
}