using SchoolErp.Domain.Common;

namespace SchoolErp.Domain.Entities;

public class TenantSetting : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
