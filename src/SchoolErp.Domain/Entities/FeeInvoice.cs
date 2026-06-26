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
    public decimal DiscountAmount { get; set; } = 0;
    public Guid? DiscountId { get; set; }
    public Discount? Discount { get; set; }
    public DateTime IssuedOnUtc { get; set; }
    public DateTime DueDateUtc { get; set; }
    public FeeInvoiceStatus Status { get; set; } = FeeInvoiceStatus.Pending;
    public string? AcademicYear { get; set; }
    public string? Term { get; set; }
    public bool SendNotification { get; set; } = false;
    public bool GeneratePdf { get; set; } = false;

    public decimal Balance => Amount - AmountPaid - DiscountAmount;

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}
