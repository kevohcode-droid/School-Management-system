using SchoolErp.Domain.Common;

namespace SchoolErp.Domain.Entities;

public class FeeCategory : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<FeeTemplateItem> TemplateItems { get; set; } = new List<FeeTemplateItem>();
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
