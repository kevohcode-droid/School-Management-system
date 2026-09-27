using SchoolErp.Domain.Common;

namespace SchoolErp.Domain.Entities;

public class StaffMember : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    /// <summary>Optional link to the ASP.NET Identity user backing this staff member.</summary>
    public string? UserId { get; set; }

    public string EmployeeId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int Gender { get; set; }
    public string? NationalId { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? DateOfJoining { get; set; }
    public string EmploymentStatus { get; set; } = string.Empty;
    public string? Qualifications { get; set; }
}