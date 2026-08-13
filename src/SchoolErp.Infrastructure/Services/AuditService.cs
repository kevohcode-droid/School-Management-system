using Microsoft.AspNetCore.Http;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Entities;
using SchoolErp.Infrastructure.Persistence;

namespace SchoolErp.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string userId, string email, string action, string description, string? ipAddress = null, string? userAgent = null, string? module = null, string? entity = null, string? oldValue = null, string? newValue = null, CancellationToken ct = default)
    {
        var context = _httpContextAccessor.HttpContext;
        var userAgentHeader = userAgent ?? context?.Request.Headers["User-Agent"].ToString() ?? string.Empty;

        var browser = "Unknown";
        if (userAgentHeader.Contains("Chrome") && !userAgentHeader.Contains("Edg"))
            browser = "Chrome";
        else if (userAgentHeader.Contains("Firefox"))
            browser = "Firefox";
        else if (userAgentHeader.Contains("Safari") && !userAgentHeader.Contains("Chrome"))
            browser = "Safari";
        else if (userAgentHeader.Contains("Edg"))
            browser = "Edge";

        var auditLog = new AuditLog
        {
            UserId = userId,
            UserEmail = email,
            UserName = email.Split('@')[0],
            Action = action,
            Module = module ?? "System",
            Entity = entity ?? string.Empty,
            Description = description,
            IpAddress = ipAddress ?? context?.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = userAgentHeader,
            Browser = browser,
            OldValue = oldValue,
            NewValue = newValue
        };

        _db.AuditLogs.Add(auditLog);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Swallow audit logging failures in dev to avoid blocking core flows
            // (missing columns, schema drift, etc.). Log to console for visibility.
            Console.WriteLine($"Audit log failed: {ex.Message}");
        }
    }
}
