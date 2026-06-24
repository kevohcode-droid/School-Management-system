using System.ComponentModel.DataAnnotations;

namespace SchoolErp.Application.Tenants;

public record TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? ContactEmail { get; init; }
    public bool IsActive { get; init; }
}

public record CreateTenantRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Code { get; init; } = string.Empty;

    [EmailAddress]
    public string? ContactEmail { get; init; }
}
