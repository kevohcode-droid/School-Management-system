using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolErp.Domain.Entities;

namespace SchoolErp.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.Property(t => t.Name).HasMaxLength(200).IsRequired();
        b.Property(t => t.Code).HasMaxLength(50).IsRequired();
        b.HasIndex(t => t.Code).IsUnique();
    }
}

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> b)
    {
        b.Property(s => s.AdmissionNumber).HasMaxLength(50).IsRequired();
        b.Property(s => s.FirstName).HasMaxLength(100).IsRequired();
        b.Property(s => s.LastName).HasMaxLength(100).IsRequired();
        b.Property(s => s.Email).HasMaxLength(256);
        b.Ignore(s => s.FullName);

        b.HasIndex(s => new { s.TenantId, s.AdmissionNumber }).IsUnique();

        b.HasOne(s => s.Section)
            .WithMany(sec => sec.Students)
            .HasForeignKey(s => s.SectionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> b)
    {
        b.Property(s => s.Name).HasMaxLength(150).IsRequired();
        b.Property(s => s.GradeLevel).HasMaxLength(50).IsRequired();
        b.HasIndex(s => new { s.TenantId, s.Name });
    }
}

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> b)
    {
        b.Property(c => c.Name).HasMaxLength(150).IsRequired();
        b.Property(c => c.Code).HasMaxLength(50).IsRequired();
        b.HasIndex(c => new { c.TenantId, c.Code }).IsUnique();

        b.HasOne(c => c.Section)
            .WithMany(s => s.Courses)
            .HasForeignKey(c => c.SectionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> b)
    {
        b.Property(g => g.Score).HasPrecision(6, 2);
        b.Property(g => g.MaxScore).HasPrecision(6, 2);
        b.Property(g => g.Remarks).HasMaxLength(500);
        b.Ignore(g => g.Percentage);

        b.HasOne(g => g.Student)
            .WithMany(s => s.Grades)
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(g => g.Course)
            .WithMany(c => c.Grades)
            .HasForeignKey(g => g.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FeeInvoiceConfiguration : IEntityTypeConfiguration<FeeInvoice>
{
    public void Configure(EntityTypeBuilder<FeeInvoice> b)
    {
        b.Property(f => f.InvoiceNumber).HasMaxLength(50).IsRequired();
        b.Property(f => f.Description).HasMaxLength(500);
        b.Property(f => f.Amount).HasPrecision(12, 2);
        b.Property(f => f.AmountPaid).HasPrecision(12, 2);
        b.Property(f => f.DiscountAmount).HasPrecision(12, 2);
        b.Property(f => f.AcademicYear).HasMaxLength(20);
        b.Property(f => f.Term).HasMaxLength(20);
        b.Ignore(f => f.Balance);
        b.HasIndex(f => new { f.TenantId, f.InvoiceNumber }).IsUnique();

        b.HasOne(f => f.Student)
            .WithMany(s => s.FeeInvoices)
            .HasForeignKey(f => f.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(f => f.Discount)
            .WithMany()
            .HasForeignKey(f => f.DiscountId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class FeeCategoryConfiguration : IEntityTypeConfiguration<FeeCategory>
{
    public void Configure(EntityTypeBuilder<FeeCategory> b)
    {
        b.Property(f => f.Name).HasMaxLength(150).IsRequired();
        b.Property(f => f.Code).HasMaxLength(50).IsRequired();
        b.Property(f => f.Description).HasMaxLength(500);
        b.HasIndex(f => new { f.TenantId, f.Code }).IsUnique();
    }
}

public class FeeTemplateConfiguration : IEntityTypeConfiguration<FeeTemplate>
{
    public void Configure(EntityTypeBuilder<FeeTemplate> b)
    {
        b.Property(f => f.Name).HasMaxLength(200).IsRequired();
        b.Property(f => f.Description).HasMaxLength(500);
    }
}

public class FeeTemplateItemConfiguration : IEntityTypeConfiguration<FeeTemplateItem>
{
    public void Configure(EntityTypeBuilder<FeeTemplateItem> b)
    {
        b.Property(f => f.Amount).HasPrecision(12, 2);

        b.HasOne(f => f.FeeTemplate)
            .WithMany(t => t.Items)
            .HasForeignKey(f => f.FeeTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(f => f.FeeCategory)
            .WithMany(c => c.TemplateItems)
            .HasForeignKey(f => f.FeeCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> b)
    {
        b.Property(p => p.TransactionReference).HasMaxLength(100).IsRequired();
        b.Property(p => p.Amount).HasPrecision(12, 2);
        b.Property(p => p.Notes).HasMaxLength(500);

        b.HasOne(p => p.FeeInvoice)
            .WithMany(i => i.PaymentTransactions)
            .HasForeignKey(p => p.FeeInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> b)
    {
        b.Property(i => i.Description).HasMaxLength(200).IsRequired();
        b.Property(i => i.Amount).HasPrecision(12, 2);

        b.HasOne(i => i.FeeInvoice)
            .WithMany(f => f.Items)
            .HasForeignKey(i => i.FeeInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(i => i.FeeCategory)
            .WithMany(c => c.InvoiceItems)
            .HasForeignKey(i => i.FeeCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> b)
    {
        b.Property(d => d.Code).HasMaxLength(50).IsRequired();
        b.Property(d => d.Name).HasMaxLength(150).IsRequired();
        b.Property(d => d.Description).HasMaxLength(500);
        b.Property(d => d.Value).HasPrecision(10, 2);
        b.HasIndex(d => new { d.TenantId, d.Code }).IsUnique();
    }
}
