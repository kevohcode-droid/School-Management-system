using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolErp.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public Guid TenantId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    // Use the base IdentityUser.PhoneNumber property; do not redeclare it.
    [NotMapped]
    public string? Address { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
