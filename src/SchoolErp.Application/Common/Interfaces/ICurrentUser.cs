namespace SchoolErp.Application.Common.Interfaces;

/// <summary>
/// Ambient information about the caller, resolved per-request from the JWT.
/// The <see cref="TenantId"/> drives the EF Core global query filter.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? UserName { get; }
    Guid? TenantId { get; }
    IReadOnlyCollection<string> Roles { get; }
}
