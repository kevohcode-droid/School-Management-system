using Microsoft.AspNetCore.Identity;

namespace SchoolErp.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    /// <summary>The school this user belongs to. Drives tenant scoping.</summary>
    public Guid TenantId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
