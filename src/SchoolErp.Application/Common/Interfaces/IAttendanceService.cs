using SchoolErp.Application.Attendance.Dtos;

namespace SchoolErp.Application.Common.Interfaces;

public interface IAttendanceService
{
    Task BulkMarkAttendanceAsync(BulkMarkAttendanceDto dto, Guid userId);
    Task<List<AttendanceSummaryDto>> GetDailyAttendanceAsync(Guid classId, DateTime date);
    Task<List<StudentAttendanceDto>> GetClassStudentsAsync(Guid classId);
}
