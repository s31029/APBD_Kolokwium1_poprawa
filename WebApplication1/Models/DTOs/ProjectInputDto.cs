using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.DTOs;

public class ProjectInputDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ProjectId must be a positive number.")]
    public int ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Objective { get; set; } = null!;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
}