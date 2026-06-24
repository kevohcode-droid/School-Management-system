using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Tenants;

/// <summary>Cross-tenant administration; intended for SuperAdmin callers only.</summary>
public interface ITenantService
{
    Task<IReadOnlyList<TenantDto>> GetAllAsync(CancellationToken ct = default);
    Task<Result<TenantDto>> CreateAsync(CreateTenantRequest request, CancellationToken ct = default);
}
