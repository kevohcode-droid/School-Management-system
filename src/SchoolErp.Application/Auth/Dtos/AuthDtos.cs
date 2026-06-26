using System.ComponentModel.DataAnnotations;

namespace SchoolErp.Application.Auth.Dtos;

public record ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
}

public record RegisterRequest
{
    /// <summary>Code of the tenant (school) the new user belongs to.</summary>
    [Required]
    public string TenantCode { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; init; } = string.Empty;

    [Required]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    public string LastName { get; init; } = string.Empty;

    /// <summary>One of the values in <see cref="SchoolErp.Domain.Common.Roles"/>.</summary>
    [Required]
    public string Role { get; init; } = string.Empty;
}

public record LoginRequest
{
    [Required]
    public string TenantCode { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public record AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Guid TenantId { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}

public record AuthResult
{
    public bool Succeeded { get; init; }
    public AuthResponse? Response { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static AuthResult Success(AuthResponse response) => new() { Succeeded = true, Response = response };
    public static AuthResult Failure(params string[] errors) => new() { Succeeded = false, Errors = errors };
}
