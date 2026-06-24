using System.ComponentModel.DataAnnotations;
using SchoolErp.Domain.Enums;

namespace SchoolErp.Application.Students.Dtos;

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
}

public record CreateStudentRequest
{
    [Required]
    public string AdmissionNumber { get; init; } = string.Empty;

    [Required]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    public string LastName { get; init; } = string.Empty;

    [EmailAddress]
    public string? Email { get; init; }

    public DateOnly? DateOfBirth { get; init; }
    public Gender Gender { get; init; } = Gender.Unspecified;
    public Guid? SectionId { get; init; }
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
