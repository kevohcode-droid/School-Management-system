using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Preferences;
using SchoolErp.Application.Preferences.Dtos;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PreferencesController : ControllerBase
{
    private readonly IPreferenceService _preferenceService;
    private readonly ICurrentUser _currentUser;

    public PreferencesController(IPreferenceService preferenceService, ICurrentUser currentUser)
    {
        _preferenceService = preferenceService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var preferences = await _preferenceService.GetPreferencesAsync(userId, ct);
        return Ok(preferences ?? new PreferenceDto());
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] UpdatePreferenceRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _preferenceService.UpdatePreferencesAsync(userId, request, ct);
        if (!success)
            return BadRequest(new { message = "Failed to update preferences" });

        return Ok(new { message = "Preferences updated successfully" });
    }
}