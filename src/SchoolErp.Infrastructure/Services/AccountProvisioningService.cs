using Microsoft.AspNetCore.Identity;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Infrastructure.Identity;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SchoolErp.Infrastructure.Services;

/// <summary>
/// Creates ASP.NET Identity accounts for school entities with a temporary password
/// using a cryptographically random temporary password.
/// Only the PasswordHash is stored; the plain-text password is returned once for the admin.
/// </summary>
public class AccountProvisioningService : IAccountProvisioningService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountProvisioningService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AccountProvisionResult> ProvisionAsync(
        Guid tenantId,
        string firstName,
        string lastName,
        string? email,
        string role,
        CancellationToken ct = default)
    {
        // Generate username: firstname.lastname (lowercase, ASCII-safe, dots instead of spaces)
        var baseUsername = GenerateUsername(firstName, lastName);

        // Ensure uniqueness by appending a suffix if the username is taken
        var username = await EnsureUniqueUsernameAsync(baseUsername);

        if (role is not ("Student" or "Parent" or "Teacher" or "Staff" or "Accountant"))
            return new AccountProvisionResult(false, null, null, new[] { "This role cannot be provisioned through this workflow." });

        var tempPassword = $"S{Convert.ToHexString(RandomNumberGenerator.GetBytes(8))}a7!";

        // Use email as login identifier if provided, otherwise use generated username
        var loginEmail = !string.IsNullOrWhiteSpace(email) ? email.Trim() : $"{username}@school.local";

        // Never attach a staff record to an account from another tenant or silently reuse another role.
        var existing = await _userManager.FindByEmailAsync(loginEmail);
        if (existing is not null)
        {
            if (existing.TenantId != tenantId || !await _userManager.IsInRoleAsync(existing, role))
                return new AccountProvisionResult(false, null, null, new[] { "An account with this email already exists." });

            return new AccountProvisionResult(true, existing.Id, null, Array.Empty<string>());
        }

        var user = new ApplicationUser
        {
            UserName = username,
            Email = loginEmail,
            FirstName = firstName,
            LastName = lastName,
            TenantId = tenantId,
            EmailConfirmed = true,   // Admin-created accounts are pre-confirmed
            MustChangePassword = true
        };

        var result = await _userManager.CreateAsync(user, tempPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return new AccountProvisionResult(false, null, null, errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return new AccountProvisionResult(false, null, null, roleResult.Errors.Select(error => error.Description).ToArray());
        }

        return new AccountProvisionResult(true, user.Id, tempPassword, Array.Empty<string>());
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string GenerateUsername(string firstName, string lastName)
    {
        var sanitize = (string s) => Regex.Replace(s.ToLowerInvariant().Trim(), @"[^a-z0-9]", "");
        var fn = sanitize(firstName);
        var ln = sanitize(lastName);
        return string.IsNullOrEmpty(ln) ? fn : $"{fn}.{ln}";
    }

    private async Task<string> EnsureUniqueUsernameAsync(string baseUsername)
    {
        var candidate = baseUsername;
        var suffix = 1;
        while (await _userManager.FindByNameAsync(candidate) is not null)
        {
            candidate = $"{baseUsername}{suffix++}";
        }
        return candidate;
    }

}
