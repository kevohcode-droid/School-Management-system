using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Application.Dashboard;
using SchoolErp.Application.Dashboard.Dtos;

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
    [Authorize]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken ct)
    {
        var summary = await _dashboardService.GetSummaryAsync(ct);
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