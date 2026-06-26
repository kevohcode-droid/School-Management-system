using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Common;
using SchoolErp.Domain.Entities;
using SchoolErp.Infrastructure.Identity;

namespace SchoolErp.Infrastructure.Persistence;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole, string>, IApplicationDbContext
{
    private readonly ICurrentUser _currentUser;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUser currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<FeeInvoice> FeeInvoices => Set<FeeInvoice>();
    public DbSet<FeeCategory> FeeCategories => Set<FeeCategory>();
    public DbSet<FeeTemplate> FeeTemplates => Set<FeeTemplate>();
    public DbSet<FeeTemplateItem> FeeTemplateItems => Set<FeeTemplateItem>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Discount> Discounts => Set<Discount>();

    /// <summary>
    /// Tenant referenced by the global query filters. Read per-query by EF Core,
    /// so it always reflects the current request's authenticated tenant.
    /// </summary>
    public Guid CurrentTenantId => _currentUser.TenantId ?? Guid.Empty;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Apply a TenantId == CurrentTenantId global query filter to every
        // entity implementing ITenantEntity, preventing cross-school leaks.
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var tenantProperty = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
            var currentTenant = Expression.Property(
                Expression.Constant(this), nameof(CurrentTenantId));
            var body = Expression.Equal(tenantProperty, currentTenant);
            var lambda = Expression.Lambda(body, parameter);

            builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantAndAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyTenantAndAudit();
        return base.SaveChanges();
    }

    private void ApplyTenantAndAudit()
    {
        var now = DateTime.UtcNow;
        var user = _currentUser.UserName ?? _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is ITenantEntity tenantEntity
                && entry.State == EntityState.Added
                && tenantEntity.TenantId == Guid.Empty
                && _currentUser.TenantId is { } tenantId)
            {
                tenantEntity.TenantId = tenantId;
            }

            if (entry.Entity is AuditableEntity auditable)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditable.CreatedAtUtc = now;
                        auditable.CreatedBy = user;
                        break;
                    case EntityState.Modified:
                        auditable.UpdatedAtUtc = now;
                        auditable.UpdatedBy = user;
                        break;
                }
            }
        }
    }
}
