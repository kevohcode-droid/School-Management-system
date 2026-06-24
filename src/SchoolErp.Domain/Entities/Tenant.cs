using SchoolErp.Domain.Common;

namespace SchoolErp.Domain.Entities;

/// <summary>
/// A school. Every tenant-scoped entity references one of these via TenantId.
/// </summary>
public class Tenant : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Short, unique identifier for the school (e.g. used as a subdomain).</summary>
    public string Code { get; set; } = string.Empty;

    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; } = true;
}
