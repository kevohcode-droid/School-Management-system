namespace SchoolErp.Application.Settings;

public record UpdateSettingsRequest(string Category, Dictionary<string, string> Settings);
