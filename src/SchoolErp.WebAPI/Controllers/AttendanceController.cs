using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Attendance.Dtos;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Common;
using SchoolErp.Domain.Entities;
using SchoolErp.Domain.Enums;
using System.Security.Claims;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AttendanceController(
        IAttendanceService attendanceService,
        IApplicationDbContext db,
        ICurrentUser currentUser)
    {
        _attendanceService = attendanceService;
        _db = db;
        _currentUser = currentUser;
    }

    [HttpPost("bulk")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> BulkMark([FromBody] BulkMarkAttendanceDto dto, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;
        if (tenantId == Guid.Empty)
            return Unauthorized(new { message = "Tenant information is required." });

        if (dto.Records is null || dto.Records.Count == 0)
            return BadRequest(new { message = "At least one attendance record is required." });

        var classExists = await _db.Sections
            .AsNoTracking()
            .AnyAsync(s => s.Id == dto.ClassId && s.TenantId == tenantId, ct);
        if (!classExists)
            return NotFound(new { message = "Class not found." });

        var studentIds = dto.Records.Select(r => r.StudentId).ToList();
        if (studentIds.Any(id => id == Guid.Empty) || studentIds.Distinct().Count() != studentIds.Count)
            return BadRequest(new { message = "Each attendance record must contain a unique student ID." });

        var validStudentCount = await _db.Students
            .AsNoTracking()
            .CountAsync(s => s.TenantId == tenantId && s.SectionId == dto.ClassId && studentIds.Contains(s.Id), ct);
        if (validStudentCount != studentIds.Count)
            return BadRequest(new { message = "All students must belong to the selected class." });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _attendanceService.BulkMarkAttendanceAsync(dto, userId);
        return Ok(new { message = "Attendance marked successfully" });
    }

    [HttpGet("daily")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> GetDaily([FromQuery] Guid classId, [FromQuery] DateTime date, CancellationToken ct)
    {
        var records = await _attendanceService.GetDailyAttendanceAsync(classId, date);
        return Ok(records);
    }

    [HttpGet("class-students")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> GetClassStudents([FromQuery] Guid classId, CancellationToken ct)
    {
        var students = await _attendanceService.GetClassStudentsAsync(classId);
        return Ok(students);
    }

    [HttpGet("export")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> Export(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] Guid? classId,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;

        IQueryable<AttendanceRecord> query = _db.AttendanceRecords
            .Where(a => a.TenantId == tenantId);

        if (dateFrom.HasValue) query = query.Where(a => a.Date >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(a => a.Date <= dateTo.Value);
        if (classId.HasValue) query = query.Where(a => a.ClassId == classId.Value);
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<AttendanceStatus>(status, true, out var parsedStatus))
                query = query.Where(a => a.Status == parsedStatus);
        }

        var records = await query
            .Include(a => a.Student)
            .Include(a => a.Class)
            .AsNoTracking()
            .ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance");

        var headers = new[]
        {
            "Date", "Student", "Admission Number", "Class", "Status", "Remarks"
        };

        for (int i = 0; i < headers.Length; i++)
            worksheet.Cell(1, i + 1).Value = headers[i];

        for (int row = 0; row < records.Count; row++)
        {
            var r = records[row];
            worksheet.Cell(row + 2, 1).Value = r.Date.ToString("yyyy-MM-dd");
            worksheet.Cell(row + 2, 2).Value = r.Student?.FullName ?? string.Empty;
            worksheet.Cell(row + 2, 3).Value = r.Student?.AdmissionNumber ?? string.Empty;
            worksheet.Cell(row + 2, 4).Value = r.Class?.Name ?? string.Empty;
            worksheet.Cell(row + 2, 5).Value = r.Status.ToString();
            worksheet.Cell(row + 2, 6).Value = r.Remarks ?? string.Empty;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Attendance.xlsx");
    }

    [HttpGet("template")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public IActionResult DownloadTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance");

        var headers = new[]
        {
            "Date", "Student", "Admission Number", "Class", "Status", "Remarks"
        };

        for (int i = 0; i < headers.Length; i++)
            worksheet.Cell(1, i + 1).Value = headers[i];

        worksheet.Row(2).Cell(1).Value = "2024-01-15";
        worksheet.Row(2).Cell(2).Value = "Jane Doe";
        worksheet.Row(2).Cell(3).Value = "ADM001";
        worksheet.Row(2).Cell(4).Value = "Grade 7 A";
        worksheet.Row(2).Cell(5).Value = "Present";
        worksheet.Row(2).Cell(6).Value = "";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Attendance-Template.xlsx");
    }

    [HttpGet("all")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] Guid? classId,
        CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;

        IQueryable<AttendanceRecord> query = _db.AttendanceRecords
            .Where(a => a.TenantId == tenantId);

        if (dateFrom.HasValue) query = query.Where(a => a.Date >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(a => a.Date <= dateTo.Value);
        if (classId.HasValue) query = query.Where(a => a.ClassId == classId.Value);

        return Ok(await query
            .Include(a => a.Student)
            .Include(a => a.Class)
            .AsNoTracking()
            .ToListAsync(ct));
    }
}