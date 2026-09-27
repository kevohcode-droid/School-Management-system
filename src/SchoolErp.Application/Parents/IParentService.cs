using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Parents;

public interface IParentService
{
    Task<IReadOnlyList<ParentDto>> GetAllAsync(CancellationToken ct = default);
    Task<Result<ParentDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<CreateParentResponse>> CreateAsync(CreateParentRequest request, CancellationToken ct = default);
    Task<Result<ParentDto>> UpdateAsync(Guid id, UpdateParentRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ParentStudentLinkDto>> GetLinkedStudentsAsync(Guid parentId, CancellationToken ct = default);
}
