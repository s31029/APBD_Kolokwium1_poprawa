using System.Data.SqlClient;
using WebApplication1.Exceptions;
using WebApplication1.Models.DTOs;
using WebApplication1.Services;
using WebApplication1.Exceptions;
using WebApplication1.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace WebApplication1.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;

    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Connection string not found.");
    }

    public async Task<ProjectDetailsDto> GetProjectByIdAsync(int projectId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query1 = @"
            SELECT 
                p.ProjectId, p.Objective, p.StartDate, p.EndDate,
                a.Name AS ArtifactName, a.OriginDate,
                i.InstitutionId, i.Name AS InstitutionName, i.FoundedYear
            FROM Preservation_Project p
            JOIN Artifact a ON p.ArtifactId = a.ArtifactId
            JOIN Institution i ON a.InstitutionId = i.InstitutionId
            WHERE p.ProjectId = @ProjectId;";

        await using var cmd1 = new SqlCommand(query1, connection);
        cmd1.Parameters.AddWithValue("@ProjectId", projectId);

        var reader = await cmd1.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new NotFoundException($"Project with ID {projectId} not found.");

        var project = new ProjectDetailsDto
        {
            ProjectId = reader.GetInt32(0),
            Objective = reader.GetString(1),
            StartDate = reader.GetDateTime(2),
            EndDate = await reader.IsDBNullAsync(3) ? null : reader.GetDateTime(3),
            Artifact = new ArtifactDto
            {
                Name = reader.GetString(4),
                OriginDate = reader.GetDateTime(5),
                Institution = new InstitutionDto
                {
                    InstitutionId = reader.GetInt32(6),
                    Name = reader.GetString(7),
                    FoundedYear = reader.GetInt32(8)
                }
            }
        };

        await reader.CloseAsync();
        
        var query2 = @"
            SELECT s.FirstName, s.LastName, s.HireDate, sa.Role
            FROM Staff_Assignment sa
            JOIN Staff s ON sa.StaffId = s.StaffId
            WHERE sa.ProjectId = @ProjectId;";

        await using var cmd2 = new SqlCommand(query2, connection);
        cmd2.Parameters.AddWithValue("@ProjectId", projectId);

        var reader2 = await cmd2.ExecuteReaderAsync();
        while (await reader2.ReadAsync())
        {
            project.StaffAssignments.Add(new StaffAssignmentDto
            {
                FirstName = reader2.GetString(0),
                LastName = reader2.GetString(1),
                HireDate = reader2.GetDateTime(2),
                Role = reader2.GetString(3)
            });
        }

        return project;
    }
    public async Task AddArtifactWithProjectAsync(CreateArtifactWithProjectDto request)
{
    await using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();

    var transaction = await connection.BeginTransactionAsync();
    await using var command = new SqlCommand();
    command.Connection = connection;
    command.Transaction = (SqlTransaction)transaction;

    try
    {
        command.CommandText = "SELECT 1 FROM Institution WHERE InstitutionId = @InstitutionId";
        command.Parameters.AddWithValue("@InstitutionId", request.Artifact.InstitutionId);
        var institutionExists = await command.ExecuteScalarAsync();
        if (institutionExists is null)
            throw new NotFoundException($"Institution with ID {request.Artifact.InstitutionId} not found.");
        
        command.Parameters.Clear();
        command.CommandText = @"
            INSERT INTO Artifact (ArtifactId, Name, OriginDate, InstitutionId)
            VALUES (@ArtifactId, @Name, @OriginDate, @InstitutionId);";
        command.Parameters.AddWithValue("@ArtifactId", request.Artifact.ArtifactId);
        command.Parameters.AddWithValue("@Name", request.Artifact.Name);
        command.Parameters.AddWithValue("@OriginDate", request.Artifact.OriginDate);
        command.Parameters.AddWithValue("@InstitutionId", request.Artifact.InstitutionId);

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (SqlException e) when (e.Number == 2627)
        {
            throw new ConflictException($"Artifact with ID {request.Artifact.ArtifactId} already exists.");
        }
        
        command.Parameters.Clear();
        command.CommandText = @"
            INSERT INTO Preservation_Project (ProjectId, ArtifactId, StartDate, EndDate, Objective)
            VALUES (@ProjectId, @ArtifactId, @StartDate, @EndDate, @Objective);";
        command.Parameters.AddWithValue("@ProjectId", request.Project.ProjectId);
        command.Parameters.AddWithValue("@ArtifactId", request.Artifact.ArtifactId);
        command.Parameters.AddWithValue("@StartDate", request.Project.StartDate);
        command.Parameters.AddWithValue("@EndDate", request.Project.EndDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Objective", request.Project.Objective);

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (SqlException e) when (e.Number == 2627)
        {
            throw new ConflictException($"Project with ID {request.Project.ProjectId} already exists.");
        }

        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
}