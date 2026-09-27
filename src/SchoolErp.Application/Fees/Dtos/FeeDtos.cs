using System.ComponentModel.DataAnnotations;
using SchoolErp.Domain.Enums;

namespace SchoolErp.Application.Fees.Dtos;

/// <summary>
/// Represents a fee invoice with all associated details.
/// </summary>
public record FeeInvoiceDto
{
    /// <summary>Unique identifier for the invoice.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Invoice number displayed to users.</summary>
    public string InvoiceNumber { get; init; } = string.Empty;
    
    /// <summary>Reference to the student this invoice is for.</summary>
    public Guid StudentId { get; init; }
    
    /// <summary>Full name of the student.</summary>
    public string StudentName { get; init; } = string.Empty;
    
    /// <summary>Student's admission number.</summary>
    public string AdmissionNumber { get; init; } = string.Empty;
    
    /// <summary>Name of the student's section/class.</summary>
    public string? SectionName { get; init; }
    
    /// <summary>Optional description or note on the invoice.</summary>
    public string? Description { get; init; }
    
    /// <summary>Total amount due for this invoice.</summary>
    public decimal Amount { get; init; }
    
    /// <summary>Total amount paid to date.</summary>
    public decimal AmountPaid { get; init; }
    
    /// <summary>Total discount applied.</summary>
    public decimal DiscountAmount { get; init; }
    
    /// <summary>Outstanding balance (Amount - AmountPaid - DiscountAmount).</summary>
    public decimal Balance { get; init; }
    
    /// <summary>Date when the invoice was issued.</summary>
    public DateTime IssuedOnUtc { get; init; }
    
    /// <summary>Due date for payment.</summary>
    public DateTime DueDateUtc { get; init; }
    
    /// <summary>Current status of the invoice.</summary>
    public FeeInvoiceStatus Status { get; init; }
    
    /// <summary>Academic year this invoice belongs to.</summary>
    public string? AcademicYear { get; init; }
    
    /// <summary>Term (e.g., "First Term", "Mid-Year").</summary>
    public string? Term { get; init; }
    
    /// <summary>List of individual invoice items.</summary>
    public List<InvoiceItemDto> Items { get; init; } = new();
    
    /// <summary>List of payment transactions made against this invoice.</summary>
    public List<PaymentTransactionDto> PaymentTransactions { get; init; } = new();
}

/// <summary>
/// Request payload for creating a new fee invoice.
/// </summary>
public record CreateInvoiceRequest
{
    /// <summary>Student to invoice.</summary>
    [Required]
    public Guid StudentId { get; init; }

    /// <summary>Academic year (e.g., "2024/2025").</summary>
    [Required]
    public string? AcademicYear { get; init; }

    /// <summary>Term identifier.</summary>
    [Required]
    public string? Term { get; init; }

    /// <summary>Optional description or note.</summary>
    public string? Description { get; init; }

    /// <summary>Due date for payment.</summary>
    [Required]
    public DateTime DueDateUtc { get; init; }

    /// <summary>Optional discount to apply.</summary>
    public Guid? DiscountId { get; init; }

    /// <summary>List of fee items to include on the invoice.</summary>
    [Required]
    public List<CreateInvoiceItemRequest> Items { get; init; } = new();

    /// <summary>Whether to send notification (e.g., email to parent).</summary>
    public bool SendNotification { get; init; } = false;
    
    /// <summary>Whether to generate a PDF receipt.</summary>
    public bool GeneratePdf { get; init; } = false;
}

/// <summary>
/// A single line item on a fee invoice.
/// </summary>
public record CreateInvoiceItemRequest
{
    /// <summary>Fee category to charge (e.g., Tuition, Examination).</summary>
    [Required]
    public Guid FeeCategoryId { get; init; }

    /// <summary>Description of the fee item.</summary>
    [Required]
    public string Description { get; init; } = string.Empty;

    /// <summary>Amount for this line item. Must be greater than zero.</summary>
    [Required]
    [Range(0.01, 999999999.99)]
    public decimal Amount { get; init; }

    /// <summary>Display order for sorting items on the invoice.</summary>
    public int DisplayOrder { get; init; } = 0;
}

