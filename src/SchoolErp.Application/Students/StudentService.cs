using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;
using SchoolErp.Application.Students.Dtos;
using SchoolErp.Domain.Entities;

namespace SchoolErp.Application.Students;

public class StudentService : IStudentService
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _auditService;

    public StudentService(IApplicationDbContext db, IAuditService auditService)
    {
        _db = db;
        _auditService = auditService;
    }

    public async Task<IReadOnlyList<StudentDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Students
            .AsNoTracking()
            .OrderBy(s => s.LastName)
            .Select(s => new StudentDto
            {
                Id = s.Id,
                AdmissionNumber = s.AdmissionNumber,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                DateOfBirth = s.DateOfBirth,
                Gender = s.Gender,
                EnrollmentDate = s.EnrollmentDate,
                SectionId = s.SectionId,
                SectionName = s.Section != null ? s.Section.Name : null
            })
            .ToListAsync(ct);
    }

    public async Task<Result<StudentDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var student = await _db.Students
            .AsNoTracking()
            .Include(s => s.Section)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
        return student is null
            ? Result<StudentDto>.Failure("Student not found.")
            : Result<StudentDto>.Success(Map(student));
    }

    public async Task<Result<StudentDto>> CreateAsync(CreateStudentRequest request, CancellationToken ct = default)
    {
        var exists = await _db.Students.AnyAsync(s => s.AdmissionNumber == request.AdmissionNumber, ct);
        if (exists)
            return Result<StudentDto>.Failure($"A student with admission number '{request.AdmissionNumber}' already exists.");

        if (request.SectionId is { } sectionId && !await _db.Sections.AnyAsync(x => x.Id == sectionId, ct))
            return Result<StudentDto>.Failure("Section not found.");

        var student = new Student
        {
            AdmissionNumber = request.AdmissionNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            SectionId = request.SectionId,
            EnrollmentDate = DateTime.UtcNow
        };

        _db.Students.Add(student);
        await _db.SaveChangesAsync(ct);
        await _auditService.LogAsync("system", request.Email, "CREATE_STUDENT", $"Created student {student.FirstName} {student.LastName}", ct: ct);

        return Result<StudentDto>.Success(Map(student));
    }

    public async Task<Result<StudentDto>> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken ct = default)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student is null)
            return Result<StudentDto>.Failure("Student not found.");

        if (request.SectionId is { } sectionId && !await _db.Sections.AnyAsync(x => x.Id == sectionId, ct))
            return Result<StudentDto>.Failure("Section not found.");

        student.FirstName = request.FirstName;
        student.LastName = request.LastName;
        student.Email = request.Email;
        student.DateOfBirth = request.DateOfBirth;
        student.Gender = request.Gender;
        student.SectionId = request.SectionId;

        await _db.SaveChangesAsync(ct);
        await _auditService.LogAsync("system", request.Email, "UPDATE_STUDENT", $"Updated student {student.FirstName} {student.LastName}", ct: ct);
        return Result<StudentDto>.Success(Map(student));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student is null)
            return Result.Failure("Student not found.");

        _db.Students.Remove(student);
        await _db.SaveChangesAsync(ct);
        await _auditService.LogAsync("system", student.Email ?? "unknown", "DELETE_STUDENT", $"Deleted student {student.FirstName} {student.LastName}", ct: ct);
        return Result.Success();
    }

    public async Task<IReadOnlyList<Student>> GetForExportAsync(CancellationToken ct = default)
    {
        return await _db.Students
            .AsNoTracking()
            .Include(s => s.Section)
            .ToListAsync(ct);
    }

    public async Task<Result<int>> ImportFromRowsAsync(IReadOnlyList<StudentImportRow> rows, CancellationToken ct = default)
    {
        int importedCount = 0;
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.AdmissionNumber))
                continue;

            var existing = await _db.Students.FirstOrDefaultAsync(s => s.AdmissionNumber == row.AdmissionNumber, ct);
            if (existing != null)
                continue;

            Guid? sectionId = row.SectionId;
            if (sectionId == Guid.Empty && !string.IsNullOrWhiteSpace(row.SectionName))
            {
                var section = await _db.Sections.FirstOrDefaultAsync(s => s.Name == row.SectionName, ct);
                sectionId = section?.Id;
            }

            var student = new Student
            {
                AdmissionNumber = row.AdmissionNumber,
                FirstName = row.FirstName,
                LastName = row.LastName,
                Email = row.Email,
                Gender = row.Gender,
                DateOfBirth = row.DateOfBirth,
                SectionId = sectionId == Guid.Empty ? null : sectionId,
                EnrollmentDate = DateTime.UtcNow,
                TenantId = Guid.Empty
            };

            _db.Students.Add(student);
            importedCount++;
        }

        await _db.SaveChangesAsync(ct);
        return Result<int>.Success(importedCount, "Students imported successfully");
    }

    private static StudentDto Map(Student s) => new()
    {
        Id = s.Id,
        AdmissionNumber = s.AdmissionNumber,
        FirstName = s.FirstName,
        LastName = s.LastName,
        Email = s.Email,
        DateOfBirth = s.DateOfBirth,
        Gender = s.Gender,
        EnrollmentDate = s.EnrollmentDate,
        SectionId = s.SectionId,
        SectionName = s.Section != null ? s.Section.Name : null
    };
}