using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Attendance.Dtos;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Entities;
using SchoolErp.Infrastructure.Persistence;

namespace SchoolErp.Infrastructure;

public class AttendanceService : IAttendanceService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public AttendanceService(ApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task BulkMarkAttendanceAsync(BulkMarkAttendanceDto dto, Guid userId)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;
        
        // Check if attendance already exists for this class/date to prevent duplicates
        var existingRecords = await _context.AttendanceRecords
            .Where(a => a.TenantId == tenantId && a.ClassId == dto.ClassId && a.Date == dto.Date)
            .Select(a => a.StudentId)
            .ToListAsync();

        var newRecords = new List<AttendanceRecord>();

        foreach (var record in dto.Records)
        {
            if (existingRecords.Contains(record.StudentId)) continue; // Skip if already marked

            newRecords.Add(new AttendanceRecord
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                StudentId = record.StudentId,
                ClassId = dto.ClassId,
                Date = record.Date,
                Status = record.Status,
                Remarks = record.Remarks,
                MarkedByUserId = userId
            });
        }

        if (newRecords.Any())
        {
            _context.AttendanceRecords.AddRange(newRecords);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<AttendanceSummaryDto>> GetDailyAttendanceAsync(Guid classId, DateTime date)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;
        
        return await _context.AttendanceRecords
            .Where(a => a.TenantId == tenantId && a.ClassId == classId && a.Date == date)
            .Join(_context.Students, 
                  a => a.StudentId, 
                  s => s.Id, 
                  (a, s) => new AttendanceSummaryDto(s.Id, s.FullName, a.Status, a.Remarks))
            .ToListAsync();
    }

    public async Task<List<StudentAttendanceDto>> GetClassStudentsAsync(Guid classId)
    {
        var tenantId = _currentUser.TenantId ?? Guid.Empty;
        
        return await _context.Students
            .Where(s => s.TenantId == tenantId && s.SectionId == classId)
            .Select(s => new StudentAttendanceDto(
                s.Id,
                s.FullName,
                s.SectionId,
                s.Section != null ? s.Section.Name : null
            ))
            .ToListAsync();
    }
}