/// <summary>
/// DTO for an individual invoice line item.
/// </summary>
public record InvoiceItemDto
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Reference to the fee category.</summary>
    public Guid FeeCategoryId { get; init; }
    
    /// <summary>Name of the fee category.</summary>
    public string CategoryName { get; init; } = string.Empty;
    
    /// <summary>Description of the fee item.</summary>
    public string Description { get; init; } = string.Empty;
    
    /// <summary>Amount for this item.</summary>
    public decimal Amount { get; init; }
    
    /// <summary>Display order.</summary>
    public int DisplayOrder { get; init; }
}

/// <summary>
/// DTO for a fee template that can generate multiple invoices at once.
/// </summary>
public record FeeTemplateDto
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Name of the fee template.</summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>Optional description.</summary>
    public string? Description { get; init; }
    
    /// <summary>Billing frequency for the template.</summary>
    public BillingFrequency BillingFrequency { get; init; }
    
    /// <summary>Whether this template is active.</summary>
    public bool IsActive { get; init; }
    
    /// <summary>List of fee items in the template.</summary>
    public List<FeeTemplateItemDto> Items { get; init; } = new();
    
    /// <summary>List of target section IDs.</summary>
    public List<Guid> TargetSectionIds { get; init; } = new();
}

/// <summary>
/// Request payload for creating a fee template.
/// </summary>
public record CreateFeeTemplateRequest
{
    /// <summary>Name for the fee template.</summary>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>Optional description.</summary>
    public string? Description { get; init; }

    /// <summary>Billing frequency.</summary>
    [Required]
    public BillingFrequency BillingFrequency { get; init; }

    /// <summary>Target section IDs to apply this template to.</summary>
    [Required]
    public List<Guid> TargetSectionIds { get; init; } = new();

    /// <summary>List of fee items.</summary>
    [Required]
    public List<CreateFeeTemplateItemRequest> Items { get; init; } = new();
}

/// <summary>
/// A single item within a fee template.
/// </summary>
public record CreateFeeTemplateItemRequest
{
    /// <summary>Fee category to charge.</summary>
    [Required]
    public Guid FeeCategoryId { get; init; }

    /// <summary>Amount for this item. Must be greater than zero.</summary>
    [Required]
    [Range(0.01, 999999999.99)]
    public decimal Amount { get; init; }

    /// <summary>Whether this item is optional.</summary>
    public bool IsOptional { get; init; } = false;
    
    /// <summary>Display order for sorting.</summary>
    public int DisplayOrder { get; init; } = 0;
}

/// <summary>
/// DTO for an item within a fee template.
/// </summary>
public record FeeTemplateItemDto
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Reference to the fee category.</summary>
    public Guid FeeCategoryId { get; init; }
    
    /// <summary>Name of the fee category.</summary>
    public string CategoryName { get; init; } = string.Empty;
    
    /// <summary>Amount for this item.</summary>
    public decimal Amount { get; init; }
    
    /// <summary>Whether this item is optional.</summary>
    public bool IsOptional { get; init; }
    
    /// <summary>Display order.</summary>
    public int DisplayOrder { get; init; }
}

/// <summary>
/// DTO for a payment transaction against a fee invoice.
/// </summary>
public record PaymentTransactionDto
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Reference to the invoice this payment applies to.</summary>
    public Guid FeeInvoiceId { get; init; }
    
    /// <summary>Invoice number for reference.</summary>
    public string InvoiceNumber { get; init; } = string.Empty;
    
    /// <summary>Transaction reference (e.g., bank ref, mobile pay code).</summary>
    public string TransactionReference { get; init; } = string.Empty;
    
    /// <summary>Mode of payment used.</summary>
    public PaymentMode PaymentMode { get; init; }
    
    /// <summary>Amount paid.</summary>
    public decimal Amount { get; init; }
    
    /// <summary>Date and time the payment was made.</summary>
    public DateTime PaymentDateUtc { get; init; }
    
    /// <summary>Optional notes about the payment.</summary>
    public string? Notes { get; init; }
    
    /// <summary>Current status of the payment.</summary>
    public PaymentStatus Status { get; init; }
    
    /// <summary>User ID who received the payment.</summary>
    public string? ReceivedByUserName { get; init; }
}

/// <summary>
/// Request payload for recording a new payment.
/// </summary>
public record CreatePaymentRequest
{
    /// <summary>Invoice to apply the payment to.</summary>
    [Required]
    public Guid FeeInvoiceId { get; init; }

