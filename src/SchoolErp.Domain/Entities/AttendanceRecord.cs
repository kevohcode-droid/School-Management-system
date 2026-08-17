using SchoolErp.Domain.Common;
using SchoolErp.Domain.Enums;

namespace SchoolErp.Domain.Entities;

public class AttendanceRecord : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
    public Guid MarkedByUserId { get; set; }

    public Student Student { get; set; } = null!;
    public Section Class { get; set; } = null!;
}
