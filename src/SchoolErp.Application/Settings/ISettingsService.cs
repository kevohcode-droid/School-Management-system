namespace SchoolErp.Application.Settings;

public interface ISettingsService
{
    Task<string?> GetSettingAsync(string category, string key, CancellationToken ct = default);
    Task SaveSettingAsync(string category, string key, string value, CancellationToken ct = default);
    Task<Dictionary<string, string>> GetCategoryAsync(string category, CancellationToken ct = default);
}
