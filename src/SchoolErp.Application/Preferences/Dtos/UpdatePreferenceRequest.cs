namespace SchoolErp.Application.Preferences.Dtos
{
    public record UpdatePreferenceRequest
    {
        public string? Theme { get; init; }
        public bool? ReceiveEmails { get; init; }
    }
}
