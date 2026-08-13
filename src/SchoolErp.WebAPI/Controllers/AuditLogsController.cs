using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Application.AuditLogs;
using SchoolErp.Application.AuditLogs.Dtos;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Get([FromQuery] AuditLogFilter filter, CancellationToken ct)
    {
        var result = await _auditLogService.GetAuditLogsAsync(filter, ct);
        return Ok(result);
    }
}