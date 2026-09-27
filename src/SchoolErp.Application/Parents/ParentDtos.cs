using SchoolErp.Domain.Enums;

namespace SchoolErp.Application.Parents;

public record ParentDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string? UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? Occupation { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public record CreateParentRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? Occupation { get; init; }
}

public record UpdateParentRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? Occupation { get; init; }
}

public record CreateParentResponse
{
    public ParentDto Parent { get; init; } = null!;
    /// <summary>Temporary password shown once to the admin. Null if account already existed.</summary>
    public string? TemporaryPassword { get; init; }
    /// <summary>Username generated for the parent's login account.</summary>
    public string? Username { get; init; }
}

public record ParentStudentLinkDto
{
    public Guid ParentId { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string AdmissionNumber { get; init; } = string.Empty;
    public RelationshipType RelationshipType { get; init; }
    public bool IsPrimaryContact { get; init; }
}
