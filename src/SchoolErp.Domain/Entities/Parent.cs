using SchoolErp.Domain.Common;

namespace SchoolErp.Domain.Entities;

public class Parent : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    /// <summary>Optional link to the ASP.NET Identity user backing this parent.</summary>
    public string? UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Occupation { get; set; }

    public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
}
