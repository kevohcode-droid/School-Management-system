using Google.Apis.Auth;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Auth.Dtos;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Common;
using SchoolErp.Infrastructure.Persistence;
using System.Net;

namespace SchoolErp.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IJwtTokenGenerator tokenGenerator,
        IEmailService emailService,
        IAuditService auditService)
    {
        _userManager = userManager;
        _db = db;
        _tokenGenerator = tokenGenerator;
        _emailService = emailService;
        _auditService = auditService;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (!Roles.TenantRoles.Contains(request.Role))
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
        await _auditService.LogAsync(user.Id, user.Email!, "REGISTER", $"{user.Email} registered an account", ct: ct);

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
        await _auditService.LogAsync(user.Id, user.Email!, "LOGIN", $"{user.Email} logged in", ct: ct);
        return AuthResult.Success(BuildResponse(user, roles));
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(token);
        var resetLink = $"http://localhost:4200/reset-password?token={encodedToken}&email={user.Email}";

        var body = $@"
            <h2>Reset your password</h2>
            <p>We received a request to reset your password for School ERP.</p>
            <p><a href='{resetLink}'>Click here to reset your password</a></p>
            <p>If you did not request this, you can ignore this email.</p>";

        await _emailService.SendEmailAsync(user.Email!, "Reset Your Password", body, ct);
        await _auditService.LogAsync(user.Id, user.Email!, "FORGOT_PASSWORD", $"{user.Email} requested a password reset", ct: ct);
    }

    public async Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return AuthResult.Failure("Invalid password reset request.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return AuthResult.Failure(result.Errors.Select(e => e.Description).ToArray());

        await _auditService.LogAsync(user.Id, user.Email!, "RESET_PASSWORD", $"{user.Email} reset their password", ct: ct);
        return AuthResult.Success(BuildResponse(user, await _userManager.GetRolesAsync(user)));
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
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        var (token, expires) = _tokenGenerator.GenerateToken(
            user.Id, user.UserName!, user.TenantId, roleList, fullName);

        return new AuthResponse
        {
            AccessToken = token,
            ExpiresAtUtc = expires,
            UserId = user.Id,
            Email = user.Email!,
            FullName = fullName,
            TenantId = user.TenantId,
            Roles = roleList
        };
    }
}
