using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Common;
using SchoolErp.Domain.Enums;

namespace SchoolErp.WebAPI.Controllers;

/// <summary>
/// Student-only portal. Every endpoint enforces ownership:
/// the authenticated user must own the student record they are querying
/// (Student.UserId == CurrentUser.UserId AND same TenantId).
/// No write operations are exposed.
/// </summary>
[ApiController]
[Route("api/student-portal")]
[Authorize(Roles = Roles.Student)]
public class StudentPortalController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public StudentPortalController(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    // ── Resolve the student record that belongs to the caller ──────────────
    /// <summary>
    /// Resolves the student record linked to the current authenticated user.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The student's ID if found, null otherwise.</returns>
    private async Task<Guid?> ResolveOwnStudentIdAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId) || _currentUser.TenantId is null)
            return null;

        return await _db.Students
            .AsNoTracking()
            .Where(s => s.TenantId == _currentUser.TenantId && s.UserId == _currentUser.UserId)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(ct);
    }

    // ── GET /api/student-portal/me ─────────────────────────────────────────
    /// <summary>
    /// Returns the authenticated student's own profile.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Student profile information.</returns>
    /// <response code="200">Returns the student profile.</response>
    /// <response code="404">Student record not found.</response>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var ownId = await ResolveOwnStudentIdAsync(ct);
        if (ownId is null)
            return NotFound(new { message = "No student record is linked to your account. Contact your school administrator." });

        var tenantId = _currentUser.TenantId!.Value;

        var student = await _db.Students
            .AsNoTracking()
            .Include(s => s.Section)
            .Where(s => s.Id == ownId && s.TenantId == tenantId)
            .Select(s => new StudentProfileDto
            {
                StudentId    = s.Id,
                AdmissionNumber = s.AdmissionNumber,
                FullName     = s.FullName,
                FirstName    = s.FirstName,
                LastName     = s.LastName,
                Email        = s.Email,
                DateOfBirth  = s.DateOfBirth,
                Gender       = s.Gender.ToString(),
                SectionId    = s.SectionId,
                SectionName  = s.Section != null ? s.Section.Name : null,
                EnrollmentDate = s.EnrollmentDate
            })
            .FirstOrDefaultAsync(ct);

        if (student is null)
            return NotFound(new { message = "Student record not found." });

        return Ok(student);
    }

    // ── GET /api/student-portal/performance ────────────────────────────────
    /// <summary>
    /// Returns the student's own marks grouped by term.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Performance data with marks grouped by term.</returns>
    /// <response code="200">Returns performance data.</response>
    /// <response code="404">No student record linked to account.</response>
    [HttpGet("performance")]
    public async Task<IActionResult> GetMyPerformance(CancellationToken ct)
    {
        var ownId = await ResolveOwnStudentIdAsync(ct);
        if (ownId is null)
            return NotFound(new { message = "No student record linked to your account." });

        var tenantId = _currentUser.TenantId!.Value;

        var marks = await _db.StudentMarks
            .AsNoTracking()
            .Where(m => m.StudentId == ownId && m.TenantId == tenantId &&
                        (m.ReviewStatus == "Approved" || m.ReviewStatus == "Finalized"))
            .OrderByDescending(m => m.DateRecorded)
            .ThenBy(m => m.Subject)
            .Select(m => new StudentPortalMarkDto
            {
                Id            = m.Id,
                Subject       = m.Subject,
                ExamTerm      = m.ExamTerm,
                ScoreObtained = m.ScoreObtained,
                MaxScore      = m.MaxScore,
                Percentage    = m.MaxScore == 0 ? 0 : Math.Round(m.ScoreObtained / m.MaxScore * 100m, 2),
                DateRecorded  = m.DateRecorded
            })
            .ToListAsync(ct);

        // Group by term for the front-end report card view
        var grouped = marks
            .GroupBy(m => m.ExamTerm)
            .OrderByDescending(g => g.Key)
            .Select(g => new TermReportDto
            {
                Term       = g.Key,
                Marks      = g.ToList(),
                Average    = g.Any() ? Math.Round(g.Average(m => m.Percentage), 1) : 0m,
                TotalMarks = g.Sum(m => m.ScoreObtained),
                TotalMax   = g.Sum(m => m.MaxScore)
            })
            .ToList();

        return Ok(new { marks, grouped });
    }

    // ── GET /api/student-portal/attendance ─────────────────────────────────
    /// <summary>
    /// Returns the student's own attendance records.
    /// </summary>
    /// <param name="year">Optional year filter.</param>
    /// <param name="month">Optional month filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Attendance records and summary statistics.</returns>
    /// <response code="200">Returns attendance records.</response>
    [HttpGet("attendance")]
    public async Task<IActionResult> GetMyAttendance(
        [FromQuery] int? year, [FromQuery] int? month, CancellationToken ct)
    {
        var ownId = await ResolveOwnStudentIdAsync(ct);
        if (ownId is null)
            return NotFound(new { message = "No student record linked to your account." });

        var tenantId = _currentUser.TenantId!.Value;

        var query = _db.AttendanceRecords
            .AsNoTracking()
            .Where(a => a.StudentId == ownId && a.TenantId == tenantId);

        if (year.HasValue)
            query = query.Where(a => a.Date.Year == year.Value);

        if (month.HasValue)
            query = query.Where(a => a.Date.Month == month.Value);

        var records = await query
            .OrderByDescending(a => a.Date)
            .Select(a => new StudentAttendanceDto
            {
                Date     = a.Date,
                Status   = a.Status.ToString(),
                Remarks  = a.Remarks,
                ClassId  = a.ClassId
            })
            .ToListAsync(ct);

        var totalRecords = records.Count;
        var summary = new AttendanceSummaryDto
        {
            Total    = totalRecords,
            Present  = records.Count(r => r.Status == AttendanceStatus.Present.ToString()),
            Absent   = records.Count(r => r.Status == AttendanceStatus.Absent.ToString()),
            Late     = records.Count(r => r.Status == AttendanceStatus.Late.ToString()),
            Excused  = records.Count(r => r.Status == AttendanceStatus.Excused.ToString()),
            Percentage = totalRecords == 0 ? 0m
                : Math.Round((decimal)(records.Count(r => r.Status == AttendanceStatus.Present.ToString()) * 100m / totalRecords), 1)
        };

        return Ok(new { records, summary });
    }

    // ── GET /api/student-portal/fees ───────────────────────────────────────
    /// <summary>
    /// Returns the student's own fee invoices and payment history (read-only).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Invoices with payment history and totals.</returns>
    /// <response code="200">Returns fee data.</response>
    [HttpGet("fees")]
    public async Task<IActionResult> GetMyFees(CancellationToken ct)
    {
        var ownId = await ResolveOwnStudentIdAsync(ct);
        if (ownId is null)
            return NotFound(new { message = "No student record linked to your account." });

        var tenantId = _currentUser.TenantId!.Value;

        var invoices = await _db.FeeInvoices
            .AsNoTracking()
            .Include(i => i.Items)
            .Include(i => i.PaymentTransactions)
            .Where(i => i.StudentId == ownId && i.TenantId == tenantId)
            .OrderByDescending(i => i.IssuedOnUtc)
            .Select(i => new StudentInvoiceDto
            {
                Id            = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                Description   = i.Description,
                Amount        = i.Amount,
                AmountPaid    = i.AmountPaid,
                DiscountAmount = i.DiscountAmount,
                Balance       = i.Amount - i.AmountPaid - i.DiscountAmount,
                DueDateUtc    = i.DueDateUtc,
                IssuedOnUtc   = i.IssuedOnUtc,
                Status        = i.Status.ToString(),
                AcademicYear  = i.AcademicYear,
                Term          = i.Term,
                Items = i.Items.Select(item => new StudentInvoiceItemDto
                {
                    Description = item.Description,
                    CategoryName = item.FeeCategory.Name,
                    Amount      = item.Amount
                }).ToList(),
                Payments = i.PaymentTransactions.Select(p => new StudentPaymentDto
                {
                    TransactionReference = p.TransactionReference,
                    Amount      = p.Amount,
                    PaymentDate = p.PaymentDateUtc,
                    Mode        = p.PaymentMode.ToString(),
                    Status      = p.Status.ToString()
                }).ToList()
            })
            .ToListAsync(ct);

        var totalOutstanding = invoices.Sum(i => i.Balance);
        var totalPaid        = invoices.Sum(i => i.AmountPaid);
        var overdueCount     = invoices.Count(i => i.Status == FeeInvoiceStatus.Overdue.ToString());

        return Ok(new { invoices, totalOutstanding, totalPaid, overdueCount });
    }

    // ── GET /api/student-portal/dashboard ──────────────────────────────────
    /// <summary>
    /// Aggregated summary for the student dashboard overview.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dashboard summary with attendance, fees, and announcements.</returns>
    /// <response code="200">Returns dashboard summary.</response>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var ownId = await ResolveOwnStudentIdAsync(ct);
        if (ownId is null)
            return NotFound(new { message = "No student record linked to your account." });

        var tenantId = _currentUser.TenantId!.Value;

        // Attendance
        var attendanceRecords = await _db.AttendanceRecords
            .AsNoTracking()
            .Where(a => a.StudentId == ownId && a.TenantId == tenantId)
            .Select(a => a.Status)
            .ToListAsync(ct);

        var totalAtt   = attendanceRecords.Count;
        var presentAtt = attendanceRecords.Count(s => s == AttendanceStatus.Present);
        var attPct     = totalAtt == 0 ? 0m : Math.Round((decimal)(presentAtt * 100m / totalAtt), 1);

        // Marks — latest term average
        var latestTermMarks = await _db.StudentMarks
            .AsNoTracking()
            .Where(m => m.StudentId == ownId && m.TenantId == tenantId)
            .OrderByDescending(m => m.DateRecorded)
            .Take(50)
            .Select(m => new { m.ExamTerm, m.ScoreObtained, m.MaxScore })
            .ToListAsync(ct);

        var latestTerm = latestTermMarks.FirstOrDefault()?.ExamTerm ?? "N/A";
        var avgPct = latestTermMarks.Any(m => m.ExamTerm == latestTerm && m.MaxScore > 0)
            ? Math.Round((decimal)latestTermMarks
                .Where(m => m.ExamTerm == latestTerm && m.MaxScore > 0)
                .Average(m => (decimal)(m.ScoreObtained / m.MaxScore * 100)), 1)
            : 0m;

        // Fees
        var feeData = await _db.FeeInvoices
            .AsNoTracking()
            .Where(i => i.StudentId == ownId && i.TenantId == tenantId)
            .Select(i => new { Balance = i.Amount - i.AmountPaid - i.DiscountAmount, i.Status })
            .ToListAsync(ct);

        var totalBalance   = feeData.Sum(i => i.Balance);
        var overdueInvoices = feeData.Count(i => i.Status == FeeInvoiceStatus.Overdue);

        // Announcements (tenant-wide, latest 5)
        var announcements = await _db.Announcements
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.IsActive)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(5)
            .Select(a => new AnnouncementDto { Title = a.Title, CreatedAt = a.CreatedAtUtc })
            .ToListAsync(ct);

        return Ok(new StudentDashboardDto
        {
            AttendancePercentage = attPct,
            LatestTermAverage    = avgPct,
            LatestTerm           = latestTerm,
            FeeBalance           = totalBalance,
            OverdueInvoices      = overdueInvoices,
            Announcements        = announcements
        });
    }
}

