using SchoolErp.Application.Dashboard.Dtos;

namespace SchoolErp.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);
    Task<DashboardPublicDto> GetPublicSummaryAsync(CancellationToken ct = default);
}