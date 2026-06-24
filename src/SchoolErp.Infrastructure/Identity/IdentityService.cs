using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Auth.Dtos;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Common;
using SchoolErp.Infrastructure.Persistence;

namespace SchoolErp.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IJwtTokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _db = db;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (!Roles.All.Contains(request.Role))
            return AuthResult.Failure($"Invalid role '{request.Role}'.");

        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Code == request.TenantCode && t.IsActive, ct);
        if (tenant is null)
            return AuthResult.Failure($"Unknown or inactive tenant '{request.TenantCode}'.");

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return AuthResult.Failure("A user with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            TenantId = tenant.Id
        };

        var created = await _userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
            return AuthResult.Failure(created.Errors.Select(e => e.Description).ToArray());

        await _userManager.AddToRoleAsync(user, request.Role);

        return AuthResult.Success(BuildResponse(user, new[] { request.Role }));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Code == request.TenantCode && t.IsActive, ct);
        if (tenant is null)
            return AuthResult.Failure("Invalid credentials.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.TenantId != tenant.Id)
            return AuthResult.Failure("Invalid credentials.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return AuthResult.Failure("Invalid credentials.");

        var roles = await _userManager.GetRolesAsync(user);
        return AuthResult.Success(BuildResponse(user, roles));
    }

    private AuthResponse BuildResponse(ApplicationUser user, IEnumerable<string> roles)
    {
        var roleList = roles.ToList();
        var (token, expires) = _tokenGenerator.GenerateToken(
            user.Id, user.UserName!, user.TenantId, roleList);

        return new AuthResponse
        {
            AccessToken = token,
            ExpiresAtUtc = expires,
            UserId = user.Id,
            Email = user.Email!,
            TenantId = user.TenantId,
            Roles = roleList
        };
    }
}