/// <summary>
/// Returns the authenticated student's profile information.
/// </summary>
public sealed class StudentProfileDto
{
    /// <summary>Student identifier.</summary>
    public Guid StudentId { get; set; }
    
    /// <summary>Admission number issued by the school.</summary>
    public string AdmissionNumber { get; set; } = string.Empty;
    
    /// <summary>Full name of the student.</summary>
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>First name.</summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>Last name.</summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>Email address.</summary>
    public string? Email { get; set; }
    
    /// <summary>Date of birth.</summary>
    public DateOnly? DateOfBirth { get; set; }
    
    /// <summary>Gender as string representation.</summary>
    public string Gender { get; set; } = string.Empty;
    
    /// <summary>Section/Class identifier.</summary>
    public Guid? SectionId { get; set; }
    
    /// <summary>Section/Class name.</summary>
    public string? SectionName { get; set; }
    
    /// <summary>Enrollment date.</summary>
    public DateTime EnrollmentDate { get; set; }
}

/// <summary>
/// Represents a single mark entry for a student.
/// </summary>
public sealed class StudentPortalMarkDto
{
    /// <summary>Mark identifier.</summary>
    public Guid Id { get; set; }
    
    /// <summary>Subject name.</summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>Exam term (e.g., "Mid-Year", "Final").</summary>
    public string ExamTerm { get; set; } = string.Empty;
    
