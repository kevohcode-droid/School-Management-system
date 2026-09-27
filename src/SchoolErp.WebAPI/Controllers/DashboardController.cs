using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Application.Dashboard;
using SchoolErp.Application.Dashboard.Dtos;
using SchoolErp.Domain.Common;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SuperAdmin},{Roles.Accountant},{Roles.Staff},{Roles.Teacher},{Roles.Student},{Roles.Parent}")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken ct)
    {
        var summary = await _dashboardService.GetSummaryAsync(ct);
        if (!User.IsInRole(Roles.Admin) && !User.IsInRole(Roles.SuperAdmin) && !User.IsInRole(Roles.Accountant))
        {
            summary = summary with
            {
                PendingFees = 0,
                TodayCollections = 0,
                MonthlyCollections = 0
            };
        }
        return Ok(summary);
    }

    [HttpGet("public-summary")]
    [AllowAnonymous]
    public async Task<ActionResult<DashboardPublicDto>> GetPublicSummary(CancellationToken ct)
    {
        var summary = await _dashboardService.GetPublicSummaryAsync(ct);
        return Ok(summary);
    }
}