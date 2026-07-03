using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Auth.Dtos;
using SchoolErp.Application.Common.Interfaces;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _db;

    public AuthController(IIdentityService identityService, IApplicationDbContext db)
    {
        _identityService = identityService;
        _db = db;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await _identityService.RegisterAsync(request, ct);
        return result.Succeeded ? Ok(result.Response) : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await _identityService.LoginAsync(request, ct);
        return result.Succeeded ? Ok(result.Response) : Unauthorized(new { errors = result.Errors });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model, CancellationToken ct)
    {
        await _identityService.ForgotPasswordAsync(model.Email, ct);
        return Ok(new { Message = "If the email matches an account, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model, CancellationToken ct)
    {
        var result = await _identityService.ResetPasswordAsync(model, ct);
        return result.Succeeded ? Ok(result.Response) : BadRequest(new { errors = result.Errors });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me([FromServices] ICurrentUser currentUser, CancellationToken ct)
    {
        var tenant = currentUser.TenantId is { } tenantId
            ? await _db.Tenants
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == tenantId, ct)
            : null;

        return Ok(new
        {
            currentUser.UserId,
            currentUser.UserName,
            currentUser.FullName,
            currentUser.TenantId,
            TenantName = tenant?.Name,
            TenantCode = tenant?.Code,
            currentUser.Roles
        });
    }

    [HttpPost("google-signup")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleSignUp([FromBody] GoogleSignupRequest request, CancellationToken ct)
    {
        var result = await _identityService.GoogleSignupAsync(request, ct);
        return result.Succeeded ? Ok(result.Response) : BadRequest(new { errors = result.Errors });
    }
}