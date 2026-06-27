using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Domain.Common;
using SchoolErp.Domain.Entities;
using SchoolErp.Infrastructure.Identity;

namespace SchoolErp.Infrastructure.Persistence;

/// <summary>
/// Applies migrations and seeds baseline data: roles and a local demo admin.
/// </summary>
public static class DbSeeder
{
    private const string DemoTenantCode = "demo";
    private const string DemoAdminEmail = "admin@demo.school";
    private const string DemoAdminPassword = "Passw0rd!";

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
            .FirstOrDefaultAsync(t => t.Code == DemoTenantCode);

        if (tenant is null)
        {
            tenant = new Tenant
            {
                Name = "Demo School",
                Code = DemoTenantCode,
                ContactEmail = DemoAdminEmail,
                IsActive = true
            };

            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }
        else if (!tenant.IsActive)
        {
            tenant.IsActive = true;
            await db.SaveChangesAsync();
        }

        var admin = await userManager.FindByEmailAsync(DemoAdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = DemoAdminEmail,
                Email = DemoAdminEmail,
                EmailConfirmed = true,
                FirstName = "Demo",
                LastName = "Admin",
                TenantId = tenant.Id
            };

            var created = await userManager.CreateAsync(admin, DemoAdminPassword);
            if (!created.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed demo admin: {string.Join("; ", created.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            admin.TenantId = tenant.Id;
            admin.EmailConfirmed = true;
            await userManager.UpdateAsync(admin);

            if (!await userManager.CheckPasswordAsync(admin, DemoAdminPassword))
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(admin);
                var reset = await userManager.ResetPasswordAsync(admin, resetToken, DemoAdminPassword);
                if (!reset.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to reset demo admin password: {string.Join("; ", reset.Errors.Select(e => e.Description))}");
                }
            }
        }

        foreach (var role in new[] { Roles.SuperAdmin, Roles.Admin })
        {
            if (!await userManager.IsInRoleAsync(admin, role))
                await userManager.AddToRoleAsync(admin, role);
        }
    }

}
