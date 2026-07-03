using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Entities;

namespace SchoolErp.Application.Settings;

public class SettingsService : ISettingsService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMemoryCache _cache;

    public SettingsService(IApplicationDbContext db, ICurrentUser currentUser, IMemoryCache cache)
    {
        _db = db;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<string?> GetSettingAsync(string category, string key, CancellationToken ct = default)
    {
        if (_currentUser.TenantId is not { } tenantId)
            return null;

        var cacheKey = $"Tenant_{tenantId}_{category}_{key}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

            var setting = await _db.TenantSettings
                .FirstOrDefaultAsync(s => s.TenantId == tenantId
                                       && s.Category == category
                                       && s.Key == key, ct);

            return setting?.Value;
        });
    }

    public async Task SaveSettingAsync(string category, string key, string value, CancellationToken ct = default)
    {
        if (_currentUser.TenantId is not { } tenantId)
            throw new InvalidOperationException("Tenant not resolved.");

        var setting = await _db.TenantSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId
                                   && s.Category == category
                                   && s.Key == key, ct);

        if (setting == null)
        {
            setting = new TenantSetting
            {
                TenantId = tenantId,
                Category = category,
                Key = key,
                Value = value
            };
            _db.TenantSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        var cacheKey = $"Tenant_{tenantId}_{category}_{key}";
        _cache.Remove(cacheKey);
    }

    public async Task<Dictionary<string, string>> GetCategoryAsync(string category, CancellationToken ct = default)
    {
        if (_currentUser.TenantId is not { } tenantId)
            return new Dictionary<string, string>();

        var cacheKey = $"Tenant_{tenantId}_Category_{category}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

            var settings = await _db.TenantSettings
                .Where(s => s.TenantId == tenantId && s.Category == category)
                .ToListAsync(ct);

            return settings.ToDictionary(s => s.Key, s => s.Value);
        });
    }
}
