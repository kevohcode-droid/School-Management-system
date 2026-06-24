using System.Security.Claims;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Infrastructure.Identity;

namespace SchoolErp.WebAPI.Services;

/// <summary>Resolves the caller from the JWT claims on the current request.</summary>
public class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal? _principal;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _principal = accessor.HttpContext?.User;
    }

    public bool IsAuthenticated => _principal?.Identity?.IsAuthenticated ?? false;

    public string? UserId => _principal?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _principal?.FindFirstValue("sub");

    public string? UserName => _principal?.FindFirstValue(ClaimTypes.Name)
        ?? _principal?.FindFirstValue("unique_name");

    public Guid? TenantId =>
        Guid.TryParse(_principal?.FindFirstValue(JwtTokenGenerator.TenantClaimType), out var id)
            ? id
            : null;

    public IReadOnlyCollection<string> Roles =>
        _principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? Array.Empty<string>();
}
