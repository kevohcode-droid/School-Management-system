using SchoolErp.Domain.Common;

namespace SchoolErp.Domain.Entities;

/// <summary>A classroom/section that students are assigned to.</summary>
public class Section : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Grade/year level, e.g. "Grade 5".</summary>
    public string GradeLevel { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
