using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Application.Students;
using SchoolErp.Application.Students.Dtos;
using SchoolErp.Domain.Common;
using SchoolErp.Domain.Enums;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _students;

    public StudentsController(IStudentService students) => _students = students;

    [HttpGet]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _students.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _students.GetByIdAsync(id, ct);
        return result.Succeeded ? Ok(result.Value) : NotFound(new { errors = result.Errors });
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    public async Task<IActionResult> Create(CreateStudentRequest request, CancellationToken ct)
    {
        var result = await _students.CreateAsync(request, ct);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(new { errors = result.Errors });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    public async Task<IActionResult> Update(Guid id, UpdateStudentRequest request, CancellationToken ct)
    {
        var result = await _students.UpdateAsync(id, request, ct);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _students.DeleteAsync(id, ct);
        return result.Succeeded ? NoContent() : NotFound(new { errors = result.Errors });
    }

    [HttpGet("export")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var students = await _students.GetForExportAsync(ct);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Students");

        var headers = new[]
        {
            "AdmissionNumber", "FirstName", "LastName", "Email", "Gender", "DateOfBirth", "SectionName"
        };

        for (int i = 0; i < headers.Length; i++)
            worksheet.Cell(1, i + 1).Value = headers[i];

        for (int row = 0; row < students.Count; row++)
        {
            var s = students[row];
            worksheet.Cell(row + 2, 1).Value = s.AdmissionNumber;
            worksheet.Cell(row + 2, 2).Value = s.FirstName;
            worksheet.Cell(row + 2, 3).Value = s.LastName;
            worksheet.Cell(row + 2, 4).Value = s.Email ?? string.Empty;
            worksheet.Cell(row + 2, 5).Value = s.Gender.ToString();
            worksheet.Cell(row + 2, 6).Value = s.DateOfBirth?.ToString() ?? string.Empty;
            worksheet.Cell(row + 2, 7).Value = s.EnrollmentDate;
            worksheet.Cell(row + 2, 8).Value = s.SectionId.HasValue ? s.SectionId.ToString() : string.Empty;
            worksheet.Cell(row + 2, 9).Value = s.Section?.Name ?? string.Empty;
            worksheet.Cell(row + 2, 10).Value = s.Id.ToString();
            worksheet.Cell(row + 2, 11).Value = s.TenantId.ToString();
            worksheet.Cell(row + 2, 12).Value = s.UserId ?? string.Empty;
            worksheet.Cell(row + 2, 13).Value = s.CreatedAtUtc;
            worksheet.Cell(row + 2, 14).Value = s.CreatedBy ?? string.Empty;
            worksheet.Cell(row + 2, 15).Value = s.UpdatedAtUtc;
            worksheet.Cell(row + 2, 16).Value = s.UpdatedBy ?? string.Empty;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Students.xlsx");
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

        var importRows = rows.Select(row =>
        {
            var sectionName = row.Cell(7).GetString();

            return new StudentImportRow
            {
                AdmissionNumber = row.Cell(1).GetString(),
                FirstName = row.Cell(2).GetString(),
                LastName = row.Cell(3).GetString(),
                Email = row.Cell(4).GetString(),
                Gender = Enum.TryParse<Gender>(row.Cell(5).GetString(), true, out var g) ? g : Gender.Unspecified,
                DateOfBirth = DateOnly.TryParse(row.Cell(6).GetString(), out var dob) ? dob : (DateOnly?)null,
                SectionId = Guid.Empty,
                SectionName = sectionName
            };
        }).ToList();

        var result = await _students.ImportFromRowsAsync(importRows, ct);
        return result.Succeeded ? Ok(new { imported = result.Value }) : BadRequest(new { errors = result.Errors });
    }

    [HttpGet("template")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public IActionResult DownloadTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Students");

        var headers = new[]
        {
            "AdmissionNumber", "FirstName", "LastName", "Email", "Gender", "DateOfBirth", "SectionName"
        };

        for (int i = 0; i < headers.Length; i++)
            worksheet.Cell(1, i + 1).Value = headers[i];

        var exampleRow = worksheet.Row(2);
        exampleRow.Cell(1).Value = "ADM001";
        exampleRow.Cell(2).Value = "John";
        exampleRow.Cell(3).Value = "Doe";
        exampleRow.Cell(4).Value = "john@example.com";
        exampleRow.Cell(5).Value = "Male";
        exampleRow.Cell(6).Value = "2010-01-15";
        exampleRow.Cell(7).Value = "Grade 7 A";

        var exampleRow2 = worksheet.Row(3);
        exampleRow2.Cell(1).Value = "ADM002";
        exampleRow2.Cell(2).Value = "Jane";
        exampleRow2.Cell(3).Value = "Smith";
        exampleRow2.Cell(4).Value = "jane@example.com";
        exampleRow2.Cell(5).Value = "Female";
        exampleRow2.Cell(6).Value = "2009-05-20";
        exampleRow2.Cell(7).Value = "Grade 8 B";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Students-Template.xlsx");
    }
}