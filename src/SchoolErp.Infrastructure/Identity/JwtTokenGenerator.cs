using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Infrastructure.Configuration;

namespace SchoolErp.Infrastructure.Identity;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    public const string TenantClaimType = "tenant_id";
    public const string LinkedStudentIdsClaimType = "linkedStudentIds";

    private readonly JwtSettings _settings;

    public JwtTokenGenerator(IOptions<JwtSettings> settings) => _settings = settings.Value;

    public (string Token, DateTime ExpiresAtUtc) GenerateToken(
        string userId,
        string userName,
        Guid tenantId,
        IEnumerable<string> roles,
        string fullName,
        IEnumerable<Guid>? linkedStudentIds = null)
    {
        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(ClaimTypes.NameIdentifier, userId),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new(ClaimTypes.Name, userName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(TenantClaimType, tenantId.ToString()),
            new("tenantId", tenantId.ToString()),
            new("userId", userId),
            new("full_name", fullName)
        };
        foreach (var role in roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role));
        }

        var studentIds = linkedStudentIds?.Distinct().Select(id => id.ToString()).ToArray() ?? Array.Empty<string>();
        if (studentIds.Length > 0)
        {
            claims.Add(new Claim(LinkedStudentIdsClaimType, string.Join(",", studentIds)));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
