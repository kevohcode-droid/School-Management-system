using SchoolErp.Application.AuditLogs.Dtos;

namespace SchoolErp.Application.AuditLogs;

public interface IAuditLogService
{
    Task<PaginatedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogFilter filter, CancellationToken ct = default);
}

public record PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}