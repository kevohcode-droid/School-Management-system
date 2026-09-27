using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Common;
using SchoolErp.Domain.Entities;
using SchoolErp.Domain.Enums;
using SchoolErp.Infrastructure.Identity;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/user-management")]
[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
public class UserManagementController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserManagementController(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _currentUser = currentUser;
        _userManager = userManager;
    }

    [HttpPost("parents/link-student")]
    public async Task<IActionResult> LinkParentStudent([FromBody] LinkParentStudentRequest request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        if (tenantId is null)
            return Unauthorized();

        var parent = await _db.Parents.FirstOrDefaultAsync(p => p.Id == request.ParentId && p.TenantId == tenantId, ct);
        if (parent is null)
            return NotFound(new { message = "Parent not found." });

        var studentExists = await _db.Students.AnyAsync(s => s.Id == request.StudentId && s.TenantId == tenantId, ct);
        if (!studentExists)
            return NotFound(new { message = "Student not found." });

        if (!string.IsNullOrWhiteSpace(request.ParentUserId))
        {
            var user = await _userManager.FindByIdAsync(request.ParentUserId);
            if (user is null || user.TenantId != tenantId)
                return BadRequest(new { message = "Parent user account is invalid for this tenant." });

            if (!await _userManager.IsInRoleAsync(user, Roles.Parent))
                await _userManager.AddToRoleAsync(user, Roles.Parent);

            parent.UserId = user.Id;
        }

        var existing = await _db.ParentStudents
            .FirstOrDefaultAsync(ps =>
                ps.TenantId == tenantId &&
                ps.ParentId == request.ParentId &&
                ps.StudentId == request.StudentId, ct);

        if (request.IsPrimaryContact)
        {
            var existingPrimaryLinks = await _db.ParentStudents
                .Where(ps => ps.TenantId == tenantId && ps.StudentId == request.StudentId && ps.IsPrimaryContact)
                .ToListAsync(ct);

            foreach (var link in existingPrimaryLinks)
                link.IsPrimaryContact = false;
        }

        if (existing is null)
        {
            existing = new ParentStudent
            {
                TenantId = tenantId.Value,
                ParentId = request.ParentId,
                StudentId = request.StudentId
            };
            _db.ParentStudents.Add(existing);
        }

        existing.RelationshipType = request.RelationshipType;
        existing.IsPrimaryContact = request.IsPrimaryContact;

        await _db.SaveChangesAsync(ct);

        return Ok(new { message = "Parent and student linked successfully." });
    }

    [HttpDelete("parents/{parentId:guid}/students/{studentId:guid}")]
    public async Task<IActionResult> UnlinkParentStudent(Guid parentId, Guid studentId, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        if (tenantId is null)
            return Unauthorized();

        var link = await _db.ParentStudents
            .FirstOrDefaultAsync(ps => ps.TenantId == tenantId && ps.ParentId == parentId && ps.StudentId == studentId, ct);

        if (link is null)
            return NotFound(new { message = "Parent-student link not found." });

        _db.ParentStudents.Remove(link);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}

public sealed class LinkParentStudentRequest
{
    public Guid ParentId { get; set; }
    public string? ParentUserId { get; set; }
    public Guid StudentId { get; set; }
    public RelationshipType RelationshipType { get; set; } = RelationshipType.Guardian;
    public bool IsPrimaryContact { get; set; }
}
