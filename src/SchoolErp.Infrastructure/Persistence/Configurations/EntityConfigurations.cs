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
        b.Ignore(f => f.Balance);
        b.HasIndex(f => new { f.TenantId, f.InvoiceNumber }).IsUnique();

        b.HasOne(f => f.Student)
            .WithMany(s => s.FeeInvoices)
            .HasForeignKey(f => f.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
