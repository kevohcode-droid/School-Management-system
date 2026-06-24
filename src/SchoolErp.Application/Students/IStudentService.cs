using SchoolErp.Application.Common.Models;
using SchoolErp.Application.Students.Dtos;

namespace SchoolErp.Application.Students;

public interface IStudentService
{
    Task<IReadOnlyList<StudentDto>> GetAllAsync(CancellationToken ct = default);
    Task<Result<StudentDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<StudentDto>> CreateAsync(CreateStudentRequest request, CancellationToken ct = default);
    Task<Result<StudentDto>> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}
