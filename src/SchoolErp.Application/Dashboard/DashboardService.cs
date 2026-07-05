using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Dashboard.Dtos;
using SchoolErp.Application.Settings;
using SchoolErp.Domain.Entities;
using SchoolErp.Domain.Enums;

namespace SchoolErp.Application.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _db;
    private readonly ISettingsService _settingsService;
    private readonly ICurrentUser _currentUser;

    public DashboardService(IApplicationDbContext db, ISettingsService settingsService, ICurrentUser currentUser)
    {
        _db = db;
        _settingsService = settingsService;
        _currentUser = currentUser;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var startOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalStudents = await _db.Students.CountAsync(ct);
        var totalClasses = await _db.Sections.CountAsync(ct);

        var attendanceRecords = await _db.AttendanceRecords
            .Where(a => a.Date.Date == today)
            .ToListAsync(ct);

        var attendancePercentage = attendanceRecords.Count > 0
            ? (decimal?)attendanceRecords.Count(a => a.Status == AttendanceStatus.Present) / attendanceRecords.Count * 100
            : null;

        var pendingInvoices = await _db.FeeInvoices
            .Where(f => f.Status != FeeInvoiceStatus.Paid)
            .Select(f => new { f.Amount, f.AmountPaid, f.DiscountAmount })
            .ToListAsync(ct);

        var pendingFees = pendingInvoices.Sum(x => x.Amount - x.AmountPaid - x.DiscountAmount);

        var todayCollections = await _db.PaymentTransactions
            .Where(p => p.PaymentDateUtc.Date == today)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        var newAdmissions = await _db.Students
            .CountAsync(s => s.EnrollmentDate >= startOfMonth, ct);

        var monthlyCollections = await _db.PaymentTransactions
            .Where(p => p.PaymentDateUtc >= startOfMonth)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        var academicYear = await _settingsService.GetSettingAsync("Academic", "AcademicYear", ct);
        var currentTerm = await _settingsService.GetSettingAsync("Academic", "CurrentTerm", ct);
        var currentSemester = await _settingsService.GetSettingAsync("Academic", "CurrentSemester", ct);

        return new DashboardSummaryDto
        {
            TotalStudents = totalStudents,
            TotalStaff = 0,
            TotalClasses = totalClasses,
            AttendancePercentage = attendancePercentage,
            PendingFees = pendingFees,
            TodayCollections = todayCollections,
            NewAdmissions = newAdmissions,
            NewStaff = 0,
            MonthlyCollections = monthlyCollections,
            ActiveUsers = 0,
            LastLogin = null,
            Notifications = 0,
            AcademicYear = academicYear,
            CurrentTerm = currentTerm,
            CurrentSemester = currentSemester
        };
    }

    public async Task<DashboardPublicDto> GetPublicSummaryAsync(CancellationToken ct = default)
    {
        var totalStudents = await _db.Students.IgnoreQueryFilters().CountAsync(ct);
        var totalStaff = await _db.StaffMembers.IgnoreQueryFilters().CountAsync(ct);
        var totalParents = await _db.Parents.IgnoreQueryFilters().CountAsync(ct);
        var totalClasses = await _db.Sections.IgnoreQueryFilters().CountAsync(ct);

        var academicYear = await _db.TenantSettings
            .IgnoreQueryFilters()
            .Where(s => s.Category == "Academic" && s.Key == "AcademicYear")
            .Select(s => s.Value)
            .FirstOrDefaultAsync(ct);

        var currentTerm = await _db.TenantSettings
            .IgnoreQueryFilters()
            .Where(s => s.Category == "Academic" && s.Key == "CurrentTerm")
            .Select(s => s.Value)
            .FirstOrDefaultAsync(ct);

        var announcements = await _db.Announcements
            .IgnoreQueryFilters()
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => a.Title)
            .Take(5)
            .ToListAsync(ct);

        return new DashboardPublicDto
        {
            TotalStudents = totalStudents,
            TotalStaff = totalStaff,
            TotalParents = totalParents,
            TotalClasses = totalClasses,
            AcademicYear = academicYear,
            CurrentTerm = currentTerm,
            Announcements = announcements
        };
    }
}