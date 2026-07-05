using System.ComponentModel.DataAnnotations;
using SchoolErp.Domain.Enums;

namespace SchoolErp.Application.Students.Dtos;

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