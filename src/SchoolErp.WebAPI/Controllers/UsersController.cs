using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Infrastructure.Identity;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _db;

    public UsersController(UserManager<ApplicationUser> userManager, IApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe([FromServices] ICurrentUser currentUser, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound();

        var tenant = currentUser.TenantId is { } tenantId
            ? await _db.Tenants
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == tenantId, ct)
            : null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        var fullName = (!string.IsNullOrEmpty(user.FirstName) || !string.IsNullOrEmpty(user.LastName))
            ? $"{user.FirstName} {user.LastName}".Trim()
            : currentUser.FullName ?? user.UserName ?? "Unknown";

        return Ok(new
        {
            FullName = fullName,
            Email = user.Email ?? "",
            Phone = user.PhoneNumber ?? "",
            Role = role,
            School = tenant?.Name ?? "Unknown",
            TenantCode = tenant?.Code,
            Username = user.UserName ?? "",
            LastLogin = user.LastLoginAt?.ToString("o"),
            DateJoined = user.LockoutEnd?.UtcDateTime,
            Address = user.Address
        });
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request, [FromServices] ICurrentUser currentUser, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var nameParts = request.FullName.Split(' ', 2);
        user.FirstName = nameParts[0];
        user.LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
        user.PhoneNumber = request.Phone;
        user.Address = request.Address;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new { message = "Profile updated successfully" });
    }
}

public record UpdateProfileRequest
{
    public string FullName { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Address { get; init; }
}