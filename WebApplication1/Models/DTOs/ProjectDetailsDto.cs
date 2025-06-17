namespace WebApplication1.Models.DTOs;

public class ProjectDetailsDto
{
    public int ProjectId { get; set; }
    public string Objective { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ArtifactDto Artifact { get; set; } = null!;
    public List<StaffAssignmentDto> StaffAssignments { get; set; } = new();
}
