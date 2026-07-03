namespace SchoolErp.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    /// <summary>Issues a signed JWT embedding the user id, tenant, user name, tenant, roles, and full name.</summary>
    (string Token, DateTime ExpiresAtUtc) GenerateToken(
        string userId,
        string userName,
        Guid tenantId,
        IEnumerable<string> roles,
        string fullName);
}
