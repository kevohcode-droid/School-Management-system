using Google.Apis.Auth;
using System.Text;
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
            .FirstOrDefaultAsync(t => t.Code == request.TenantCode, ct);
        
        if (tenant is null)
        {
            // Auto-create tenant if it doesn't exist
            tenant = new Domain.Entities.Tenant
            {
                Name = $"{request.TenantCode} School",
                Code = request.TenantCode,
                ContactEmail = request.Email,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.Tenants.Add(tenant);
            await _db.SaveChangesAsync(ct);
        }
        else if (!tenant.IsActive)
        {
            return AuthResult.Failure($"Tenant '{request.TenantCode}' is inactive.");
        }

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

    public async Task ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Microsoft.AspNetCore.WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetLink = $"http://localhost:4200/reset-password?token={encodedToken}&email={user.Email}";

        await Task.CompletedTask;

        // TODO: await _emailService.SendEmailAsync(user.Email, "Reset Your Password", $"Click here: {resetLink}");
    }

    public async Task<AuthResult> GoogleSignupAsync(GoogleSignupRequest request, CancellationToken ct = default)
    {
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Code == request.TenantCode && t.IsActive, ct);

        if (tenant is null)
        {
            return AuthResult.Failure($"Tenant '{request.TenantCode}' not found or inactive.");
        }

        var payload = await VerifyGoogleTokenAsync(request.Token);
        if (payload is null)
        {
            return AuthResult.Failure("Invalid Google token.");
        }

        var email = payload.Email;
        var existing = await _userManager.FindByEmailAsync(email);

        if (existing is not null)
        {
            var roles = await _userManager.GetRolesAsync(existing);
            return AuthResult.Success(BuildResponse(existing, roles));
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = payload.GivenName ?? "Google",
            LastName = payload.FamilyName ?? "User",
            TenantId = tenant.Id,
            EmailConfirmed = true
        };

        var created = await _userManager.CreateAsync(user);
        if (!created.Succeeded)
        {
            return AuthResult.Failure(created.Errors.Select(e => e.Description).ToArray());
        }

        await _userManager.AddToRoleAsync(user, Roles.Student);

        return AuthResult.Success(BuildResponse(user, new[] { Roles.Student }));
    }

    private static async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string token)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { "673268021018-jtkrs64bsbi5mnhjhce1gelph85kg34p.apps.googleusercontent.com" }
            };
            return await GoogleJsonWebSignature.ValidateAsync(token, settings);
        }
        catch
        {
            return null;
        }
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
