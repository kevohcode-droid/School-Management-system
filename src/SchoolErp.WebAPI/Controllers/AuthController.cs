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
    private readonly SchoolErp.Infrastructure.Persistence.ApplicationDbContext _appDb;
    private readonly SchoolErp.Application.Common.Interfaces.IJwtTokenGenerator _jwtGenerator;

    public AuthController(IIdentityService identityService, IApplicationDbContext db, SchoolErp.Infrastructure.Persistence.ApplicationDbContext appDb, SchoolErp.Application.Common.Interfaces.IJwtTokenGenerator jwtGenerator)
    {
        _identityService = identityService;
        _db = db;
        _appDb = appDb;
        _jwtGenerator = jwtGenerator;
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

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _identityService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword, ct);
        return result.Succeeded ? Ok(new { message = "Password updated successfully" }) : BadRequest(new { errors = result.Errors });
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

    // Dev-only: issue a token for the seeded demo admin. Not for production use.
    [HttpPost("dev-token")]
    [AllowAnonymous]
    public async Task<IActionResult> DevToken(CancellationToken ct)
    {
        var demoEmail = "kevohkevi110@gmail.com";
        var user = await _appDb.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == demoEmail, ct);
        if (user is null)
            return NotFound(new { message = "Demo admin not found" });

        var roleIds = await _appDb.Set<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>()
            .Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToListAsync(ct);

        var roles = await _appDb.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToListAsync(ct);
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        var (token, expires) = _jwtGenerator.GenerateToken(user.Id, user.UserName ?? user.Email!, user.TenantId, roles, fullName);

        return Ok(new { AccessToken = token, ExpiresAtUtc = expires, UserId = user.Id, Email = user.Email, FullName = fullName, TenantId = user.TenantId, Roles = roles });
    }
}