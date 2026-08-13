namespace SchoolErp.Application.Preferences.Dtos
{
    public record PreferenceDto
    {
        public string Theme { get; init; } = "default";
        public bool ReceiveEmails { get; init; } = false;
    }
}
