using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Settings;
using SchoolErp.Domain.Common;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService) => _settingsService = settingsService;

    [HttpGet("{category}")]
    public async Task<IActionResult> GetCategory(string category, CancellationToken ct)
    {
        var settings = await _settingsService.GetCategoryAsync(category, ct);
        return Ok(settings);
    }

    [HttpGet("key/{category}/{key}")]
    public async Task<IActionResult> GetSetting(string category, string key, CancellationToken ct)
    {
        var value = await _settingsService.GetSettingAsync(category, key, ct);
        return Ok(new { key, value });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request, CancellationToken ct)
    {
        foreach (var item in request.Settings)
        {
            await _settingsService.SaveSettingAsync(request.Category, item.Key, item.Value, ct);
        }
        return Ok(new { message = "Settings updated successfully" });
    }
}
