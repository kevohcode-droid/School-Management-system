namespace SchoolErp.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(string userId, string email, string action, string description, string? ipAddress = null, string? userAgent = null, CancellationToken ct = default);
}
