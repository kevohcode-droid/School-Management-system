using System.ComponentModel.DataAnnotations;
using SchoolErp.Domain.Common;
using SchoolErp.Domain.Enums;

namespace SchoolErp.Domain.Entities;

public class FeeTemplate : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BillingFrequency BillingFrequency { get; set; } = BillingFrequency.Termly;
    public bool IsActive { get; set; } = true;

    public ICollection<FeeTemplateItem> Items { get; set; } = new List<FeeTemplateItem>();
    public ICollection<FeeTemplateClass> TargetClasses { get; set; } = new List<FeeTemplateClass>();
}

public class FeeTemplateItem : AuditableEntity
{
    public Guid FeeTemplateId { get; set; }
    public FeeTemplate FeeTemplate { get; set; } = null!;

    public Guid FeeCategoryId { get; set; }
    public FeeCategory FeeCategory { get; set; } = null!;

    public decimal Amount { get; set; }
    public bool IsOptional { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
}

public class FeeTemplateClass
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FeeTemplateId { get; set; }
    public FeeTemplate FeeTemplate { get; set; } = null!;

    public Guid SectionId { get; set; }
    public Section Section { get; set; } = null!;
}
