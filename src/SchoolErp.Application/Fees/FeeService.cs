using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;
using SchoolErp.Application.Fees.Dtos;
using SchoolErp.Domain.Entities;
using SchoolErp.Domain.Enums;

namespace SchoolErp.Application.Fees;

public class FeeService : IFeeService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public FeeService(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    // Invoice operations
    public async Task<IReadOnlyList<FeeInvoiceDto>> GetAllInvoicesAsync(CancellationToken ct = default)
    {
        return await _db.FeeInvoices
            .AsNoTracking()
            .Include(i => i.Student)
            .Include(i => i.Student!.Section)
            .Include(i => i.Discount)
            .Include(i => i.Items)
            .ThenInclude(item => item.FeeCategory)
            .Include(i => i.PaymentTransactions)
            .OrderByDescending(i => i.IssuedOnUtc)
            .Select(i => MapToDto(i))
            .ToListAsync(ct);
    }

    public async Task<Result<FeeInvoiceDto>> GetInvoiceByIdAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await _db.FeeInvoices
            .AsNoTracking()
            .Include(i => i.Student)
            .Include(i => i.Student!.Section)
            .Include(i => i.Discount)
            .Include(i => i.Items)
            .ThenInclude(item => item.FeeCategory)
            .Include(i => i.PaymentTransactions)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        return invoice is null
            ? Result<FeeInvoiceDto>.Failure("Invoice not found.")
            : Result<FeeInvoiceDto>.Success(MapToDto(invoice));
    }

    public async Task<Result<FeeInvoiceDto>> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken ct = default)
    {
        var student = await _db.Students
            .Include(s => s.Section)
            .FirstOrDefaultAsync(s => s.Id == request.StudentId, ct);
        
        if (student is null)
            return Result<FeeInvoiceDto>.Failure("Student not found.");

        Discount? discount = null;
        if (request.DiscountId.HasValue)
        {
            discount = await _db.Discounts.FirstOrDefaultAsync(d => d.Id == request.DiscountId.Value && d.IsActive, ct);
            if (discount is null)
                return Result<FeeInvoiceDto>.Failure("Discount not found or inactive.");
        }

        var invoiceNumber = await GenerateInvoiceNumberAsync(ct);
        var totalAmount = request.Items.Sum(i => i.Amount);

        var invoice = new FeeInvoice
        {
            InvoiceNumber = invoiceNumber,
            StudentId = request.StudentId,
            Description = request.Description,
            Amount = totalAmount,
            IssuedOnUtc = DateTime.UtcNow,
            DueDateUtc = request.DueDateUtc,
            DiscountId = request.DiscountId,
            DiscountAmount = discount != null ? CalculateDiscount(discount, totalAmount) : 0,
            AcademicYear = request.AcademicYear,
            Term = request.Term,
            SendNotification = request.SendNotification,
            GeneratePdf = request.GeneratePdf,
            Status = FeeInvoiceStatus.Pending
        };

        foreach (var item in request.Items)
        {
            var category = await _db.FeeCategories.FindAsync(new object[] { item.FeeCategoryId }, ct);
            if (category is null)
                return Result<FeeInvoiceDto>.Failure($"Fee category with ID {item.FeeCategoryId} not found.");

            invoice.Items.Add(new InvoiceItem
            {
                FeeCategoryId = item.FeeCategoryId,
                Description = item.Description,
                Amount = item.Amount,
                DisplayOrder = item.DisplayOrder
            });
        }

        _db.FeeInvoices.Add(invoice);
        await _db.SaveChangesAsync(ct);

        // TODO: Implement PDF generation if GeneratePdf is true
        // TODO: Implement notification sending if SendNotification is true

        var createdInvoice = await _db.FeeInvoices
            .AsNoTracking()
            .Include(i => i.Student)
            .Include(i => i.Student!.Section)
            .Include(i => i.Discount)
            .Include(i => i.Items)
            .ThenInclude(item => item.FeeCategory)
            .Include(i => i.PaymentTransactions)
            .FirstOrDefaultAsync(i => i.Id == invoice.Id, ct);

        return Result<FeeInvoiceDto>.Success(MapToDto(createdInvoice!));
    }

    public async Task<Result<StudentBalanceDto>> GetStudentBalanceAsync(Guid studentId, CancellationToken ct = default)
    {
        var student = await _db.Students
            .AsNoTracking()
            .Include(s => s.Section)
            .Include(s => s.FeeInvoices)
            .ThenInclude(i => i.PaymentTransactions)
            .FirstOrDefaultAsync(s => s.Id == studentId, ct);

        if (student is null)
            return Result<StudentBalanceDto>.Failure("Student not found.");

        var invoices = student.FeeInvoices.ToList();
        var totalOutstanding = invoices.Sum(i => i.Balance);
        var totalPaid = invoices.Sum(i => i.AmountPaid);
        var overdueInvoices = invoices.Count(i => i.Status == FeeInvoiceStatus.Overdue);

        var recentInvoices = invoices
            .OrderByDescending(i => i.IssuedOnUtc)
            .Take(5)
            .Select(i => new FeeInvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                StudentId = i.StudentId,
                StudentName = student.FullName,
                AdmissionNumber = student.AdmissionNumber,
                SectionName = student.Section?.Name,
                Description = i.Description,
                Amount = i.Amount,
                AmountPaid = i.AmountPaid,
                DiscountAmount = i.DiscountAmount,
                Balance = i.Balance,
                IssuedOnUtc = i.IssuedOnUtc,
                DueDateUtc = i.DueDateUtc,
                Status = i.Status,
                AcademicYear = i.AcademicYear,
                Term = i.Term,
                Items = i.Items.Select(item => new InvoiceItemDto
                {
                    Id = item.Id,
                    FeeCategoryId = item.FeeCategoryId,
                    CategoryName = item.FeeCategory?.Name ?? "",
                    Description = item.Description,
                    Amount = item.Amount,
                    DisplayOrder = item.DisplayOrder
                }).ToList(),
                PaymentTransactions = i.PaymentTransactions.Select(p => new PaymentTransactionDto
                {
                    Id = p.Id,
                    FeeInvoiceId = p.FeeInvoiceId,
                    InvoiceNumber = i.InvoiceNumber,
                    TransactionReference = p.TransactionReference,
                    PaymentMode = p.PaymentMode,
                    Amount = p.Amount,
                    PaymentDateUtc = p.PaymentDateUtc,
                    Notes = p.Notes,
                    Status = p.Status,
                    ReceivedByUserName = p.ReceivedByUserId
                }).ToList()
            })
            .ToList();

        return Result<StudentBalanceDto>.Success(new StudentBalanceDto
        {
            StudentId = student.Id,
            StudentName = student.FullName,
            AdmissionNumber = student.AdmissionNumber,
            TotalOutstanding = totalOutstanding,
            TotalPaid = totalPaid,
            OverdueInvoices = overdueInvoices,
            RecentInvoices = recentInvoices
        });
    }

    // Fee Template operations
    public async Task<IReadOnlyList<FeeTemplateDto>> GetAllFeeTemplatesAsync(CancellationToken ct = default)
    {
        return await _db.FeeTemplates
            .AsNoTracking()
            .Include(t => t.Items)
            .ThenInclude(i => i.FeeCategory)
            .Include(t => t.TargetClasses)
            .ThenInclude(tc => tc.Section)
            .OrderBy(t => t.Name)
            .Select(t => new FeeTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                BillingFrequency = t.BillingFrequency,
                IsActive = t.IsActive,
                Items = t.Items.Select(i => new FeeTemplateItemDto
                {
                    Id = i.Id,
                    FeeCategoryId = i.FeeCategoryId,
                    CategoryName = i.FeeCategory.Name,
                    Amount = i.Amount,
                    IsOptional = i.IsOptional,
                    DisplayOrder = i.DisplayOrder
                }).ToList(),
                TargetSectionIds = t.TargetClasses.Select(tc => tc.SectionId).ToList()
            })
            .ToListAsync(ct);
    }

    public async Task<Result<FeeTemplateDto>> CreateFeeTemplateAsync(CreateFeeTemplateRequest request, CancellationToken ct = default)
    {
        // Validate sections exist
        var sectionIds = request.TargetSectionIds.Distinct().ToList();
        var existingSections = await _db.Sections
            .Where(s => sectionIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync(ct);

        if (existingSections.Count != sectionIds.Count)
            return Result<FeeTemplateDto>.Failure("One or more sections not found.");

        var template = new FeeTemplate
        {
            Name = request.Name,
            Description = request.Description,
            BillingFrequency = request.BillingFrequency,
            IsActive = true
        };

        foreach (var item in request.Items)
        {
            var category = await _db.FeeCategories.FindAsync(new object[] { item.FeeCategoryId }, ct);
            if (category is null)
                return Result<FeeTemplateDto>.Failure($"Fee category with ID {item.FeeCategoryId} not found.");

            template.Items.Add(new FeeTemplateItem
            {
                FeeCategoryId = item.FeeCategoryId,
                Amount = item.Amount,
                IsOptional = item.IsOptional,
                DisplayOrder = item.DisplayOrder
            });
        }

        foreach (var sectionId in sectionIds)
        {
            template.TargetClasses.Add(new FeeTemplateClass
            {
                SectionId = sectionId
            });
        }

        _db.FeeTemplates.Add(template);
        await _db.SaveChangesAsync(ct);

        var createdTemplate = await _db.FeeTemplates
            .AsNoTracking()
            .Include(t => t.Items)
            .ThenInclude(i => i.FeeCategory)
            .Include(t => t.TargetClasses)
            .FirstOrDefaultAsync(t => t.Id == template.Id, ct);

        return Result<FeeTemplateDto>.Success(new FeeTemplateDto
        {
            Id = createdTemplate!.Id,
            Name = createdTemplate.Name,
            Description = createdTemplate.Description,
            BillingFrequency = createdTemplate.BillingFrequency,
            IsActive = createdTemplate.IsActive,
            Items = createdTemplate.Items.Select(i => new FeeTemplateItemDto
            {
                Id = i.Id,
                FeeCategoryId = i.FeeCategoryId,
                CategoryName = i.FeeCategory.Name,
                Amount = i.Amount,
                IsOptional = i.IsOptional,
                DisplayOrder = i.DisplayOrder
            }).ToList(),
            TargetSectionIds = createdTemplate.TargetClasses.Select(tc => tc.SectionId).ToList()
        });
    }

    public async Task<Result> GenerateInvoicesFromTemplateAsync(Guid templateId, string academicYear, string term, CancellationToken ct = default)
    {
        var template = await _db.FeeTemplates
            .Include(t => t.Items)
            .Include(t => t.TargetClasses)
            .ThenInclude(tc => tc.Section)
            .ThenInclude(s => s.Students)
            .FirstOrDefaultAsync(t => t.Id == templateId, ct);

        if (template is null)
            return Result.Failure("Template not found.");

        if (!template.IsActive)
            return Result.Failure("Template is inactive.");

        foreach (var templateClass in template.TargetClasses)
        {
            foreach (var student in templateClass.Section!.Students)
            {
                var invoiceNumber = await GenerateInvoiceNumberAsync(ct);
                var totalAmount = template.Items.Sum(i => i.Amount);

                var invoice = new FeeInvoice
                {
                    InvoiceNumber = invoiceNumber,
                    StudentId = student.Id,
                    Description = $"{template.Name} - {academicYear} {term}",
                    Amount = totalAmount,
                    IssuedOnUtc = DateTime.UtcNow,
                    DueDateUtc = DateTime.UtcNow.AddDays(30),
                    AcademicYear = academicYear,
                    Term = term,
                    Status = FeeInvoiceStatus.Pending
                };

                foreach (var item in template.Items)
                {
                    invoice.Items.Add(new InvoiceItem
                    {
                        FeeCategoryId = item.FeeCategoryId,
                        Description = item.FeeCategory.Name,
                        Amount = item.Amount,
                        DisplayOrder = item.DisplayOrder
                    });
                }

                _db.FeeInvoices.Add(invoice);
            }
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    // Payment operations
    public async Task<IReadOnlyList<PaymentTransactionDto>> GetAllPaymentsAsync(CancellationToken ct = default)
    {
        return await _db.PaymentTransactions
            .AsNoTracking()
            .Include(p => p.FeeInvoice)
            .OrderByDescending(p => p.PaymentDateUtc)
            .Select(p => new PaymentTransactionDto
            {
                Id = p.Id,
                FeeInvoiceId = p.FeeInvoiceId,
                InvoiceNumber = p.FeeInvoice.InvoiceNumber,
                TransactionReference = p.TransactionReference,
                PaymentMode = p.PaymentMode,
                Amount = p.Amount,
                PaymentDateUtc = p.PaymentDateUtc,
                Notes = p.Notes,
                Status = p.Status,
                ReceivedByUserName = p.ReceivedByUserId
            })
            .ToListAsync(ct);
    }

    public async Task<Result<PaymentTransactionDto>> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken ct = default)
    {
        var invoice = await _db.FeeInvoices
            .Include(i => i.PaymentTransactions)
            .FirstOrDefaultAsync(i => i.Id == request.FeeInvoiceId, ct);

        if (invoice is null)
            return Result<PaymentTransactionDto>.Failure("Invoice not found.");

        if (request.Amount > invoice.Balance)
            return Result<PaymentTransactionDto>.Failure($"Payment amount exceeds outstanding balance of {invoice.Balance:C}");

        var transaction = new PaymentTransaction
        {
            FeeInvoiceId = request.FeeInvoiceId,
            TransactionReference = request.TransactionReference,
            PaymentMode = request.PaymentMode,
            Amount = request.Amount,
            PaymentDateUtc = request.PaymentDateUtc,
            Notes = request.Notes,
            Status = PaymentStatus.Completed,
            ReceivedByUserId = _currentUser.UserId
        };

        invoice.AmountPaid += request.Amount;
        invoice.Status = invoice.Balance - request.Amount <= 0 ? FeeInvoiceStatus.Paid : FeeInvoiceStatus.PartiallyPaid;

        _db.PaymentTransactions.Add(transaction);
        await _db.SaveChangesAsync(ct);

        var createdTransaction = await _db.PaymentTransactions
            .AsNoTracking()
            .Include(p => p.FeeInvoice)
            .FirstOrDefaultAsync(p => p.Id == transaction.Id, ct);

        return Result<PaymentTransactionDto>.Success(new PaymentTransactionDto
        {
            Id = createdTransaction!.Id,
            FeeInvoiceId = createdTransaction.FeeInvoiceId,
            InvoiceNumber = createdTransaction.FeeInvoice.InvoiceNumber,
            TransactionReference = createdTransaction.TransactionReference,
            PaymentMode = createdTransaction.PaymentMode,
            Amount = createdTransaction.Amount,
            PaymentDateUtc = createdTransaction.PaymentDateUtc,
            Notes = createdTransaction.Notes,
            Status = createdTransaction.Status,
            ReceivedByUserName = createdTransaction.ReceivedByUserId
        });
    }

    // Fee Category operations
    public async Task<IReadOnlyList<FeeCategoryDto>> GetAllFeeCategoriesAsync(CancellationToken ct = default)
    {
        return await _db.FeeCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new FeeCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<Result<FeeCategoryDto>> CreateFeeCategoryAsync(CreateFeeCategoryRequest request, CancellationToken ct = default)
    {
        var exists = await _db.FeeCategories
            .AnyAsync(c => c.Code == request.Code, ct);

        if (exists)
            return Result<FeeCategoryDto>.Failure($"Fee category with code '{request.Code}' already exists.");

        var category = new FeeCategory
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            IsActive = true
        };

        _db.FeeCategories.Add(category);
        await _db.SaveChangesAsync(ct);

        return Result<FeeCategoryDto>.Success(new FeeCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Code = category.Code,
            Description = category.Description,
            IsActive = category.IsActive
        });
    }

    // Discount operations
    public async Task<IReadOnlyList<DiscountDto>> GetAllDiscountsAsync(CancellationToken ct = default)
    {
        return await _db.Discounts
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new DiscountDto
            {
                Id = d.Id,
                Code = d.Code,
                Name = d.Name,
                Description = d.Description,
                Type = d.Type,
                Value = d.Value,
                ValidFromUtc = d.ValidFromUtc,
                ValidUntilUtc = d.ValidUntilUtc,
                IsActive = d.IsActive,
                MaxUses = d.MaxUses,
                TimesUsed = d.TimesUsed
            })
            .ToListAsync(ct);
    }

    public async Task<Result<DiscountDto>> CreateDiscountAsync(CreateDiscountRequest request, CancellationToken ct = default)
    {
        var exists = await _db.Discounts
            .AnyAsync(d => d.Code == request.Code, ct);

        if (exists)
            return Result<DiscountDto>.Failure($"Discount with code '{request.Code}' already exists.");

        var discount = new Discount
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Value = request.Value,
            ValidFromUtc = request.ValidFromUtc,
            ValidUntilUtc = request.ValidUntilUtc,
            IsActive = true,
            MaxUses = request.MaxUses
        };

        _db.Discounts.Add(discount);
        await _db.SaveChangesAsync(ct);

        return Result<DiscountDto>.Success(new DiscountDto
        {
            Id = discount.Id,
            Code = discount.Code,
            Name = discount.Name,
            Description = discount.Description,
            Type = discount.Type,
            Value = discount.Value,
            ValidFromUtc = discount.ValidFromUtc,
            ValidUntilUtc = discount.ValidUntilUtc,
            IsActive = discount.IsActive,
            MaxUses = discount.MaxUses,
            TimesUsed = discount.TimesUsed
        });
    }

    private async Task<string> GenerateInvoiceNumberAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}";
        var lastInvoice = await _db.FeeInvoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(ct);

        int sequence = 1;
        if (lastInvoice != null)
        {
            var lastSequenceStr = lastInvoice.InvoiceNumber.Split('-').Last();
            if (int.TryParse(lastSequenceStr, out var lastSequence))
                sequence = lastSequence + 1;
        }

        return $"{prefix}-{sequence:D4}";
    }

    private static decimal CalculateDiscount(Discount discount, decimal amount)
    {
        return discount.Type == DiscountType.Percentage
            ? amount * (discount.Value / 100)
            : discount.Value;
    }

    private static FeeInvoiceDto MapToDto(FeeInvoice invoice)
    {
        return new FeeInvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            StudentId = invoice.StudentId,
            StudentName = invoice.Student?.FullName ?? "",
            AdmissionNumber = invoice.Student?.AdmissionNumber ?? "",
            SectionName = invoice.Student?.Section?.Name,
            Description = invoice.Description,
            Amount = invoice.Amount,
            AmountPaid = invoice.AmountPaid,
            DiscountAmount = invoice.DiscountAmount,
            Balance = invoice.Balance,
            IssuedOnUtc = invoice.IssuedOnUtc,
            DueDateUtc = invoice.DueDateUtc,
            Status = invoice.Status,
            AcademicYear = invoice.AcademicYear,
            Term = invoice.Term,
            Items = invoice.Items.Select(item => new InvoiceItemDto
            {
                Id = item.Id,
                FeeCategoryId = item.FeeCategoryId,
                CategoryName = item.FeeCategory?.Name ?? "",
                Description = item.Description,
                Amount = item.Amount,
                DisplayOrder = item.DisplayOrder
            }).ToList(),
            PaymentTransactions = invoice.PaymentTransactions.Select(p => new PaymentTransactionDto
            {
                Id = p.Id,
                FeeInvoiceId = p.FeeInvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                TransactionReference = p.TransactionReference,
                PaymentMode = p.PaymentMode,
                Amount = p.Amount,
                PaymentDateUtc = p.PaymentDateUtc,
                Notes = p.Notes,
                Status = p.Status,
                ReceivedByUserName = p.ReceivedByUserId
            }).ToList()
        };
    }
}
