using SchoolErp.Domain.Common;
using SchoolErp.Domain.Enums;

namespace SchoolErp.Domain.Entities;

public class Student : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    /// <summary>Optional link to the ASP.NET Identity user backing this student.</summary>
    public string? UserId { get; set; }

    public string AdmissionNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public Gender Gender { get; set; } = Gender.Unspecified;
    public DateTime EnrollmentDate { get; set; }

    public Guid? SectionId { get; set; }
    public Section? Section { get; set; }

    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    public ICollection<FeeInvoice> FeeInvoices { get; set; } = new List<FeeInvoice>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}