    /// <summary>Unique transaction reference (e.g., bank receipt number).</summary>
    [Required]
    public string TransactionReference { get; init; } = string.Empty;

    /// <summary>Mode of payment.</summary>
    [Required]
    public PaymentMode PaymentMode { get; init; }

    /// <summary>Amount being paid. Must be greater than zero.</summary>
    [Required]
    [Range(0.01, 999999999.99)]
    public decimal Amount { get; init; }

    /// <summary>Date the payment was made.</summary>
    [Required]
    public DateTime PaymentDateUtc { get; init; }

    /// <summary>Optional notes about the payment.</summary>
    public string? Notes { get; init; }
}

/// <summary>
/// DTO for a fee category (e.g., Tuition, Examination Fees).
/// </summary>
public record FeeCategoryDto
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Name of the fee category.</summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>Unique code for the category.</summary>
    public string Code { get; init; } = string.Empty;
    
    /// <summary>Optional description.</summary>
    public string? Description { get; init; }
    
    /// <summary>Whether this category is active.</summary>
    public bool IsActive { get; init; }
}

/// <summary>
/// Request payload for creating a new fee category.
/// </summary>
public record CreateFeeCategoryRequest
{
    /// <summary>Name of the fee category.</summary>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>Unique code for the category.</summary>
    [Required]
    public string Code { get; init; } = string.Empty;

    /// <summary>Optional description.</summary>
    public string? Description { get; init; }
}

/// <summary>
/// DTO for a discount that can be applied to invoices.
/// </summary>
public record DiscountDto
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Discount code that can be entered by users.</summary>
    public string Code { get; init; } = string.Empty;
    
    /// <summary>Display name of the discount.</summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>Optional description of the discount.</summary>
    public string? Description { get; init; }
    
    /// <summary>Type of discount (Percentage or Fixed Amount).</summary>
    public DiscountType Type { get; init; }
    
    /// <summary>Value of the discount (percentage or fixed amount).</summary>
    public decimal Value { get; init; }
    
    /// <summary>Start of validity period (UTC).</summary>
    public DateTime? ValidFromUtc { get; init; }
    
    /// <summary>End of validity period (UTC).</summary>
    public DateTime? ValidUntilUtc { get; init; }
    
    /// <summary>Whether this discount is currently active.</summary>
    public bool IsActive { get; init; }
    
    /// <summary>Maximum number of times this discount can be used.</summary>
    public int MaxUses { get; init; }
    
    /// <summary>Current number of times this discount has been used.</summary>
    public int TimesUsed { get; init; }
}

/// <summary>
/// Request payload for creating a new discount.
/// </summary>
public record CreateDiscountRequest
{
    /// <summary>Unique discount code.</summary>
    [Required]
    public string Code { get; init; } = string.Empty;

    /// <summary>Display name of the discount.</summary>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>Optional description.</summary>
    public string? Description { get; init; }

    /// <summary>Type of discount.</summary>
    [Required]
    public DiscountType Type { get; init; }

    /// <summary>Value of the discount.</summary>
    [Required]
    [Range(0.01, 999999999.99)]
    public decimal Value { get; init; }

    /// <summary>Start of validity period (UTC).</summary>
    public DateTime? ValidFromUtc { get; init; }
    
    /// <summary>End of validity period (UTC).</summary>
    public DateTime? ValidUntilUtc { get; init; }

    /// <summary>Maximum uses allowed. Default is unlimited (0).</summary>
    public int MaxUses { get; init; } = 0;
}

/// <summary>
/// DTO for a student's outstanding fee balance summary.
/// </summary>
public record StudentBalanceDto
{
    /// <summary>Student identifier.</summary>
    public Guid StudentId { get; init; }
    
    /// <summary>Student's full name.</summary>
    public string StudentName { get; init; } = string.Empty;
    
    /// <summary>Student's admission number.</summary>
    public string AdmissionNumber { get; init; } = string.Empty;
    
    /// <summary>Total outstanding balance across all invoices.</summary>
    public decimal TotalOutstanding { get; init; }
    
    /// <summary>Total amount paid to date.</summary>
    public decimal TotalPaid { get; init; }
    
    /// <summary>Count of overdue invoices.</summary>
    public int OverdueInvoices { get; init; }
    
    /// <summary>Recently issued invoices (limited to most recent).</summary>
    public List<FeeInvoiceDto> RecentInvoices { get; init; } = new();
}