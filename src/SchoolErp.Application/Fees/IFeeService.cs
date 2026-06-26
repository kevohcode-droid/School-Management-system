using SchoolErp.Application.Common.Models;
using SchoolErp.Application.Fees.Dtos;

namespace SchoolErp.Application.Fees;

public interface IFeeService
{
    // Invoice operations
    Task<IReadOnlyList<FeeInvoiceDto>> GetAllInvoicesAsync(CancellationToken ct = default);
    Task<Result<FeeInvoiceDto>> GetInvoiceByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<FeeInvoiceDto>> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken ct = default);
    Task<Result<StudentBalanceDto>> GetStudentBalanceAsync(Guid studentId, CancellationToken ct = default);

    // Fee Template operations
    Task<IReadOnlyList<FeeTemplateDto>> GetAllFeeTemplatesAsync(CancellationToken ct = default);
    Task<Result<FeeTemplateDto>> CreateFeeTemplateAsync(CreateFeeTemplateRequest request, CancellationToken ct = default);
    Task<Result> GenerateInvoicesFromTemplateAsync(Guid templateId, string academicYear, string term, CancellationToken ct = default);

    // Payment operations
    Task<IReadOnlyList<PaymentTransactionDto>> GetAllPaymentsAsync(CancellationToken ct = default);
    Task<Result<PaymentTransactionDto>> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken ct = default);

    // Fee Category operations
    Task<IReadOnlyList<FeeCategoryDto>> GetAllFeeCategoriesAsync(CancellationToken ct = default);
    Task<Result<FeeCategoryDto>> CreateFeeCategoryAsync(CreateFeeCategoryRequest request, CancellationToken ct = default);

    // Discount operations
    Task<IReadOnlyList<DiscountDto>> GetAllDiscountsAsync(CancellationToken ct = default);
    Task<Result<DiscountDto>> CreateDiscountAsync(CreateDiscountRequest request, CancellationToken ct = default);
}
