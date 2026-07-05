using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Dashboard.Dtos;

public record DashboardPublicDto
{
    public int TotalStudents { get; init; }
    public int TotalStaff { get; init; }
    public int TotalParents { get; init; }
    public int TotalClasses { get; init; }
    public string? AcademicYear { get; init; }
    public string? CurrentTerm { get; init; }
    public List<string> Announcements { get; init; } = new();
}