using Microsoft.EntityFrameworkCore;
using SchoolErp.Domain.Entities;

namespace SchoolErp.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the persistence context so the Application layer can run
/// queries without depending on the concrete EF Core / Infrastructure types.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Student> Students { get; }
    DbSet<Section> Sections { get; }
    DbSet<Course> Courses { get; }
    DbSet<Grade> Grades { get; }
    DbSet<FeeInvoice> FeeInvoices { get; }
    DbSet<FeeCategory> FeeCategories { get; }
    DbSet<FeeTemplate> FeeTemplates { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    DbSet<Discount> Discounts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
