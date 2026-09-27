using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Common;
using SchoolErp.Domain.Entities;

namespace SchoolErp.WebAPI.Controllers;

/// <summary>
/// Controller for managing student performance data including marks and grades.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PerformanceController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public PerformanceController(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Bulk saves or updates student marks.
    /// </summary>
    /// <param name="marks">List of marks to save or update.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Number of marks saved successfully.</returns>
    /// <response code="200">Marks saved successfully.</response>
    /// <response code="400">No marks provided for saving.</response>
    [HttpPost("bulk-save")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> BulkSave([FromBody] List<SaveStudentMarkItemDto> marks, CancellationToken ct)
    {
        if (marks == null || marks.Count == 0)
        {
            return BadRequest(new { message = "No marks provided for saving." });
        }

        var tenantId = _currentUser.TenantId ?? Guid.Empty;
        if (tenantId == Guid.Empty)
            return Unauthorized(new { message = "Tenant information is required." });

        var studentIds = marks.Select(m => m.StudentId).Where(id => id != Guid.Empty).Distinct().ToList();
        if (studentIds.Count == 0 || marks.Any(m => m.StudentId == Guid.Empty))
            return BadRequest(new { message = "No valid student IDs provided." });

        // Validate that students belong to the current tenant
        var students = await _db.Students
            .Where(s => s.TenantId == tenantId && studentIds.Contains(s.Id))
            .Select(s => new { s.Id, s.SectionId })
            .ToDictionaryAsync(s => s.Id, ct);

        if (students.Count != studentIds.Count)
            return BadRequest(new { message = "One or more students do not belong to this school." });

        var savedCount = 0;
        var now = DateTime.UtcNow;

        foreach (var item in marks)
        {
            if (string.IsNullOrWhiteSpace(item.Subject) || string.IsNullOrWhiteSpace(item.ExamTerm))
                return BadRequest(new { message = "Subject and exam term are required for every mark." });

            var subject = item.Subject.Trim();
            var examTerm = item.ExamTerm.Trim();
            var maxScore = item.MaxScore;
            var scoreObtained = item.ScoreObtained;

            if (maxScore <= 0 || scoreObtained < 0 || scoreObtained > maxScore)
                return BadRequest(new { message = $"Marks must be between 0 and the maximum score ({maxScore})." });

            if (_currentUser.Roles.Contains(Roles.Teacher) &&
                !_currentUser.Roles.Any(r => r is Roles.SuperAdmin or Roles.Admin) &&
                (students[item.StudentId].SectionId is not { } sectionId ||
                 !await IsAssignedCourseAsync(sectionId, subject, tenantId, ct)))
                return Forbid();
            
            var dateRecorded = item.DateRecorded ?? now;

            // Check if mark already exists for student, subject, and term (or by Id)
            StudentMark? existing = null;
            if (item.Id.HasValue && item.Id.Value != Guid.Empty)
            {
                existing = await _db.StudentMarks
                    .FirstOrDefaultAsync(m => m.Id == item.Id.Value &&
                                              m.TenantId == tenantId &&
                                              m.StudentId == item.StudentId &&
                                              m.Subject == subject &&
                                              m.ExamTerm == examTerm, ct);

                if (existing == null)
                    return BadRequest(new { message = "The mark being updated does not match the selected student, subject, and term." });
            }

            if (existing == null)
            {
                existing = await _db.StudentMarks
                    .FirstOrDefaultAsync(m => m.TenantId == tenantId &&
                                              m.StudentId == item.StudentId &&
                                              m.Subject == subject &&
                                              m.ExamTerm == examTerm, ct);
            }

            if (existing != null)
            {
                if (existing.ReviewStatus == "Finalized")
                    return Conflict(new { message = "Finalized marks must be reopened by an administrator before they can be changed." });

                existing.ScoreObtained = scoreObtained;
                existing.MaxScore = maxScore;
                existing.DateRecorded = dateRecorded;
                existing.ReviewStatus = "Submitted";
                existing.ReviewComment = null;
                existing.ReviewedBy = null;
                existing.ReviewedAtUtc = null;
                existing.UpdatedAtUtc = now;
                existing.UpdatedBy = _currentUser.UserName ?? _currentUser.UserId;
            }
            else
            {
                var newMark = new StudentMark
                {
                    TenantId = tenantId,
                    StudentId = item.StudentId,
                    Subject = subject,
                    ExamTerm = examTerm,
                    ScoreObtained = scoreObtained,
                    MaxScore = maxScore,
                    DateRecorded = dateRecorded,
                    ReviewStatus = "Submitted",
                    CreatedAtUtc = now,
                    CreatedBy = _currentUser.UserName ?? _currentUser.UserId
                };
                _db.StudentMarks.Add(newMark);
            }

            savedCount++;
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            message = $"{savedCount} student mark(s) saved successfully.",
            count = savedCount
        });
    }

    [HttpPost("mark/{markId:guid}/review")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    public async Task<IActionResult> ReviewMark(Guid markId, [FromBody] ReviewStudentMarkDto request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;
        if (tenantId == Guid.Empty)
            return Unauthorized(new { message = "Tenant information is required." });

        if (request.Status is not ("Under Review" or "Approved" or "Rejected" or "Finalized"))
            return BadRequest(new { message = "Choose Under Review, Approved, Rejected, or Finalized." });

        if (request.Comment?.Length > 1000)
            return BadRequest(new { message = "Review comments must be 1000 characters or fewer." });

        var mark = await _db.StudentMarks.FirstOrDefaultAsync(m => m.Id == markId && m.TenantId == tenantId, ct);
        if (mark == null)
            return NotFound(new { message = "Mark not found." });

        if (mark.ReviewStatus == "Finalized")
            return Conflict(new { message = "Reopen the finalized result before changing its review status." });

        if (request.Status == "Finalized" && mark.ReviewStatus != "Approved")
            return BadRequest(new { message = "Only approved marks can be finalized." });

        mark.ReviewStatus = request.Status;
        mark.ReviewComment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();
        mark.ReviewedBy = _currentUser.UserName ?? _currentUser.UserId;
        mark.ReviewedAtUtc = DateTime.UtcNow;
        mark.UpdatedAtUtc = DateTime.UtcNow;
        mark.UpdatedBy = _currentUser.UserName ?? _currentUser.UserId;
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = $"Mark {request.Status.ToLowerInvariant()} successfully.", status = mark.ReviewStatus });
    }

    [HttpPost("mark/{markId:guid}/reopen")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    public async Task<IActionResult> ReopenMark(Guid markId, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;
        if (tenantId == Guid.Empty)
            return Unauthorized(new { message = "Tenant information is required." });

        var mark = await _db.StudentMarks.FirstOrDefaultAsync(m => m.Id == markId && m.TenantId == tenantId, ct);
        if (mark == null)
            return NotFound(new { message = "Mark not found." });
        if (mark.ReviewStatus != "Finalized")
            return BadRequest(new { message = "Only finalized marks can be reopened." });

        mark.ReviewStatus = "Approved";
        mark.ReviewComment = "Reopened by an administrator.";
        mark.ReviewedBy = _currentUser.UserName ?? _currentUser.UserId;
        mark.ReviewedAtUtc = DateTime.UtcNow;
        mark.UpdatedAtUtc = DateTime.UtcNow;
        mark.UpdatedBy = _currentUser.UserName ?? _currentUser.UserId;
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Finalized mark reopened for correction.", status = mark.ReviewStatus });
    }

    /// <summary>
    /// Retrieves all historical performance records for a specific student ordered by date.
    /// </summary>
    /// <param name="studentId">The student's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of marks for the student.</returns>
    /// <response code="200">Returns student's performance history.</response>
    /// <response code="403">Access denied (not the student or not authorized).</response>
    /// <response code="404">Student not found.</response>
    [HttpGet("student/{studentId:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher},{Roles.Student}")]
    public async Task<IActionResult> GetStudentPerformance(Guid studentId, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;
        if (tenantId == Guid.Empty)
            return Unauthorized(new { message = "Tenant information is required." });

        if (_currentUser.Roles.Contains(Roles.Student) &&
            !_currentUser.Roles.Any(r => r is Roles.SuperAdmin or Roles.Admin or Roles.Teacher))
        {
            var ownsStudentRecord = await _db.Students
                .AsNoTracking()
                .AnyAsync(s => s.Id == studentId && s.TenantId == tenantId && s.UserId == _currentUser.UserId, ct);

            if (!ownsStudentRecord)
                return Forbid();
        }

        var student = await _db.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId && s.TenantId == tenantId, ct);

        if (student == null)
        {
            return NotFound(new { message = "Student not found." });
        }

        if (_currentUser.Roles.Contains(Roles.Teacher) &&
            !_currentUser.Roles.Any(r => r is Roles.SuperAdmin or Roles.Admin) &&
            (student.SectionId is not { } sectionId ||
             !await _db.Courses.AnyAsync(c => c.TenantId == tenantId &&
                                              c.TeacherUserId == _currentUser.UserId &&
                                              c.SectionId == sectionId, ct)))
            return Forbid();

        var marksQuery = _db.StudentMarks
            .Where(m => m.StudentId == studentId && m.TenantId == tenantId)
            .AsQueryable();

        if (_currentUser.Roles.Contains(Roles.Student) &&
            !_currentUser.Roles.Any(r => r is Roles.SuperAdmin or Roles.Admin or Roles.Teacher))
            marksQuery = marksQuery.Where(m => m.ReviewStatus == "Approved" || m.ReviewStatus == "Finalized");

        if (_currentUser.Roles.Contains(Roles.Teacher) &&
            !_currentUser.Roles.Any(r => r is Roles.SuperAdmin or Roles.Admin))
        {
            marksQuery = marksQuery.Where(m => _db.Courses.Any(c =>
                c.TenantId == tenantId && c.TeacherUserId == _currentUser.UserId &&
                c.SectionId == student.SectionId && c.Name.ToLower() == m.Subject.ToLower()));
        }

        var marks = await marksQuery
            .OrderByDescending(m => m.DateRecorded)
            .ThenBy(m => m.Subject)
            .Select(m => new StudentMarkDto
            {
                Id = m.Id,
                StudentId = m.StudentId,
                StudentName = student.FullName,
                AdmissionNumber = student.AdmissionNumber,
                Subject = m.Subject,
                ExamTerm = m.ExamTerm,
                ScoreObtained = m.ScoreObtained,
                MaxScore = m.MaxScore,
                Percentage = m.MaxScore == 0 ? 0m : Math.Round(m.ScoreObtained / m.MaxScore * 100m, 2),
                DateRecorded = m.DateRecorded,
                ReviewStatus = m.ReviewStatus,
                ReviewComment = m.ReviewComment
            })
            .ToListAsync(ct);

        return Ok(marks);
    }

    /// <summary>
    /// Retrieves marks for a specific class and optional exam term.
    /// </summary>
    /// <param name="classId">The class/section identifier.</param>
    /// <param name="term">Optional exam term filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of marks for all students in the class.</returns>
    /// <response code="200">Returns class performance data.</response>
    /// <response code="404">Class not found or no marks available.</response>
    [HttpGet("class/{classId:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> GetClassPerformance(Guid classId, [FromQuery] string? term, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;
        if (tenantId == Guid.Empty)
            return Unauthorized(new { message = "Tenant information is required." });

        var classExists = await _db.Sections
            .AsNoTracking()
            .AnyAsync(s => s.Id == classId && s.TenantId == tenantId, ct);

        if (!classExists)
        {
            return NotFound(new { message = "Class not found." });
        }

        var query = _db.StudentMarks
            .Where(m => m.TenantId == tenantId && m.Student != null && m.Student.SectionId == classId);

        if (_currentUser.Roles.Contains(Roles.Teacher) &&
            !_currentUser.Roles.Any(r => r is Roles.SuperAdmin or Roles.Admin))
        {
            query = query.Where(m => _db.Courses.Any(c =>
                c.TenantId == tenantId && c.TeacherUserId == _currentUser.UserId &&
                c.SectionId == classId && c.Name.ToLower() == m.Subject.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(term))
        {
            var normalizedTerm = term.Trim();
            query = query.Where(m => m.ExamTerm == normalizedTerm);
        }

        var marks = await query
            .Include(m => m.Student)
            .OrderBy(m => m.Student!.LastName)
            .ThenBy(m => m.Student!.FirstName)
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
                DateRecorded = m.DateRecorded,
                ReviewStatus = m.ReviewStatus,
                ReviewComment = m.ReviewComment
            })
            .ToListAsync(ct);

        return Ok(marks);
    }

    private Task<bool> IsAssignedCourseAsync(Guid sectionId, string subject, Guid tenantId, CancellationToken ct)
    {
        var normalizedSubject = subject.ToLower();
        return _db.Courses.AnyAsync(c => c.TenantId == tenantId &&
                                         c.TeacherUserId == _currentUser.UserId &&
                                         c.SectionId == sectionId &&
                                         c.Name.ToLower() == normalizedSubject, ct);
    }
}

