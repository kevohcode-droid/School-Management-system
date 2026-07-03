using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Application.Attendance.Dtos;
using SchoolErp.Application.Common.Interfaces;
using System.Security.Claims;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkMark([FromBody] BulkMarkAttendanceDto dto, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _attendanceService.BulkMarkAttendanceAsync(dto, userId);
        return Ok(new { message = "Attendance marked successfully" });
    }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily([FromQuery] Guid classId, [FromQuery] DateTime date, CancellationToken ct)
    {
        var records = await _attendanceService.GetDailyAttendanceAsync(classId, date);
        return Ok(records);
    }

    [HttpGet("class-students")]
    public async Task<IActionResult> GetClassStudents([FromQuery] Guid classId, CancellationToken ct)
    {
        var students = await _attendanceService.GetClassStudentsAsync(classId);
        return Ok(students);
    }
}
