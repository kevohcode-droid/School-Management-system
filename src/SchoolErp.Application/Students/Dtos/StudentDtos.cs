using System.ComponentModel.DataAnnotations;
using SchoolErp.Domain.Enums;
using SchoolErp.Domain.Entities;

namespace SchoolErp.Application.Students.Dtos;

// CreateStudentRequest moved to separate file: CreateStudentRequest.cs

public record StudentDto
{
    public Guid Id { get; init; }
    public string AdmissionNumber { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public DateTime EnrollmentDate { get; init; }
    public Guid? SectionId { get; init; }
    public string? SectionName { get; init; }
    public string? UserId { get; init; }
    public Guid TenantId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public string? UpdatedBy { get; init; }
    public Section? Section { get; init; }
}

/// <summary>Returned when an admin creates a student — includes the one-time temporary password.</summary>
public record CreateStudentResponse
{
    public StudentDto Student { get; init; } = null!;
    /// <summary>Temporary password shown once to the admin. Null if account already existed.</summary>
    public string? TemporaryPassword { get; init; }
    /// <summary>Login username for the student account.</summary>
    public string? Username { get; init; }
}

public record UpdateStudentRequest
{
    [Required]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    public string LastName { get; init; } = string.Empty;

    [EmailAddress]
    public string? Email { get; init; }

    public DateOnly? DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public Guid? SectionId { get; init; }
}

public record StudentImportRow
{
    public string AdmissionNumber { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public Gender Gender { get; init; } = Gender.Unspecified;
    public DateOnly? DateOfBirth { get; init; }
    public Guid? SectionId { get; init; }
    public string? SectionName { get; init; }
}