    /// <summary>Score obtained.</summary>
    public decimal ScoreObtained { get; set; }
    
    /// <summary>Maximum possible score.</summary>
    public decimal MaxScore { get; set; }
    
    /// <summary>Percentage score (0-100).</summary>
    public decimal Percentage { get; set; }
    
    /// <summary>Date the mark was recorded.</summary>
    public DateTime DateRecorded { get; set; }
}

/// <summary>
/// Term-level report grouping for student performance.
/// </summary>
public sealed class TermReportDto
{
    /// <summary>Term name.</summary>
    public string Term { get; set; } = string.Empty;
    
    /// <summary>List of marks for this term.</summary>
    public List<StudentPortalMarkDto> Marks { get; set; } = new();
    
    /// <summary>Average percentage for the term (0-100).</summary>
    public decimal Average { get; set; }
    
    /// <summary>Total marks obtained.</summary>
    public decimal TotalMarks { get; set; }
    
    /// <summary>Total maximum marks.</summary>
    public decimal TotalMax { get; set; }
}

/// <summary>
/// Student attendance record for a single day.
/// </summary>
public sealed class StudentAttendanceDto
{
    /// <summary>Date of attendance record.</summary>
    public DateTime Date { get; set; }
    
    /// <summary>Attendance status (Present/Absent/Late/Excused).</summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>Optional remarks.</summary>
    public string? Remarks { get; set; }
    
