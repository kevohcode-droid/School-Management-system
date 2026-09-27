namespace SchoolErp.Application.Common.Interfaces;

/// <summary>
/// Provisions an ASP.NET Identity user account for a school entity (Parent, Student, Staff).
/// Returns the new user ID and the plain-text temporary password to be shown to the admin once.
/// </summary>
public interface IAccountProvisioningService
{
    /// <summary>
    /// Creates an Identity account, assigns the role, and sets MustChangePassword = true.
    /// </summary>
    /// <param name="tenantId">The tenant the entity belongs to.</param>
    /// <param name="firstName">First name used for username and temp password generation.</param>
    /// <param name="lastName">Last name used for username generation.</param>
    /// <param name="email">Optional email — used as UserName if provided, otherwise username is derived from name.</param>
    /// <param name="role">The role to assign (Student, Parent, Teacher, Staff, Accountant).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The new Identity UserId and the one-time temporary password.</returns>
    Task<AccountProvisionResult> ProvisionAsync(
        Guid tenantId,
        string firstName,
        string lastName,
        string? email,
        string role,
        CancellationToken ct = default);
}

public record AccountProvisionResult(bool Succeeded, string? UserId, string? TemporaryPassword, string[] Errors);
