using SchoolErp.Domain.Common;

namespace SchoolErp.Domain.Entities;

/// <summary>A course/subject taught within the school.</summary>
public class Course : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }

    /// <summary>Optional link to the teacher (Identity user) responsible for the course.</summary>
    public string? TeacherUserId { get; set; }

    public Guid? SectionId { get; set; }
    public Section? Section { get; set; }

    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
