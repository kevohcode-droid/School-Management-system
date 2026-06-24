namespace SchoolErp.Domain.Common;

/// <summary>
/// Marks an entity as belonging to a single tenant (school). A global query
/// filter is applied to every <see cref="ITenantEntity"/> so rows are scoped
/// to the current tenant and cross-school data leaks are prevented.
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
