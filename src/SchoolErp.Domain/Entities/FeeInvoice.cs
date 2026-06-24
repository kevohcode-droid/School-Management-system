using SchoolErp.Domain.Common;
using SchoolErp.Domain.Enums;

namespace SchoolErp.Domain.Entities;

public class FeeInvoice : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime IssuedOnUtc { get; set; }
    public DateTime DueDateUtc { get; set; }
    public FeeInvoiceStatus Status { get; set; } = FeeInvoiceStatus.Pending;

    public decimal Balance => Amount - AmountPaid;
}
