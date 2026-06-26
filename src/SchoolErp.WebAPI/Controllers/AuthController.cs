using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Application.Auth.Dtos;
using SchoolErp.Application.Common.Interfaces;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService) => _identityService = identityService;

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

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me([FromServices] ICurrentUser currentUser) => Ok(new
    {
        currentUser.UserId,
        currentUser.UserName,
        currentUser.TenantId,
        currentUser.Roles
    });
}
