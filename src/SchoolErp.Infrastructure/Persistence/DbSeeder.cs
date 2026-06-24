using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Domain.Common;
using SchoolErp.Domain.Entities;
using SchoolErp.Domain.Enums;
using SchoolErp.Infrastructure.Identity;

namespace SchoolErp.Infrastructure.Persistence;

/// <summary>
/// Applies migrations and seeds baseline data: roles, a demo tenant, an admin
/// user and a small set of sample academic records.
/// </summary>
public static class DbSeeder
{
    public const string DemoTenantCode = "demo";
    public const string DemoAdminEmail = "admin@demo.school";
    public const string DemoAdminPassword = "Passw0rd!";

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

        var tenant = await db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Code == DemoTenantCode);
        if (tenant is null)
        {
            tenant = new Tenant
            {
                Name = "Demo High School",
                Code = DemoTenantCode,
                ContactEmail = DemoAdminEmail,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }

        if (await userManager.FindByEmailAsync(DemoAdminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = DemoAdminEmail,
                Email = DemoAdminEmail,
                EmailConfirmed = true,
                FirstName = "Demo",
                LastName = "Admin",
                TenantId = tenant.Id
            };
            await userManager.CreateAsync(admin, DemoAdminPassword);
            await userManager.AddToRolesAsync(admin, new[] { Roles.SuperAdmin, Roles.Admin });
        }

        await SeedAcademicDataAsync(db, tenant.Id);
    }

    private static async Task SeedAcademicDataAsync(ApplicationDbContext db, Guid tenantId)
    {
        if (await db.Sections.IgnoreQueryFilters().AnyAsync(s => s.TenantId == tenantId))
            return;

        var now = DateTime.UtcNow;

        var section = new Section
        {
            TenantId = tenantId,
            Name = "Grade 5 - A",
            GradeLevel = "Grade 5",
            Capacity = 30,
            CreatedAtUtc = now
        };

        var math = new Course
        {
            TenantId = tenantId,
            Name = "Mathematics",
            Code = "MATH101",
            Description = "Introductory mathematics.",
            Credits = 4,
            Section = section,
            CreatedAtUtc = now
        };
        var science = new Course
        {
            TenantId = tenantId,
            Name = "Science",
            Code = "SCI101",
            Description = "General science.",
            Credits = 3,
            Section = section,
            CreatedAtUtc = now
        };

        var alice = new Student
        {
            TenantId = tenantId,
            AdmissionNumber = "ADM-0001",
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice@demo.school",
            Gender = Gender.Female,
            DateOfBirth = new DateOnly(2014, 4, 12),
            EnrollmentDate = now,
            Section = section,
            CreatedAtUtc = now
        };
        var bob = new Student
        {
            TenantId = tenantId,
            AdmissionNumber = "ADM-0002",
            FirstName = "Bob",
            LastName = "Smith",
            Email = "bob@demo.school",
            Gender = Gender.Male,
            DateOfBirth = new DateOnly(2014, 9, 3),
            EnrollmentDate = now,
            Section = section,
            CreatedAtUtc = now
        };

        var grades = new[]
        {
            new Grade { TenantId = tenantId, Student = alice, Course = math, ExamType = ExamType.Midterm, Score = 88, MaxScore = 100, AssessedOnUtc = now, CreatedAtUtc = now },
            new Grade { TenantId = tenantId, Student = bob, Course = math, ExamType = ExamType.Midterm, Score = 72, MaxScore = 100, AssessedOnUtc = now, CreatedAtUtc = now },
            new Grade { TenantId = tenantId, Student = alice, Course = science, ExamType = ExamType.Quiz, Score = 19, MaxScore = 20, AssessedOnUtc = now, CreatedAtUtc = now }
        };

        var invoices = new[]
        {
            new FeeInvoice { TenantId = tenantId, Student = alice, InvoiceNumber = "INV-0001", Description = "Term 1 tuition", Amount = 1500, AmountPaid = 1500, IssuedOnUtc = now, DueDateUtc = now.AddDays(30), Status = FeeInvoiceStatus.Paid, CreatedAtUtc = now },
            new FeeInvoice { TenantId = tenantId, Student = bob, InvoiceNumber = "INV-0002", Description = "Term 1 tuition", Amount = 1500, AmountPaid = 500, IssuedOnUtc = now, DueDateUtc = now.AddDays(30), Status = FeeInvoiceStatus.PartiallyPaid, CreatedAtUtc = now }
        };

        db.Sections.Add(section);
        db.Courses.AddRange(math, science);
        db.Students.AddRange(alice, bob);
        db.Grades.AddRange(grades);
        db.FeeInvoices.AddRange(invoices);

        await db.SaveChangesAsync();
    }
}
