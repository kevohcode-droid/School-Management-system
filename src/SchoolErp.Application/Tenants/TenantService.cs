using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;
using SchoolErp.Domain.Entities;

namespace SchoolErp.Application.Tenants;

public class TenantService : ITenantService
{
    private readonly IApplicationDbContext _db;

    public TenantService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<TenantDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Tenants
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                ContactEmail = t.ContactEmail,
                IsActive = t.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<Result<TenantDto>> CreateAsync(CreateTenantRequest request, CancellationToken ct = default)
    {
        var code = request.Code.Trim().ToLowerInvariant();
        if (await _db.Tenants.AnyAsync(t => t.Code == code, ct))
            return Result<TenantDto>.Failure($"A tenant with code '{code}' already exists.");

        var tenant = new Tenant
        {
            Name = request.Name,
            Code = code,
            ContactEmail = request.ContactEmail,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync(ct);

        return Result<TenantDto>.Success(new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Code = tenant.Code,
            ContactEmail = tenant.ContactEmail,
            IsActive = tenant.IsActive
        });
    }
}
