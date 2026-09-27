using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;
using SchoolErp.Domain.Entities;
using SchoolErp.Domain.Common;

namespace SchoolErp.Application.Parents;

public class ParentService : IParentService
{
    private readonly IApplicationDbContext _db;
    private readonly IAccountProvisioningService _provisioning;
    private readonly IAuditService _audit;

    public ParentService(
        IApplicationDbContext db,
        IAccountProvisioningService provisioning,
        IAuditService audit)
    {
        _db = db;
        _provisioning = provisioning;
        _audit = audit;
    }

    public async Task<IReadOnlyList<ParentDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Parents
            .AsNoTracking()
            .OrderBy(p => p.LastName)
            .Select(p => Map(p))
            .ToListAsync(ct);
    }

    public async Task<Result<ParentDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var parent = await _db.Parents.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        return parent is null
            ? Result<ParentDto>.Failure("Parent not found.")
            : Result<ParentDto>.Success(Map(parent));
    }

    public async Task<Result<CreateParentResponse>> CreateAsync(CreateParentRequest request, CancellationToken ct = default)
    {
        // Check for duplicate email within the tenant if an email is provided
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await _db.Parents.AnyAsync(p => p.Email == request.Email, ct);
            if (emailExists)
                return Result<CreateParentResponse>.Failure($"A parent with email '{request.Email}' already exists.");
        }

        var parent = new Parent
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Occupation = request.Occupation
        };

        _db.Parents.Add(parent);
        await _db.SaveChangesAsync(ct);

        // Provision Identity account
        var provision = await _provisioning.ProvisionAsync(
            parent.TenantId,
            request.FirstName,
            request.LastName,
            request.Email,
            Roles.Parent,
            ct);

        if (!provision.Succeeded)
            return Result<CreateParentResponse>.Failure(provision.Errors);

        // Link the Identity user to the parent entity
        parent.UserId = provision.UserId;
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync("system", request.Email ?? request.FirstName,
            "CREATE_PARENT", $"Created parent {request.FirstName} {request.LastName} with account", ct: ct);

        return Result<CreateParentResponse>.Success(new CreateParentResponse
        {
            Parent = Map(parent),
            TemporaryPassword = provision.TemporaryPassword,
            Username = provision.UserId is not null ? $"{request.FirstName.ToLower()}.{request.LastName.ToLower()}" : null
        });
    }

    public async Task<Result<ParentDto>> UpdateAsync(Guid id, UpdateParentRequest request, CancellationToken ct = default)
    {
        var parent = await _db.Parents.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (parent is null)
            return Result<ParentDto>.Failure("Parent not found.");

        parent.FirstName = request.FirstName;
        parent.LastName = request.LastName;
        parent.Email = request.Email;
        parent.Phone = request.Phone;
        parent.Address = request.Address;
        parent.Occupation = request.Occupation;

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("system", request.Email ?? request.FirstName,
            "UPDATE_PARENT", $"Updated parent {parent.FirstName} {parent.LastName}", ct: ct);

        return Result<ParentDto>.Success(Map(parent));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var parent = await _db.Parents.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (parent is null)
            return Result.Failure("Parent not found.");

        _db.Parents.Remove(parent);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("system", parent.Email ?? parent.FirstName,
            "DELETE_PARENT", $"Deleted parent {parent.FirstName} {parent.LastName}", ct: ct);

        return Result.Success();
    }

    public async Task<IReadOnlyList<ParentStudentLinkDto>> GetLinkedStudentsAsync(Guid parentId, CancellationToken ct = default)
    {
        return await _db.ParentStudents
            .AsNoTracking()
            .Where(ps => ps.ParentId == parentId)
            .Select(ps => new ParentStudentLinkDto
            {
                ParentId = ps.ParentId,
                StudentId = ps.StudentId,
                StudentName = ps.Student != null ? ps.Student.FullName : string.Empty,
                AdmissionNumber = ps.Student != null ? ps.Student.AdmissionNumber : string.Empty,
                RelationshipType = ps.RelationshipType,
                IsPrimaryContact = ps.IsPrimaryContact
            })
            .ToListAsync(ct);
    }

    private static ParentDto Map(Parent p) => new()
    {
        Id = p.Id,
        TenantId = p.TenantId,
        UserId = p.UserId,
        FirstName = p.FirstName,
        LastName = p.LastName,
        Email = p.Email,
        Phone = p.Phone,
        Address = p.Address,
        Occupation = p.Occupation,
        CreatedAtUtc = p.CreatedAtUtc
    };
}