    /// <summary>Section/Class identifier.</summary>
    public Guid ClassId { get; set; }
}

/// <summary>
/// Summary statistics for attendance records.
/// </summary>
public sealed class AttendanceSummaryDto
{
    /// <summary>Total attendance records.</summary>
    public int Total { get; set; }
    
    /// <summary>Count of present days.</summary>
    public int Present { get; set; }
    
    /// <summary>Count of absent days.</summary>
    public int Absent { get; set; }
    
    /// <summary>Count of late arrivals.</summary>
    public int Late { get; set; }
    
    /// <summary>Count of excused absences.</summary>
    public int Excused { get; set; }
    
    /// <summary>Attendance percentage (0-100).</summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// Fee invoice visible to students in their portal.
/// </summary>
public sealed class StudentInvoiceDto
{
    /// <summary>Invoice identifier.</summary>
    public Guid Id { get; set; }
    
    /// <summary>Invoice number.</summary>
    public string InvoiceNumber { get; set; } = string.Empty;
    
    /// <summary>Invoice description.</summary>
    public string? Description { get; set; }
    
    /// <summary>Total invoice amount.</summary>
    public decimal Amount { get; set; }
    
    /// <summary>Amount already paid.</summary>
    public decimal AmountPaid { get; set; }

    /// <summary>Discount applied to the invoice.</summary>
    public decimal DiscountAmount { get; set; }
    
    /// <summary>Outstanding balance.</summary>
    public decimal Balance { get; set; }
    
    /// <summary>Due date for payment.</summary>
    public DateTime DueDateUtc { get; set; }
    
    /// <summary>Date invoice was issued.</summary>
    public DateTime IssuedOnUtc { get; set; }
    
    /// <summary>Current status.</summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>Academic year.</summary>
    public string? AcademicYear { get; set; }
    
    /// <summary>Term.</summary>
    public string? Term { get; set; }
    
    /// <summary>Line items on the invoice.</summary>
    public List<StudentInvoiceItemDto> Items { get; set; } = new();
    
    /// <summary>Payments made against this invoice.</summary>
    public List<StudentPaymentDto> Payments { get; set; } = new();
}

/// <summary>
/// A single line item on an invoice.
/// </summary>
public sealed class StudentInvoiceItemDto
{
    /// <summary>Description of the fee item.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Fee category for the item.</summary>
    public string CategoryName { get; set; } = string.Empty;
    
    /// <summary>Amount for this item.</summary>
    public decimal Amount { get; set; }
}

/// <summary>
/// Payment transaction associated with an invoice.
/// </summary>
public sealed class StudentPaymentDto
{
    /// <summary>Transaction reference number.</summary>
    public string TransactionReference { get; set; } = string.Empty;
    
    /// <summary>Payment amount.</summary>
    public decimal Amount { get; set; }
    
    /// <summary>Date payment was made.</summary>
    public DateTime PaymentDate { get; set; }
    
    /// <summary>Payment method used.</summary>
    public string Mode { get; set; } = string.Empty;

    /// <summary>Payment verification status.</summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Dashboard summary for student portal.
/// </summary>
public sealed class StudentDashboardDto
{
    /// <summary>Attendance percentage (0-100).</summary>
    public decimal AttendancePercentage { get; set; }
    
    /// <summary>Latest term average percentage (0-100).</summary>
    public decimal LatestTermAverage { get; set; }
    
    /// <summary>Name of the latest term.</summary>
    public string LatestTerm { get; set; } = string.Empty;
    
    /// <summary>Total outstanding fee balance.</summary>
    public decimal FeeBalance { get; set; }
    
    /// <summary>Count of overdue invoices.</summary>
    public int OverdueInvoices { get; set; }
    
    /// <summary>Recent announcements from school administration.</summary>
    public List<AnnouncementDto> Announcements { get; set; } = new();
}

/// <summary>
/// School announcement visible to students.
/// </summary>
public sealed class AnnouncementDto
{
    /// <summary>Announcement title.</summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>Date the announcement was created.</summary>
    public DateTime CreatedAt { get; set; }
}