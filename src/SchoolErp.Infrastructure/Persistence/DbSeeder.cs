using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Domain.Common;
using SchoolErp.Domain.Entities;
using SchoolErp.Infrastructure.Identity;

namespace SchoolErp.Infrastructure.Persistence;

/// <summary>
/// Applies migrations and seeds baseline data: roles and a local administrator.
/// </summary>
public static class DbSeeder
{
    private const string SeedTenantCode = "100";
    private const string SeedSchoolName = "Maina Group of Schools";
    private const string SeedAdminEmail = "kevohkevi110@gmail.com";
    private const string SeedAdminPassword = "Kevoh2060,!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<ApplicationDbContext>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.MigrateAsync();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var tenant = await db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Code == SeedTenantCode);

        if (tenant is null)
        {
            tenant = new Tenant
            {
                Name = SeedSchoolName,
                Code = SeedTenantCode,
                ContactEmail = SeedAdminEmail,
                IsActive = true
            };

            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }
        else if (!tenant.IsActive || tenant.Name == "Demo School")
        {
            tenant.IsActive = true;
            tenant.Name = SeedSchoolName;
            await db.SaveChangesAsync();
        }

        var admin = await userManager.FindByEmailAsync(SeedAdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = SeedAdminEmail,
                Email = SeedAdminEmail,
                EmailConfirmed = true,
                FirstName = "School",
                LastName = "Administrator",
                TenantId = tenant.Id
            };

            var created = await userManager.CreateAsync(admin, SeedAdminPassword);
            if (!created.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed local administrator: {string.Join("; ", created.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            admin.TenantId = tenant.Id;
            admin.EmailConfirmed = true;
            if (admin.FirstName == "Demo")
            {
                admin.FirstName = "School";
                admin.LastName = "Administrator";
            }
            await userManager.UpdateAsync(admin);

            if (!await userManager.CheckPasswordAsync(admin, SeedAdminPassword))
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(admin);
                var reset = await userManager.ResetPasswordAsync(admin, resetToken, SeedAdminPassword);
                if (!reset.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to reset local administrator password: {string.Join("; ", reset.Errors.Select(e => e.Description))}");
                }
            }
        }

        foreach (var role in new[] { Roles.Admin })
        {
            if (!await userManager.IsInRoleAsync(admin, role))
                await userManager.AddToRoleAsync(admin, role);
        }

        // Seed test users for role-based access checks.
        var dummyUsers = new List<(string Email, string FirstName, string LastName, string Role)>
        {
            ("superadmin@demo.com", "Super", "Admin", Roles.SuperAdmin),
            ("teacher@demo.com", "Alex", "Teacher", Roles.Teacher),
            ("student@demo.com", "Alex", "Student", Roles.Student),
            ("parent@demo.com", "Alex", "Parent", Roles.Parent),
            ("staff@demo.com", "Alex", "Staff", Roles.Staff),
            ("accountant@demo.com", "Alex", "Accountant", Roles.Accountant)
        };

        foreach (var u in dummyUsers)
        {
            var user = await userManager.FindByEmailAsync(u.Email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = u.Email,
                    Email = u.Email,
                    EmailConfirmed = true,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    TenantId = tenant.Id
                };
                await userManager.CreateAsync(user, "Test1234!");
            }
            else if (user.FirstName == "Demo")
            {
                user.FirstName = u.FirstName;
                user.LastName = u.LastName;
                await userManager.UpdateAsync(user);
            }
            if (!await userManager.IsInRoleAsync(user, u.Role))
            {
                await userManager.AddToRoleAsync(user, u.Role);
            }
        }
    }

}
