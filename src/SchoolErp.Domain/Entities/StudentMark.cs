using SchoolErp.Domain.Common;

namespace SchoolErp.Domain.Entities;

public class StudentMark : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string ExamTerm { get; set; } = string.Empty; // e.g., "Term 1 - 2026"
    public decimal ScoreObtained { get; set; }
    public decimal MaxScore { get; set; } = 100m;
    public DateTime DateRecorded { get; set; } = DateTime.UtcNow;
    public string ReviewStatus { get; set; } = "Submitted";
    public string? ReviewComment { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }

    public decimal Percentage => MaxScore == 0 ? 0 : Math.Round(ScoreObtained / MaxScore * 100m, 2);
}
