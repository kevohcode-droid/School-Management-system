namespace SchoolErp.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    /// <summary>Issues a signed JWT embedding the user id, tenant and roles.</summary>
    (string Token, DateTime ExpiresAtUtc) GenerateToken(
        string userId,
        string userName,
        Guid tenantId,
        IEnumerable<string> roles);
}
