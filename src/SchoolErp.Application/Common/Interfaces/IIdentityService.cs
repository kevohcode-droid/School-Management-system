using SchoolErp.Application.Auth.Dtos;

namespace SchoolErp.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResult> GoogleSignupAsync(GoogleSignupRequest request, CancellationToken ct = default);
    Task ForgotPasswordAsync(string email, CancellationToken ct = default);
}
