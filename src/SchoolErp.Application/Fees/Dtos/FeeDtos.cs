using System.ComponentModel.DataAnnotations;
using SchoolErp.Domain.Enums;

namespace SchoolErp.Application.Fees.Dtos;

// Invoice DTOs
public record FeeInvoiceDto
{
    public Guid Id { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string AdmissionNumber { get; init; } = string.Empty;
    public string? SectionName { get; init; }
    public string? Description { get; init; }
    public decimal Amount { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal Balance { get; init; }
    public DateTime IssuedOnUtc { get; init; }
    public DateTime DueDateUtc { get; init; }
    public FeeInvoiceStatus Status { get; init; }
    public string? AcademicYear { get; init; }
    public string? Term { get; init; }
    public List<InvoiceItemDto> Items { get; init; } = new();
    public List<PaymentTransactionDto> PaymentTransactions { get; init; } = new();
}

public record CreateInvoiceRequest
{
    [Required]
    public Guid StudentId { get; init; }

    [Required]
    public string? AcademicYear { get; init; }

    [Required]
    public string? Term { get; init; }

    public string? Description { get; init; }

    [Required]
    public DateTime DueDateUtc { get; init; }

    public Guid? DiscountId { get; init; }

    [Required]
    public List<CreateInvoiceItemRequest> Items { get; init; } = new();

    public bool SendNotification { get; init; } = false;
    public bool GeneratePdf { get; init; } = false;
}

public record CreateInvoiceItemRequest
{
    [Required]
    public Guid FeeCategoryId { get; init; }

    [Required]
    public string Description { get; init; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; init; }

    public int DisplayOrder { get; init; } = 0;
}

public record InvoiceItemDto
{
    public Guid Id { get; init; }
    public Guid FeeCategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public int DisplayOrder { get; init; }
}

// Fee Template DTOs
public record FeeTemplateDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public BillingFrequency BillingFrequency { get; init; }
    public bool IsActive { get; init; }
    public List<FeeTemplateItemDto> Items { get; init; } = new();
    public List<Guid> TargetSectionIds { get; init; } = new();
}

public record CreateFeeTemplateRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    [Required]
    public BillingFrequency BillingFrequency { get; init; }

    [Required]
    public List<Guid> TargetSectionIds { get; init; } = new();

    [Required]
    public List<CreateFeeTemplateItemRequest> Items { get; init; } = new();
}

public record CreateFeeTemplateItemRequest
{
    [Required]
    public Guid FeeCategoryId { get; init; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; init; }

    public bool IsOptional { get; init; } = false;
    public int DisplayOrder { get; init; } = 0;
}

public record FeeTemplateItemDto
{
    public Guid Id { get; init; }
    public Guid FeeCategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public bool IsOptional { get; init; }
    public int DisplayOrder { get; init; }
}

// Payment Transaction DTOs
public record PaymentTransactionDto
{
    public Guid Id { get; init; }
    public Guid FeeInvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public string TransactionReference { get; init; } = string.Empty;
    public PaymentMode PaymentMode { get; init; }
    public decimal Amount { get; init; }
    public DateTime PaymentDateUtc { get; init; }
    public string? Notes { get; init; }
    public PaymentStatus Status { get; init; }
    public string? ReceivedByUserName { get; init; }
}

public record CreatePaymentRequest
{
    [Required]
    public Guid FeeInvoiceId { get; init; }

    [Required]
    public string TransactionReference { get; init; } = string.Empty;

    [Required]
    public PaymentMode PaymentMode { get; init; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; init; }

    [Required]
    public DateTime PaymentDateUtc { get; init; }

    public string? Notes { get; init; }
}

// Fee Category DTOs
public record FeeCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

public record CreateFeeCategoryRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Code { get; init; } = string.Empty;

    public string? Description { get; init; }
}

// Discount DTOs
public record DiscountDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DiscountType Type { get; init; }
    public decimal Value { get; init; }
    public DateTime? ValidFromUtc { get; init; }
    public DateTime? ValidUntilUtc { get; init; }
    public bool IsActive { get; init; }
    public int MaxUses { get; init; }
    public int TimesUsed { get; init; }
}

public record CreateDiscountRequest
{
    [Required]
    public string Code { get; init; } = string.Empty;

    [Required]
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    [Required]
    public DiscountType Type { get; init; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Value { get; init; }

    public DateTime? ValidFromUtc { get; init; }
    public DateTime? ValidUntilUtc { get; init; }

    public int MaxUses { get; init; } = 0;
}

// Student Balance DTO
public record StudentBalanceDto
{
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string AdmissionNumber { get; init; } = string.Empty;
    public decimal TotalOutstanding { get; init; }
    public decimal TotalPaid { get; init; }
    public int OverdueInvoices { get; init; }
    public List<FeeInvoiceDto> RecentInvoices { get; init; } = new();
}
