using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Dashboard.Dtos;

public record DashboardSummaryDto
{
    public int TotalStudents { get; init; }
    public int TotalStaff { get; init; }
    public int TotalParents { get; init; }
    public int TotalClasses { get; init; }
    public decimal? AttendancePercentage { get; init; }
    public decimal PendingFees { get; init; }
    public decimal TodayCollections { get; init; }
    public int NewAdmissions { get; init; }
    public int NewStaff { get; init; }
    public decimal MonthlyCollections { get; init; }
    public int ActiveUsers { get; init; }
    public DateTime? LastLogin { get; init; }
    public int Notifications { get; init; }
    public string? AcademicYear { get; init; }
    public string? CurrentTerm { get; init; }
    public string? CurrentSemester { get; init; }
    public List<string> Announcements { get; init; } = new();
}