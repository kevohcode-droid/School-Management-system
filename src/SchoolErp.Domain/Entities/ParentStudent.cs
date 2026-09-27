using SchoolErp.Domain.Common;
using SchoolErp.Domain.Enums;

namespace SchoolErp.Domain.Entities;

public class ParentStudent : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid ParentId { get; set; }
    public Parent? Parent { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public RelationshipType RelationshipType { get; set; } = RelationshipType.Guardian;
    public bool IsPrimaryContact { get; set; }
}
