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

    /// <summary>
    /// Set to true when an admin creates the account with a temporary password.
    /// The user is forced to change their password before accessing the dashboard.
    /// </summary>
    public bool MustChangePassword { get; set; }
}
