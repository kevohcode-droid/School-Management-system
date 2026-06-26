using SchoolErp.Domain.Common;
using SchoolErp.Domain.Enums;

namespace SchoolErp.Domain.Entities;

public class Discount : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType Type { get; set; } = DiscountType.Percentage;
    public decimal Value { get; set; }
    public DateTime? ValidFromUtc { get; set; }
    public DateTime? ValidUntilUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public int MaxUses { get; set; } = 0; // 0 = unlimited
    public int TimesUsed { get; set; } = 0;
}
