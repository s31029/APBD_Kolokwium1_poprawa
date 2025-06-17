namespace WebApplication1.Models.DTOs;

public class StaffAssignmentDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime HireDate { get; set; }
    public string Role { get; set; } = null!;
}