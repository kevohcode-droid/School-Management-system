using SchoolErp.Domain.Entities;

namespace SchoolErp.Application.Staff;

public record CreateStaffRequest
{
    public string EmployeeId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public int Gender { get; init; }
    public string? NationalId { get; init; }
    public string Designation { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string? DateOfJoining { get; init; }
    public string EmploymentStatus { get; init; } = string.Empty;
    public string? Qualifications { get; init; }
}

public record UpdateStaffRequest
{
    public string EmployeeId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public int Gender { get; init; }
    public string? NationalId { get; init; }
    public string Designation { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string? DateOfJoining { get; init; }
    public string EmploymentStatus { get; init; } = string.Empty;
    public string? Qualifications { get; init; }
}

public record StaffDto
{
    public Guid Id { get; init; }
    public string EmployeeId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public int Gender { get; init; }
    public string? NationalId { get; init; }
    public string Designation { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string? DateOfJoining { get; init; }
    public string EmploymentStatus { get; init; } = string.Empty;
    public string? Qualifications { get; init; }
}