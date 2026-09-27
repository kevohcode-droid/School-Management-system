using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Staff;
using SchoolErp.Domain.Entities;

namespace SchoolErp.Application.Staff;

public class StaffService : IStaffService
{
    private readonly IApplicationDbContext _db;
    private readonly IAccountProvisioningService _provisioning;

    public StaffService(IApplicationDbContext db, IAccountProvisioningService provisioning)
    {
        _db = db;
        _provisioning = provisioning;
    }

    public async Task<IReadOnlyList<StaffDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.StaffMembers
            .AsNoTracking()
            .OrderBy(s => s.LastName)
            .Select(s => new StaffDto
            {
                Id = s.Id,
                EmployeeId = s.EmployeeId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                Phone = s.Phone,
                Gender = s.Gender,
                NationalId = s.NationalId,
                Designation = s.Designation,
                Department = s.Department,
                DateOfJoining = s.DateOfJoining,
                EmploymentStatus = s.EmploymentStatus,
                Qualifications = s.Qualifications
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StaffMember>> GetForExportAsync(CancellationToken ct = default)
    {
        return await _db.StaffMembers.AsNoTracking().OrderBy(s => s.LastName).ToListAsync(ct);
    }

    public async Task<CreateStaffResponse> CreateAsync(CreateStaffRequest request, CancellationToken ct = default)
    {
        if (request.CreateLoginAccount &&
            (request.AccountRole is not ("Teacher" or "Staff" or "Accountant") || string.IsNullOrWhiteSpace(request.Email)))
            throw new InvalidOperationException("Choose Teacher, Staff, or Accountant and provide an email to create a login account.");

        var staff = new StaffMember
        {
            EmployeeId = request.EmployeeId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Gender = request.Gender,
            NationalId = request.NationalId,
            Designation = request.Designation,
            Department = request.Department,
            DateOfJoining = request.DateOfJoining,
            EmploymentStatus = request.EmploymentStatus,
            Qualifications = request.Qualifications
        };

        _db.StaffMembers.Add(staff);
        await _db.SaveChangesAsync(ct);

        string? temporaryPassword = null;
        if (request.CreateLoginAccount)
        {
            var provision = await _provisioning.ProvisionAsync(
                staff.TenantId,
                staff.FirstName,
                staff.LastName,
                staff.Email,
                request.AccountRole!,
                ct);

            if (!provision.Succeeded)
            {
                _db.StaffMembers.Remove(staff);
                await _db.SaveChangesAsync(ct);
                throw new InvalidOperationException(string.Join(" ", provision.Errors));
            }

            staff.UserId = provision.UserId;
            temporaryPassword = provision.TemporaryPassword;
            await _db.SaveChangesAsync(ct);
        }

        var staffDto = new StaffDto
        {
            Id = staff.Id,
            EmployeeId = staff.EmployeeId,
            FirstName = staff.FirstName,
            LastName = staff.LastName,
            Email = staff.Email,
            Phone = staff.Phone,
            Gender = staff.Gender,
            NationalId = staff.NationalId,
            Designation = staff.Designation,
            Department = staff.Department,
            DateOfJoining = staff.DateOfJoining,
            EmploymentStatus = staff.EmploymentStatus,
            Qualifications = staff.Qualifications
        };

        return new CreateStaffResponse
        {
            Staff = staffDto,
            LoginEmail = request.CreateLoginAccount ? staff.Email : null,
            TemporaryPassword = temporaryPassword
        };
    }

    public async Task<StaffDto> UpdateAsync(Guid id, UpdateStaffRequest request, CancellationToken ct = default)
    {
        var staff = await _db.StaffMembers.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (staff is null) throw new InvalidOperationException("Staff not found.");

        staff.EmployeeId = request.EmployeeId;
        staff.FirstName = request.FirstName;
        staff.LastName = request.LastName;
        staff.Email = request.Email;
        staff.Phone = request.Phone;
        staff.Gender = request.Gender;
        staff.NationalId = request.NationalId;
        staff.Designation = request.Designation;
        staff.Department = request.Department;
        staff.DateOfJoining = request.DateOfJoining;
        staff.EmploymentStatus = request.EmploymentStatus;
        staff.Qualifications = request.Qualifications;

        await _db.SaveChangesAsync(ct);

        return new StaffDto
        {
            Id = staff.Id,
            EmployeeId = staff.EmployeeId,
            FirstName = staff.FirstName,
            LastName = staff.LastName,
            Email = staff.Email,
            Phone = staff.Phone,
            Gender = staff.Gender,
            NationalId = staff.NationalId,
            Designation = staff.Designation,
            Department = staff.Department,
            DateOfJoining = staff.DateOfJoining,
            EmploymentStatus = staff.EmploymentStatus,
            Qualifications = staff.Qualifications
        };
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var staff = await _db.StaffMembers.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (staff is null) return;

        _db.StaffMembers.Remove(staff);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> ImportFromRowsAsync(IReadOnlyList<StaffImportRow> rows, CancellationToken ct = default)
    {
        int importedCount = 0;
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.EmployeeId))
                continue;

            var existing = await _db.StaffMembers.FirstOrDefaultAsync(s => s.EmployeeId == row.EmployeeId, ct);
            if (existing != null)
                continue;

            var staff = new StaffMember
            {
                EmployeeId = row.EmployeeId,
                FirstName = row.FirstName,
                LastName = row.LastName,
                Email = row.Email,
                Phone = row.Phone,
                Gender = row.Gender,
                NationalId = row.NationalId,
                Designation = row.Designation,
                Department = row.Department,
                DateOfJoining = row.DateOfJoining,
                EmploymentStatus = row.EmploymentStatus,
                Qualifications = row.Qualifications
            };

            _db.StaffMembers.Add(staff);
            importedCount++;
        }

        await _db.SaveChangesAsync(ct);
        return importedCount;
    }
}