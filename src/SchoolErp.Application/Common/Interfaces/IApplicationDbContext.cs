using Microsoft.EntityFrameworkCore;
using SchoolErp.Domain.Entities;

namespace SchoolErp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Student> Students { get; }
    DbSet<Section> Sections { get; }
    DbSet<StaffMember> StaffMembers { get; }
    DbSet<Parent> Parents { get; }
    DbSet<ParentStudent> ParentStudents { get; }
    DbSet<Announcement> Announcements { get; }
    DbSet<Course> Courses { get; }
    DbSet<Grade> Grades { get; }
    DbSet<StudentMark> StudentMarks { get; }
    DbSet<FeeInvoice> FeeInvoices { get; }
    DbSet<FeeCategory> FeeCategories { get; }
    DbSet<FeeTemplate> FeeTemplates { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    DbSet<Discount> Discounts { get; }
    DbSet<TenantSetting> TenantSettings { get; }
    DbSet<AttendanceRecord> AttendanceRecords { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<UserPreferences> UserPreferences { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
