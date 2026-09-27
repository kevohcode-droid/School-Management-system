using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Application.Staff;
using SchoolErp.Domain.Common;
using SchoolErp.Domain.Entities;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        return Ok(await _staffService.GetAllAsync(ct));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    public async Task<IActionResult> Create([FromBody] CreateStaffRequest request, CancellationToken ct)
    {
        var result = await _staffService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { id = result.Staff.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStaffRequest request, CancellationToken ct)
    {
        var result = await _staffService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _staffService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("export")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var staff = await _staffService.GetForExportAsync(ct);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Staff");

        var headers = new[]
        {
            "Employee ID / TSC Number", "Designation / Role", "Department", "Date of Joining",
            "Employment Status", "Qualifications / Degree", "First Name", "Last Name",
            "Gender", "National ID / Passport Number", "Phone Number", "Professional Email"
        };

        for (int i = 0; i < headers.Length; i++)
            worksheet.Cell(1, i + 1).Value = headers[i];

        for (int row = 0; row < staff.Count; row++)
        {
            var s = staff[row];
            worksheet.Cell(row + 2, 1).Value = s.EmployeeId;
            worksheet.Cell(row + 2, 2).Value = s.Designation;
            worksheet.Cell(row + 2, 3).Value = s.Department;
            worksheet.Cell(row + 2, 4).Value = s.DateOfJoining ?? string.Empty;
            worksheet.Cell(row + 2, 5).Value = s.EmploymentStatus;
            worksheet.Cell(row + 2, 6).Value = s.Qualifications ?? string.Empty;
            worksheet.Cell(row + 2, 7).Value = s.FirstName;
            worksheet.Cell(row + 2, 8).Value = s.LastName;
            worksheet.Cell(row + 2, 9).Value = s.Gender;
            worksheet.Cell(row + 2, 10).Value = s.NationalId ?? string.Empty;
            worksheet.Cell(row + 2, 11).Value = s.Phone ?? string.Empty;
            worksheet.Cell(row + 2, 12).Value = s.Email ?? string.Empty;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Staff.xlsx");
    }

    [HttpPost("import")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length <= 0)
            return BadRequest(new { errors = new[] { "No file uploaded." } });

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, ct);
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();
        var rows = worksheet.RowsUsed().Skip(1);

        var importRows = rows.Select(row => new StaffImportRow
        {
            EmployeeId = row.Cell(1).GetString(),
            Designation = row.Cell(2).GetString(),
            Department = row.Cell(3).GetString(),
            DateOfJoining = row.Cell(4).GetString(),
            EmploymentStatus = row.Cell(5).GetString(),
            Qualifications = row.Cell(6).GetString(),
            FirstName = row.Cell(7).GetString(),
            LastName = row.Cell(8).GetString(),
            Gender = int.TryParse(row.Cell(9).GetString(), out var g) ? g : 0,
            NationalId = row.Cell(10).GetString(),
            Phone = row.Cell(11).GetString(),
            Email = row.Cell(12).GetString()
        }).ToList();

        var imported = await _staffService.ImportFromRowsAsync(importRows, ct);
        return Ok(new { imported });
    }

    [HttpGet("template")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public IActionResult DownloadTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Staff");

        var headers = new[]
        {
            "Employee ID / TSC Number", "Designation / Role", "Department", "Date of Joining",
            "Employment Status", "Qualifications / Degree", "First Name", "Last Name",
            "Gender", "National ID / Passport Number", "Phone Number", "Professional Email"
        };

        for (int i = 0; i < headers.Length; i++)
            worksheet.Cell(1, i + 1).Value = headers[i];

        worksheet.Row(2).Cell(1).Value = "EMP-1001";
        worksheet.Row(2).Cell(2).Value = "Teacher";
        worksheet.Row(2).Cell(3).Value = "Sciences";
        worksheet.Row(2).Cell(4).Value = "2024-01-15";
        worksheet.Row(2).Cell(5).Value = "Full-time";
        worksheet.Row(2).Cell(6).Value = "B.Ed Mathematics";
        worksheet.Row(2).Cell(7).Value = "Jane";
        worksheet.Row(2).Cell(8).Value = "Doe";
        worksheet.Row(2).Cell(9).Value = "2";
        worksheet.Row(2).Cell(10).Value = "12345678";
        worksheet.Row(2).Cell(11).Value = "+254 700 000 000";
        worksheet.Row(2).Cell(12).Value = "jane@school.edu";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Staff-Template.xlsx");
    }
}