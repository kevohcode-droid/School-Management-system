namespace SchoolErp.Application.AuditLogs.Dtos;

public record AuditLogDto
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string UserRole { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Module { get; init; } = string.Empty;
    public string Entity { get; init; } = string.Empty;
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public string Description { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
    public string Browser { get; init; } = string.Empty;
    public string Status { get; init; } = "Success";
    public DateTime CreatedAtUtc { get; init; }

    public string Date => CreatedAtUtc.ToString("yyyy-MM-dd");
    public string Time => CreatedAtUtc.ToString("HH:mm:ss");
    public string FormattedCreatedAt => CreatedAtUtc.ToString("yyyy-MM-dd HH:mm:ss");
}

public record AuditLogFilter
{
    public string? SearchQuery { get; init; }
    public string? Action { get; init; }
    public string? Module { get; init; }
    public string? UserId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}