namespace SchoolErp.Application.Profile.Dtos
{
    public record UpdateProfileRequest
    {
        public string FullName { get; init; } = string.Empty;
        public string? Phone { get; init; }
        public string? Address { get; init; }
    }
}
