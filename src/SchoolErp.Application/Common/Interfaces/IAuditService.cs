namespace SchoolErp.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(string userId, string email, string action, string description, string? ipAddress = null, string? userAgent = null, string? module = null, string? entity = null, string? oldValue = null, string? newValue = null, CancellationToken ct = default);
}
