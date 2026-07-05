using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Entities;
using SchoolErp.Domain.Common;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SectionsController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public SectionsController(IApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var sections = await _db.Sections
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
        return Ok(sections);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var section = await _db.Sections.FirstOrDefaultAsync(s => s.Id == id, ct);
        return section is null ? NotFound() : Ok(section);
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    public async Task<IActionResult> Create([FromBody] CreateSectionRequest request, CancellationToken ct)
    {
        var section = new Section
        {
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
        var section = await _db.Sections.FirstOrDefaultAsync(s => s.Id == id, ct);
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
        var section = await _db.Sections.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (section is null) return NotFound();

        _db.Sections.Remove(section);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("export")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Teacher}")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var sections = await _db.Sections.AsNoTracking().OrderBy(s => s.Name).ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sections");

        var headers = new[] { "Name", "GradeLevel", "Capacity" };
        for (int i = 0; i < headers.Length; i++)
            worksheet.Cell(1, i + 1).Value = headers[i];

        for (int row = 0; row < sections.Count; row++)
        {
            var s = sections[row];
            worksheet.Cell(row + 2, 1).Value = s.Name;
            worksheet.Cell(row + 2, 2).Value = s.GradeLevel;
            worksheet.Cell(row + 2, 3).Value = s.Capacity;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Sections.xlsx");
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

        var imported = 0;
        foreach (var row in rows)
        {
            var name = row.Cell(1).GetString();
            if (string.IsNullOrWhiteSpace(name)) continue;

            var section = new Section
            {
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

        var headers = new[] { "Name", "GradeLevel", "Capacity" };
        for (int i = 0; i < headers.Length; i++)
            worksheet.Cell(1, i + 1).Value = headers[i];

        worksheet.Row(2).Cell(1).Value = "Grade 7 A";
        worksheet.Row(2).Cell(2).Value = "Grade 7";
        worksheet.Row(2).Cell(3).Value = 40;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Sections-Template.xlsx");
    }
}

public record CreateSectionRequest(string Name, string GradeLevel, int Capacity);
public record UpdateSectionRequest(string Name, string GradeLevel, int Capacity);