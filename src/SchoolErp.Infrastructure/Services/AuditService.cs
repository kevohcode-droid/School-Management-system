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

    public async Task LogAsync(string userId, string email, string action, string description, string? ipAddress = null, string? userAgent = null, CancellationToken ct = default)
    {
        var context = _httpContextAccessor.HttpContext;
        var auditLog = new AuditLog
        {
            UserId = userId,
            UserEmail = email,
            Action = action,
            Description = description,
            IpAddress = ipAddress ?? context?.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = userAgent ?? context?.Request.Headers["User-Agent"].ToString() ?? string.Empty
        };

        _db.AuditLogs.Add(auditLog);
        await _db.SaveChangesAsync(ct);
    }
}
