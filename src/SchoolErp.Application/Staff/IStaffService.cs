using SchoolErp.Application.Staff;
using SchoolErp.Domain.Entities;

namespace SchoolErp.Application.Staff;

public interface IStaffService
{
    Task<IReadOnlyList<StaffDto>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<StaffMember>> GetForExportAsync(CancellationToken ct = default);
    Task<StaffDto> CreateAsync(CreateStaffRequest request, CancellationToken ct = default);
    Task<StaffDto> UpdateAsync(Guid id, UpdateStaffRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> ImportFromRowsAsync(IReadOnlyList<StaffImportRow> rows, CancellationToken ct = default);
}

public record StaffImportRow
{
    public string EmployeeId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public int Gender { get; init; }
    public string? NationalId { get; init; }
    public string Designation { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string? DateOfJoining { get; init; }
    public string EmploymentStatus { get; init; } = string.Empty;
    public string? Qualifications { get; init; }
}