using System.Linq;

namespace SchoolErp.Domain.Common;

public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";
    public const string Parent = "Parent";

    public static readonly IReadOnlyList<string> TenantRoles = new[]
    {
        Admin, Teacher, Student, Parent
    };

    public static readonly IReadOnlyList<string> SystemRoles = new[]
    {
        SuperAdmin
    };

    public static readonly IReadOnlyList<string> All = TenantRoles.Concat(SystemRoles).ToArray();
}
