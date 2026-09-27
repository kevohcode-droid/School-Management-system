using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Common;

namespace SchoolErp.WebAPI.Controllers;

/// <summary>
/// Parent portal for viewing their children's academic and fee information.
/// All endpoints enforce that the authenticated user must be linked to the student
/// via the ParentStudents junction table.
/// </summary>
[ApiController]
[Route("api/parent")]
[Authorize(Roles = Roles.Parent)]
public class ParentPortalController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ParentPortalController(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Returns all children linked to the authenticated parent.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of children with their basic information.</returns>
    /// <response code="200">Returns list of children.</response>
    /// <response code="401">User not authenticated.</response>
    [HttpGet("children")]
    public async Task<IActionResult> GetChildren(CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        var tenantId = _currentUser.TenantId;
        
        if (string.IsNullOrWhiteSpace(userId) || tenantId is null)
            return Unauthorized(new { message = "Authentication required." });

        var children = await _db.ParentStudents
            .AsNoTracking()
            .Where(ps => ps.TenantId == tenantId && ps.Parent != null && ps.Parent.UserId == userId)
            .Include(ps => ps.Student)
            .ThenInclude(s => s!.Section)
            .OrderBy(ps => ps.Student!.LastName)
            .ThenBy(ps => ps.Student!.FirstName)
            .Select(ps => new ParentChildDto
            {
                StudentId = ps.StudentId,
                AdmissionNumber = ps.Student != null ? ps.Student.AdmissionNumber : string.Empty,
                FullName = ps.Student != null ? ps.Student.FullName : string.Empty,
                SectionId = ps.Student != null ? ps.Student.SectionId : null,
                SectionName = ps.Student != null && ps.Student.Section != null ? ps.Student.Section.Name : null,
                RelationshipType = ps.RelationshipType.ToString(),
                IsPrimaryContact = ps.IsPrimaryContact
            })
            .ToListAsync(ct);

        return Ok(children);
    }

    /// <summary>
    /// Retrieves performance data for a child linked to the authenticated parent.
    /// </summary>
    /// <param name="studentId">The child's student identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of marks with percentage scores.</returns>
    /// <response code="200">Returns performance data.</response>
    /// <response code="403">Access denied (child not linked to parent).</response>
    [HttpGet("child/{studentId:guid}/performance")]
    public async Task<IActionResult> GetChildPerformance(Guid studentId, CancellationToken ct)
    {
        if (!await IsLinkedChildAsync(studentId, ct))
            return Forbid();

        var tenantId = _currentUser.TenantId ?? Guid.Empty;
        if (tenantId == Guid.Empty)
            return Unauthorized(new { message = "Tenant information is required." });

        var marks = await _db.StudentMarks
            .AsNoTracking()
            .Where(m => m.StudentId == studentId && m.TenantId == tenantId &&
                        (m.ReviewStatus == "Approved" || m.ReviewStatus == "Finalized"))
            .OrderByDescending(m => m.DateRecorded)
            .ThenBy(m => m.Subject)
            .Select(m => new StudentMarkDto
            {
                Id = m.Id,
                StudentId = m.StudentId,
                StudentName = m.Student != null ? m.Student.FullName : string.Empty,
                AdmissionNumber = m.Student != null ? m.Student.AdmissionNumber : string.Empty,
                Subject = m.Subject,
                ExamTerm = m.ExamTerm,
                ScoreObtained = m.ScoreObtained,
                MaxScore = m.MaxScore,
                Percentage = m.MaxScore == 0 ? 0m : Math.Round(m.ScoreObtained / m.MaxScore * 100m, 2),
                DateRecorded = m.DateRecorded
            })
            .ToListAsync(ct);

        return Ok(marks);
    }

    /// <summary>
    /// Verifies that the authenticated parent is linked to the specified student.
    /// </summary>
    /// <param name="studentId">The student identifier to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if linked, false otherwise.</returns>
    private async Task<bool> IsLinkedChildAsync(Guid studentId, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        var tenantId = _currentUser.TenantId;
        
        return !string.IsNullOrWhiteSpace(userId)
            && tenantId is not null
            && await _db.ParentStudents
                .AsNoTracking()
                .AnyAsync(ps =>
                    ps.TenantId == tenantId &&
                    ps.StudentId == studentId &&
                    ps.Parent != null &&
                    ps.Parent.UserId == userId, ct);
    }
}

/// <summary>
/// Represents a child linked to a parent account.
/// </summary>
public sealed class ParentChildDto
{
    /// <summary>Student identifier.</summary>
    public Guid StudentId { get; set; }
    
    /// <summary>Admission number.</summary>
    public string AdmissionNumber { get; set; } = string.Empty;
    
    /// <summary>Full name of the student.</summary>
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>Section/Class identifier.</summary>
    public Guid? SectionId { get; set; }
    
    /// <summary>Section/Class name.</summary>
    public string? SectionName { get; set; }
    
    /// <summary>Relationship type (e.g., Mother, Father, Guardian).</summary>
    public string RelationshipType { get; set; } = string.Empty;
    
    /// <summary>Whether this is the primary contact.</summary>
    public bool IsPrimaryContact { get; set; }
}