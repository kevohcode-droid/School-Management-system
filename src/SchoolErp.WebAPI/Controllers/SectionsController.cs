using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Common;
using SchoolErp.Domain.Entities;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SectionsController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SectionsController(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;

        var sections = await _db.Sections
            .Where(s => s.TenantId == tenantId)
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
        return Ok(sections);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;
        var section = await _db.Sections.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, ct);
        return section is null ? NotFound() : Ok(section);
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    public async Task<IActionResult> Create([FromBody] CreateSectionRequest request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;

        var section = new Section
        {
            TenantId = tenantId,
            Name = request.Name,
            GradeLevel = request.GradeLevel,
            Capacity = request.Capacity
        };

        _db.Sections.Add(section);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = section.Id }, section);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSectionRequest request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;
        var section = await _db.Sections.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, ct);
        if (section is null) return NotFound();

        section.Name = request.Name;
        section.GradeLevel = request.GradeLevel;
        section.Capacity = request.Capacity;

        await _db.SaveChangesAsync(ct);
        return Ok(section);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;
        var section = await _db.Sections.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, ct);
        if (section is null) return NotFound();

        _db.Sections.Remove(section);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("export")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;

        var sections = await _db.Sections
            .Where(s => s.TenantId == tenantId)
            .AsNoTracking()
            .ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sections");

        var headers = new[] { "Class", "Code", "Grade Level", "Capacity", "Status" };
        for (int i = 0; i < headers.Length; i++)
            worksheet.Cell(1, i + 1).Value = headers[i];

        int rowNum = 2;
        foreach (var s in sections)
        {
            worksheet.Cell(rowNum, 1).Value = s.Name;
            worksheet.Cell(rowNum, 2).Value = s.GradeLevel;
            worksheet.Cell(rowNum, 3).Value = s.GradeLevel;
            worksheet.Cell(rowNum, 4).Value = s.Capacity;
            worksheet.Cell(rowNum, 5).Value = "Active";
            rowNum++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Classes.xlsx");
    }

    [HttpPost("import")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length <= 0)
            return BadRequest(new { errors = new[] { "No file uploaded." } });

        var tenantId = _currentUser.TenantId ?? Guid.Empty;

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, ct);
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();
        var rows = worksheet.RowsUsed().Skip(1);

        var imported = 0;
        foreach (var row in rows)
        {
            var name = row.Cell(1).GetString();
            if (string.IsNullOrWhiteSpace(name)) continue;

            var section = new Section
            {
                TenantId = tenantId,
                Name = name,
                GradeLevel = row.Cell(2).GetString(),
                Capacity = int.TryParse(row.Cell(3).GetString(), out var cap) ? cap : 0
            };

            _db.Sections.Add(section);
            imported++;
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { imported });
    }

    [HttpGet("template")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public IActionResult DownloadTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sections");

        var headers = new[] { "Class", "Code", "Grade Level", "Capacity", "Status" };
        for (int i = 0; i < headers.Length; i++)
            worksheet.Cell(1, i + 1).Value = headers[i];

        worksheet.Row(2).Cell(1).Value = "Grade 7 A";
        worksheet.Row(2).Cell(2).Value = "Grade 7";
        worksheet.Row(2).Cell(3).Value = "Grade 7";
        worksheet.Row(2).Cell(4).Value = 40;
        worksheet.Row(2).Cell(5).Value = "Active";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Classes-Template.xlsx");
    }
}

public record CreateSectionRequest(string Name, string GradeLevel, int Capacity);
public record UpdateSectionRequest(string Name, string GradeLevel, int Capacity);