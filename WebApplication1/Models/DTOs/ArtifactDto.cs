namespace WebApplication1.Models.DTOs;

public class ArtifactDto
{
    public string Name { get; set; } = null!;
    public DateTime OriginDate { get; set; }
    public InstitutionDto Institution { get; set; } = null!;
}