/// <summary>
/// DTO for a student mark record.
/// </summary>
public class StudentMarkDto
{
    /// <summary>Mark identifier.</summary>
    public Guid Id { get; set; }
    
    /// <summary>Student identifier.</summary>
    public Guid StudentId { get; set; }
    
    /// <summary>Student's full name.</summary>
    public string StudentName { get; set; } = string.Empty;
    
    /// <summary>Student's admission number.</summary>
    public string AdmissionNumber { get; set; } = string.Empty;
    
    /// <summary>Subject name.</summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>Exam term (e.g., "Mid-Year", "Final").</summary>
    public string ExamTerm { get; set; } = string.Empty;
    
    /// <summary>Score obtained by the student.</summary>
    public decimal ScoreObtained { get; set; }
    
    /// <summary>Maximum possible score for the exam.</summary>
    public decimal MaxScore { get; set; }
    
    /// <summary>Percentage score (0-100).</summary>
    public decimal Percentage { get; set; }
    
    /// <summary>Date the mark was recorded.</summary>
    public DateTime DateRecorded { get; set; }
    public string ReviewStatus { get; set; } = string.Empty;
    public string? ReviewComment { get; set; }
}

public class ReviewStudentMarkDto
{
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

/// <summary>
/// Request DTO for saving/updating student marks in bulk.
/// </summary>
public class SaveStudentMarkItemDto
{
    /// <summary>Mark identifier (null for new marks). Optional.</summary>
    public Guid? Id { get; set; }
    
    /// <summary>Student to assign the mark to.</summary>
    public Guid StudentId { get; set; }
    
    /// <summary>Subject name.</summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>Exam term.</summary>
    public string ExamTerm { get; set; } = string.Empty;
    
    /// <summary>Score obtained by the student.</summary>
    public decimal ScoreObtained { get; set; }
    
    /// <summary>Maximum possible score.</summary>
    public decimal MaxScore { get; set; } = 100m;
    
    /// <summary>Date the mark was recorded. Defaults to current time if not specified.</summary>
    public DateTime? DateRecorded { get; set; }
}