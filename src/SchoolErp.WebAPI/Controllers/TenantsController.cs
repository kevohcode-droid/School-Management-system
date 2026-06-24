using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Application.Tenants;
using SchoolErp.Domain.Common;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.SuperAdmin)]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenants;

    public TenantsController(ITenantService tenants) => _tenants = tenants;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _tenants.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateTenantRequest request, CancellationToken ct)
    {
        var result = await _tenants.CreateAsync(request, ct);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }
}
