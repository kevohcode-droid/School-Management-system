using SchoolErp.Domain.Enums;

namespace SchoolErp.Application.Attendance.Dtos;

public record MarkAttendanceDto(
    Guid StudentId,
    DateTime Date,
    AttendanceStatus Status,
    string? Remarks
);

public record BulkMarkAttendanceDto(
    Guid ClassId,
    DateTime Date,
    List<MarkAttendanceDto> Records
);

public record AttendanceSummaryDto(
    Guid StudentId,
    string StudentName,
    AttendanceStatus Status,
    string? Remarks
);

public record StudentAttendanceDto(
    Guid StudentId,
    string StudentName,
    Guid? SectionId,
    string? SectionName
);
