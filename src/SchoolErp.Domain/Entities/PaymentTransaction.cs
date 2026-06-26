using SchoolErp.Domain.Common;
using SchoolErp.Domain.Enums;

namespace SchoolErp.Domain.Entities;

public class PaymentTransaction : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid FeeInvoiceId { get; set; }
    public FeeInvoice FeeInvoice { get; set; } = null!;

    public string TransactionReference { get; set; } = string.Empty;
    public PaymentMode PaymentMode { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDateUtc { get; set; }
    public string? Notes { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Completed;

    public string? ReceivedByUserId { get; set; }
}
