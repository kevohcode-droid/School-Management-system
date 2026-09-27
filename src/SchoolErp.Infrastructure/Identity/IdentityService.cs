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
    private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly string _frontendUrl;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IJwtTokenGenerator tokenGenerator,
        IEmailService emailService,
        IAuditService auditService,
        Microsoft.AspNetCore.Identity.IPasswordHasher<ApplicationUser> passwordHasher,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _userManager = userManager;
        _db = db;
        _tokenGenerator = tokenGenerator;
        _emailService = emailService;
        _auditService = auditService;
        _passwordHasher = passwordHasher;
        // Read frontend URL from configuration; fallback to localhost:4200
        _frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:4200";
    }

    // Read-only helper that projects user fields we need without selecting
    // any potentially-missing columns that would cause SQL errors on older schemas.
    private async Task<ApplicationUser?> FindUserByEmailAsync(string email, CancellationToken ct = default)
    {
        var u = await _db.Users.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => new
            {
                x.Id,
                x.UserName,
                x.Email,
                x.EmailConfirmed,
                x.PasswordHash,
                x.TenantId,
                x.SecurityStamp,
                x.ConcurrencyStamp,
                x.FirstName,
                x.LastName,
                x.LastLoginAt,
                x.MustChangePassword
            })
            .SingleOrDefaultAsync(ct);

        if (u is null) return null;

        return new ApplicationUser
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            EmailConfirmed = u.EmailConfirmed,
            PasswordHash = u.PasswordHash,
            TenantId = u.TenantId,
            SecurityStamp = u.SecurityStamp,
            ConcurrencyStamp = u.ConcurrencyStamp,
            FirstName = u.FirstName,
            LastName = u.LastName,
            LastLoginAt = u.LastLoginAt,
            MustChangePassword = u.MustChangePassword
        };
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (request.Role is not (Roles.Student or Roles.Parent))
            return AuthResult.Failure("Self-registration is available only for Student and Parent accounts.");

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

        var existing = await _db.Users.AsNoTracking().AnyAsync(u => u.Email == request.Email, ct);
        if (existing)
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
        // Step 1: Validate tenant
        var tenant = await _db.Tenants
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Code == request.TenantCode && t.IsActive, ct);
        if (tenant is null)
            return AuthResult.Failure("Invalid credentials.");

        // Step 2: Lean user projection - only needed columns, no full entity load
        var userRow = await _db.Users
            .AsNoTracking()
            .Where(u => u.NormalizedEmail == request.Email.ToUpperInvariant() && u.TenantId == tenant.Id)
            .Select(u => new
            {
                u.Id, u.UserName, u.Email, u.EmailConfirmed,
                u.PasswordHash, u.TenantId, u.SecurityStamp,
                u.ConcurrencyStamp, u.FirstName, u.LastName,
                u.LastLoginAt, u.MustChangePassword,
                u.LockoutEnabled, u.LockoutEnd, u.AccessFailedCount
            })
            .FirstOrDefaultAsync(ct);

        if (userRow is null)
            return AuthResult.Failure("Invalid credentials.");

        // Step 3: Re-hydrate minimal ApplicationUser for password verification
        var user = new ApplicationUser
        {
            Id = userRow.Id, UserName = userRow.UserName, Email = userRow.Email,
            EmailConfirmed = userRow.EmailConfirmed, PasswordHash = userRow.PasswordHash,
            TenantId = userRow.TenantId, SecurityStamp = userRow.SecurityStamp,
            ConcurrencyStamp = userRow.ConcurrencyStamp, FirstName = userRow.FirstName,
            LastName = userRow.LastName, LastLoginAt = userRow.LastLoginAt,
            MustChangePassword = userRow.MustChangePassword,
            LockoutEnabled = userRow.LockoutEnabled, LockoutEnd = userRow.LockoutEnd,
            AccessFailedCount = userRow.AccessFailedCount
        };

        // Step 4: Verify password directly (no extra DB round-trip)
        var pwResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, request.Password);
        if (pwResult == PasswordVerificationResult.Failed)
            return AuthResult.Failure("Invalid credentials.");

        // Step 5: Update LastLoginAt synchronously (safe - same DbContext scope)
        await _db.Users
            .Where(u => u.Id == user.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.LastLoginAt, DateTime.UtcNow), ct);

        var roles = await _userManager.GetRolesAsync(user);
        await _auditService.LogAsync(user.Id, user.Email!, "LOGIN", $"{user.Email} logged in", ct: ct);
        return AuthResult.Success(BuildResponse(user, roles));
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        var user = await FindUserByEmailAsync(email, ct);
        if (user is null)
            return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(token);
        var resetLink = $"{_frontendUrl.TrimEnd('/')}/reset-password?token={encodedToken}&email={WebUtility.UrlEncode(user.Email)}";

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
        var user = await FindUserByEmailAsync(request.Email, ct);
        if (user is null)
            return AuthResult.Failure("Invalid password reset request.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return AuthResult.Failure(result.Errors.Select(e => e.Description).ToArray());

        await _auditService.LogAsync(user.Id, user.Email!, "RESET_PASSWORD", $"{user.Email} reset their password", ct: ct);
        return AuthResult.Success(BuildResponse(user, await _userManager.GetRolesAsync(user)));
    }

    public async Task<AuthResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return AuthResult.Failure("User not found.");

        if (!await _userManager.CheckPasswordAsync(user, currentPassword))
            return AuthResult.Failure("Current password is incorrect.");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
            return AuthResult.Failure(result.Errors.Select(e => e.Description).ToArray());

        // Clear the forced-change flag now that the user has chosen their own password
        if (user.MustChangePassword)
        {
            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);
        }

        await _auditService.LogAsync(user.Id, user.Email!, "CHANGE_PASSWORD", $"{user.Email} changed their password", ct: ct);
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
        var existing = await FindUserByEmailAsync(email, ct);

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
        var linkedStudentIds = roleList.Contains(Roles.Parent)
            ? _db.ParentStudents
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(ps => ps.Parent != null && ps.Parent.UserId == user.Id && ps.TenantId == user.TenantId)
                .Select(ps => ps.StudentId)
                .ToList()
            : new List<Guid>();

        var (token, expires) = _tokenGenerator.GenerateToken(
            user.Id, user.UserName!, user.TenantId, roleList, fullName, linkedStudentIds);

        return new AuthResponse
        {
            AccessToken = token,
            ExpiresAtUtc = expires,
            UserId = user.Id,
            Email = user.Email!,
            FullName = fullName,
            TenantId = user.TenantId,
            Roles = roleList,
            LinkedStudentIds = linkedStudentIds,
            MustChangePassword = user.MustChangePassword
        };
    }
}
