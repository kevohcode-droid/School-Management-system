using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.AuditLogs.Dtos;
using SchoolErp.Application.Common.Interfaces;

namespace SchoolErp.Application.AuditLogs;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AuditLogService(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogFilter filter, CancellationToken ct = default)
    {
        var isAdmin = _currentUser.Roles.Contains("Admin") || _currentUser.Roles.Contains("SuperAdmin");
        if (!isAdmin)
        {
            return new PaginatedResult<AuditLogDto>
            {
                Items = Array.Empty<AuditLogDto>(),
                TotalCount = 0
            };
        }

        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
        {
            var searchLower = filter.SearchQuery.ToLower();
            query = query.Where(a =>
                a.UserEmail.ToLower().Contains(searchLower) ||
                a.UserName.ToLower().Contains(searchLower) ||
                a.Action.ToLower().Contains(searchLower) ||
                a.Module.ToLower().Contains(searchLower) ||
                a.Description.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            query = query.Where(a => a.Action == filter.Action);
        }

        if (!string.IsNullOrWhiteSpace(filter.Module))
        {
            query = query.Where(a => a.Module == filter.Module);
        }

        if (!string.IsNullOrWhiteSpace(filter.UserId))
        {
            query = query.Where(a => a.UserId == filter.UserId);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(a => a.CreatedAtUtc >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(a => a.CreatedAtUtc <= filter.EndDate.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var logs = await query
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserEmail = a.UserEmail,
                UserName = a.UserName,
                UserRole = a.UserRole,
                Action = a.Action,
                Module = a.Module,
                Entity = a.Entity,
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                Description = a.Description,
                IpAddress = a.IpAddress,
                Browser = a.Browser,
                Status = a.Status,
                CreatedAtUtc = a.CreatedAtUtc
            })
            .ToListAsync(ct);

        return new PaginatedResult<AuditLogDto>
        {
            Items = logs,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }
}