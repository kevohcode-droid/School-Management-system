using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Application.Fees;
using SchoolErp.Application.Fees.Dtos;
using SchoolErp.Domain.Common;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Accountant}")]
public class FeesController : ControllerBase
{
    private readonly IFeeService _feeService;

    public FeesController(IFeeService feeService)
    {
        _feeService = feeService;
    }

    // Invoices
    [HttpGet("invoices")]
    public async Task<IActionResult> GetAllInvoices(CancellationToken ct)
    {
        var invoices = await _feeService.GetAllInvoicesAsync(ct);
        return Ok(invoices);
    }

    [HttpGet("invoices/{id:guid}")]
    public async Task<IActionResult> GetInvoiceById(Guid id, CancellationToken ct)
    {
        var result = await _feeService.GetInvoiceByIdAsync(id, ct);
        return result.Succeeded ? Ok(result.Value) : NotFound(new { errors = result.Errors });
    }

    [HttpPost("invoices")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Accountant}")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request, CancellationToken ct)
    {
        var result = await _feeService.CreateInvoiceAsync(request, ct);
        return result.Succeeded 
            ? CreatedAtAction(nameof(GetInvoiceById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(new { errors = result.Errors });
    }

    [HttpGet("students/{studentId:guid}/balance")]
    public async Task<IActionResult> GetStudentBalance(Guid studentId, CancellationToken ct)
    {
        var result = await _feeService.GetStudentBalanceAsync(studentId, ct);
        return result.Succeeded ? Ok(result.Value) : NotFound(new { errors = result.Errors });
    }

    // Fee Templates
    [HttpGet("templates")]
    public async Task<IActionResult> GetAllFeeTemplates(CancellationToken ct)
    {
        var templates = await _feeService.GetAllFeeTemplatesAsync(ct);
        return Ok(templates);
    }

    [HttpPost("templates")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Accountant}")]
    public async Task<IActionResult> CreateFeeTemplate([FromBody] CreateFeeTemplateRequest request, CancellationToken ct)
    {
        var result = await _feeService.CreateFeeTemplateAsync(request, ct);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("templates/{id:guid}/generate-invoices")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Accountant}")]
    public async Task<IActionResult> GenerateInvoicesFromTemplate(Guid id, [FromQuery] string academicYear, [FromQuery] string term, CancellationToken ct)
    {
        var result = await _feeService.GenerateInvoicesFromTemplateAsync(id, academicYear, term, ct);
        return result.Succeeded ? Ok(new { message = "Invoices generated successfully." }) : BadRequest(new { errors = result.Errors });
    }

    // Payment Records & Options
    [HttpGet("payments")]
    public async Task<IActionResult> GetAllPayments(CancellationToken ct)
    {
        var payments = await _feeService.GetAllPaymentsAsync(ct);
        return Ok(payments);
    }

    [HttpPost("payments")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Accountant}")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request, CancellationToken ct)
    {
        var result = await _feeService.CreatePaymentAsync(request, ct);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    // Fee Categories
    [HttpGet("categories")]
    public async Task<IActionResult> GetAllFeeCategories(CancellationToken ct)
    {
        var categories = await _feeService.GetAllFeeCategoriesAsync(ct);
        return Ok(categories);
    }

    [HttpPost("categories")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Accountant}")]
    public async Task<IActionResult> CreateFeeCategory([FromBody] CreateFeeCategoryRequest request, CancellationToken ct)
    {
        var result = await _feeService.CreateFeeCategoryAsync(request, ct);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    // Discounts
    [HttpGet("discounts")]
    public async Task<IActionResult> GetAllDiscounts(CancellationToken ct)
    {
        var discounts = await _feeService.GetAllDiscountsAsync(ct);
        return Ok(discounts);
    }

    [HttpPost("discounts")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Accountant}")]
    public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountRequest request, CancellationToken ct)
    {
        var result = await _feeService.CreateDiscountAsync(request, ct);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }
}
