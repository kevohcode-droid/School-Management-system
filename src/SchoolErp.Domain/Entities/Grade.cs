using SchoolErp.Domain.Common;
using SchoolErp.Domain.Enums;

namespace SchoolErp.Domain.Entities;

/// <summary>An examination/assessment result for a student in a course.</summary>
public class Grade : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public Guid CourseId { get; set; }
    public Course? Course { get; set; }

    public ExamType ExamType { get; set; }
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; } = 100m;
    public string? Remarks { get; set; }
    public DateTime AssessedOnUtc { get; set; }

    public decimal Percentage => MaxScore == 0 ? 0 : Math.Round(Score / MaxScore * 100m, 2);
}
