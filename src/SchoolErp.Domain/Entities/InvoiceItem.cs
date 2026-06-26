using SchoolErp.Domain.Common;

namespace SchoolErp.Domain.Entities;

public class InvoiceItem : AuditableEntity
{
    public Guid FeeInvoiceId { get; set; }
    public FeeInvoice FeeInvoice { get; set; } = null!;

    public Guid FeeCategoryId { get; set; }
    public FeeCategory FeeCategory { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int DisplayOrder { get; set; } = 0;
}
