using SchoolErp.Domain.Common;

namespace SchoolErp.Domain.Entities;

public class UserPreferences : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public string Appearance { get; set; } = "light";
    public string Language { get; set; } = "en";
    public string Timezone { get; set; } = "Africa/Nairobi";

    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public bool PushNotifications { get; set; } = true;

    public bool CompactMode { get; set; } = false;
    public bool ShowWelcomeBanner { get; set; } = true;
    public string DefaultLandingPage { get; set; } = "/dashboard";

    public int AutoLogoutMinutes { get; set; } = 15;
}