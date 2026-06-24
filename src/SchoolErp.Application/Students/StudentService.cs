using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;
using SchoolErp.Application.Students.Dtos;
using SchoolErp.Domain.Entities;

namespace SchoolErp.Application.Students;

public class StudentService : IStudentService
{
    private readonly IApplicationDbContext _db;

    public StudentService(IApplicationDbContext db) => _db = db;

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
        return Result<StudentDto>.Success(Map(student));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student is null)
            return Result.Failure("Student not found.");

        _db.Students.Remove(student);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
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
