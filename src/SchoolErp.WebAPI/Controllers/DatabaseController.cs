using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Entities;
using SchoolErp.Infrastructure.Persistence;
using System.Text.Json;

namespace SchoolErp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DatabaseController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DatabaseController(ApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("tables")]
    public async Task<IActionResult> GetTables(CancellationToken ct)
    {
        var tables = new[]
        {
            new { name = "Students", entityType = "Student", recordCount = (int)await _db.Students.LongCountAsync(ct), sizeKb = 0 },
            new { name = "StaffMembers", entityType = "StaffMember", recordCount = (int)await _db.StaffMembers.LongCountAsync(ct), sizeKb = 0 },
            new { name = "Sections", entityType = "Section", recordCount = (int)await _db.Sections.LongCountAsync(ct), sizeKb = 0 },
            new { name = "FeeInvoices", entityType = "FeeInvoice", recordCount = (int)await _db.FeeInvoices.LongCountAsync(ct), sizeKb = 0 },
            new { name = "PaymentTransactions", entityType = "PaymentTransaction", recordCount = (int)await _db.PaymentTransactions.LongCountAsync(ct), sizeKb = 0 },
            new { name = "FeeCategories", entityType = "FeeCategory", recordCount = (int)await _db.FeeCategories.LongCountAsync(ct), sizeKb = 0 },
            new { name = "FeeTemplates", entityType = "FeeTemplate", recordCount = (int)await _db.FeeTemplates.LongCountAsync(ct), sizeKb = 0 },
            new { name = "Courses", entityType = "Course", recordCount = (int)await _db.Courses.LongCountAsync(ct), sizeKb = 0 },
            new { name = "Grades", entityType = "Grade", recordCount = (int)await _db.Grades.LongCountAsync(ct), sizeKb = 0 },
            new { name = "Parents", entityType = "Parent", recordCount = (int)await _db.Parents.LongCountAsync(ct), sizeKb = 0 },
            new { name = "Announcements", entityType = "Announcement", recordCount = (int)await _db.Announcements.LongCountAsync(ct), sizeKb = 0 },
            new { name = "Tenants", entityType = "Tenant", recordCount = (int)await _db.Tenants.LongCountAsync(ct), sizeKb = 0 }
        };

        return Ok(tables.OrderBy(t => t.name));
    }

    [HttpGet("tables/{tableName}/records")]
    public async Task<IActionResult> GetRecords(
        string tableName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        return tableName.ToLower() switch
        {
            "students" => await GetStudents(page, pageSize, search, ct),
            "staffmembers" => await GetStaffMembers(page, pageSize, search, ct),
            "sections" => await GetSections(page, pageSize, search, ct),
            "feeinvoices" => await GetFeeInvoices(page, pageSize, search, ct),
            "paymenttransactions" => await GetPaymentTransactions(page, pageSize, search, ct),
            "feecategories" => await GetFeeCategories(page, pageSize, search, ct),
            "feetemplates" => await GetFeeTemplates(page, pageSize, search, ct),
            "courses" => await GetCourses(page, pageSize, search, ct),
            "grades" => await GetGrades(page, pageSize, search, ct),
            "parents" => await GetParents(page, pageSize, search, ct),
            "announcements" => await GetAnnouncements(page, pageSize, search, ct),
            "tenants" => await GetTenants(page, pageSize, search, ct),
            _ => NotFound($"Table '{tableName}' not found.")
        };
    }

    [HttpGet("tables/{tableName}/statistics")]
    public async Task<IActionResult> GetTableStatistics(string tableName, CancellationToken ct)
    {
        return tableName.ToLower() switch
        {
            "students" => await GetStudentStatistics(ct),
            "staffmembers" => await GetStaffMemberStatistics(ct),
            "sections" => await GetSectionStatistics(ct),
            "feeinvoices" => await GetFeeInvoiceStatistics(ct),
            "paymenttransactions" => await GetPaymentTransactionStatistics(ct),
            "feecategories" => await GetFeeCategoryStatistics(ct),
            "feetemplates" => await GetFeeTemplateStatistics(ct),
            "courses" => await GetCourseStatistics(ct),
            "grades" => await GetGradeStatistics(ct),
            "parents" => await GetParentStatistics(ct),
            "announcements" => await GetAnnouncementStatistics(ct),
            "tenants" => await GetTenantStatistics(ct),
            _ => NotFound($"Table '{tableName}' not found.")
        };
    }

    [HttpPost("tables/{tableName}/records")]
    public async Task<IActionResult> InsertRecord(string tableName, [FromBody] JsonElement record, CancellationToken ct)
    {
        return tableName.ToLower() switch
        {
            "students" => await InsertStudent(record, ct),
            "staffmembers" => await InsertStaffMember(record, ct),
            "sections" => await InsertSection(record, ct),
            "feeinvoices" => await InsertFeeInvoice(record, ct),
            "paymenttransactions" => await InsertPaymentTransaction(record, ct),
            "feecategories" => await InsertFeeCategory(record, ct),
            "feetemplates" => await InsertFeeTemplate(record, ct),
            "courses" => await InsertCourse(record, ct),
            "grades" => await InsertGrade(record, ct),
            "parents" => await InsertParent(record, ct),
            "announcements" => await InsertAnnouncement(record, ct),
            "tenants" => await InsertTenant(record, ct),
            _ => NotFound($"Table '{tableName}' not found.")
        };
    }

    [HttpDelete("tables/{tableName}/records/{id}")]
    public async Task<IActionResult> DeleteRecord(string tableName, string id, CancellationToken ct)
    {
        return tableName.ToLower() switch
        {
            "students" => await DeleteStudent(id, ct),
            "staffmembers" => await DeleteStaffMember(id, ct),
            "sections" => await DeleteSection(id, ct),
            "feeinvoices" => await DeleteFeeInvoice(id, ct),
            "paymenttransactions" => await DeletePaymentTransaction(id, ct),
            "feecategories" => await DeleteFeeCategory(id, ct),
            "feetemplates" => await DeleteFeeTemplate(id, ct),
            "courses" => await DeleteCourse(id, ct),
            "grades" => await DeleteGrade(id, ct),
            "parents" => await DeleteParent(id, ct),
            "announcements" => await DeleteAnnouncement(id, ct),
            "tenants" => await DeleteTenant(id, ct),
            _ => NotFound($"Table '{tableName}' not found.")
        };
    }

    [HttpPost("tables/{tableName}/import")]
    public async Task<IActionResult> ImportRecords(string tableName, IFormFile file, [FromForm] string strategy, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var result = new ImportResult { Total = 0, Imported = 0, Skipped = 0, Errors = new List<string>() };

        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
            return BadRequest("File contains no data rows.");

        return tableName.ToLower() switch
        {
            "students" => await ImportStudents(lines, strategy, result, ct),
            "staffmembers" => await ImportStaffMembers(lines, strategy, result, ct),
            "sections" => await ImportSections(lines, strategy, result, ct),
            _ => Ok(result)
        };
    }

    [HttpGet("tables/{tableName}/export")]
    public async Task<IActionResult> ExportRecords(string tableName, [FromQuery] int page = 1, [FromQuery] int pageSize = 1000, CancellationToken ct = default)
    {
        return tableName.ToLower() switch
        {
            "students" => await ExportStudents(page, pageSize, ct),
            "staffmembers" => await ExportStaffMembers(page, pageSize, ct),
            "sections" => await ExportSections(page, pageSize, ct),
            _ => NotFound($"Table '{tableName}' not found.")
        };
    }

    private async Task<IActionResult> GetStudents(int page, int pageSize, string? search, CancellationToken ct)
    {
        var students = await _db.Students
            .Where(s => string.IsNullOrEmpty(search) || s.AdmissionNumber.Contains(search) || s.FirstName.Contains(search) || s.LastName.Contains(search))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new { s.Id, s.AdmissionNumber, s.FirstName, s.LastName, s.Email, s.Gender, s.DateOfBirth, s.EnrollmentDate, SectionName = s.Section != null ? s.Section.Name : "" })
            .ToListAsync(ct);

        var totalCount = await _db.Students.LongCountAsync(ct);

        return Ok(new { records = students, totalCount });
    }

    private async Task<IActionResult> GetStaffMembers(int page, int pageSize, string? search, CancellationToken ct)
    {
        var staff = await _db.StaffMembers
            .Where(s => string.IsNullOrEmpty(search) || s.FirstName.Contains(search) || s.LastName.Contains(search) || s.EmployeeId.Contains(search))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new { s.Id, s.EmployeeId, s.FirstName, s.LastName, s.Email, s.Designation, s.Department })
            .ToListAsync(ct);

        var totalCount = await _db.StaffMembers.LongCountAsync(ct);

        return Ok(new { records = staff, totalCount });
    }

    private async Task<IActionResult> GetSections(int page, int pageSize, string? search, CancellationToken ct)
    {
        var sections = await _db.Sections
            .Where(s => string.IsNullOrEmpty(search) || s.Name.Contains(search) || s.GradeLevel.Contains(search))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new { s.Id, s.Name, s.GradeLevel, s.Capacity })
            .ToListAsync(ct);

        var totalCount = await _db.Sections.LongCountAsync(ct);

        return Ok(new { records = sections, totalCount });
    }

    private async Task<IActionResult> GetFeeInvoices(int page, int pageSize, string? search, CancellationToken ct)
    {
        var invoices = await _db.FeeInvoices
            .Join(_db.Students, i => i.StudentId, s => s.Id, (i, s) => new { Invoice = i, Student = s })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new { x.Invoice.Id, x.Invoice.InvoiceNumber, x.Invoice.Amount, x.Invoice.AmountPaid, x.Invoice.Status, x.Invoice.IssuedOnUtc, StudentName = $"{x.Student.FirstName} {x.Student.LastName}" })
            .ToListAsync(ct);

        var totalCount = await _db.FeeInvoices.LongCountAsync(ct);

        return Ok(new { records = invoices, totalCount });
    }

    private async Task<IActionResult> GetPaymentTransactions(int page, int pageSize, string? search, CancellationToken ct)
    {
        var payments = await _db.PaymentTransactions
            .Join(_db.FeeInvoices, p => p.FeeInvoiceId, fi => fi.Id, (p, fi) => new { Payment = p, FeeInvoice = fi })
            .Join(_db.Students, x => x.FeeInvoice.StudentId, s => s.Id, (x, s) => new { x.Payment, x.FeeInvoice, Student = s })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new { x.Payment.Id, x.Payment.TransactionReference, x.Payment.Amount, x.Payment.PaymentDateUtc, x.Payment.PaymentMode, x.Payment.Status, StudentName = $"{x.Student.FirstName} {x.Student.LastName}" })
            .ToListAsync(ct);

        var totalCount = await _db.PaymentTransactions.LongCountAsync(ct);

        return Ok(new { records = payments, totalCount });
    }

    private async Task<IActionResult> GetFeeCategories(int page, int pageSize, string? search, CancellationToken ct)
    {
        var categories = await _db.FeeCategories
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new { c.Id, c.Name, c.Description, c.IsActive })
            .ToListAsync(ct);

        var totalCount = await _db.FeeCategories.LongCountAsync(ct);

        return Ok(new { records = categories, totalCount });
    }

    private async Task<IActionResult> GetFeeTemplates(int page, int pageSize, string? search, CancellationToken ct)
    {
        var templates = await _db.FeeTemplates
            .Include(t => t.Items)
            .ThenInclude(i => i.FeeCategory)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new { t.Id, t.Name, t.Description, t.IsActive })
            .ToListAsync(ct);

        var totalCount = await _db.FeeTemplates.LongCountAsync(ct);

        return Ok(new { records = templates, totalCount });
    }

    private async Task<IActionResult> GetCourses(int page, int pageSize, string? search, CancellationToken ct)
    {
        var courses = await _db.Courses
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new { c.Id, c.Name, c.Code, c.Description })
            .ToListAsync(ct);

        var totalCount = await _db.Courses.LongCountAsync(ct);

        return Ok(new { records = courses, totalCount });
    }

    private async Task<IActionResult> GetGrades(int page, int pageSize, string? search, CancellationToken ct)
    {
        var grades = await _db.Grades
            .Join(_db.Students, g => g.StudentId, s => s.Id, (g, s) => new { Grade = g, Student = s })
            .Join(_db.Courses, x => x.Grade.CourseId, c => c.Id, (x, c) => new { x.Grade, x.Student, Course = c })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new { x.Grade.Id, ExamType = x.Grade.ExamType.ToString(), Score = x.Grade.Score, MaxScore = x.Grade.MaxScore, Percentage = x.Grade.Percentage, x.Grade.Remarks, x.Grade.AssessedOnUtc, StudentName = $"{x.Student.FirstName} {x.Student.LastName}", CourseName = x.Course.Name })
            .ToListAsync(ct);

        var totalCount = await _db.Grades.LongCountAsync(ct);

        return Ok(new { records = grades, totalCount });
    }

    private async Task<IActionResult> GetParents(int page, int pageSize, string? search, CancellationToken ct)
    {
        var parents = await _db.Parents
            .Where(p => string.IsNullOrEmpty(search) || p.FirstName.Contains(search) || p.LastName.Contains(search) || (p.Phone != null && p.Phone.Contains(search)))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new { p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address, p.Occupation })
            .ToListAsync(ct);

        var totalCount = await _db.Parents.LongCountAsync(ct);

        return Ok(new { records = parents, totalCount });
    }

    private async Task<IActionResult> GetAnnouncements(int page, int pageSize, string? search, CancellationToken ct)
    {
        var announcements = await _db.Announcements
            .Where(a => string.IsNullOrEmpty(search) || a.Title.Contains(search))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new { a.Id, a.Title, a.Message, a.IsActive, a.StartDate, a.EndDate })
            .ToListAsync(ct);

        var totalCount = await _db.Announcements.LongCountAsync(ct);

        return Ok(new { records = announcements, totalCount });
    }

    private async Task<IActionResult> GetTenants(int page, int pageSize, string? search, CancellationToken ct)
    {
        var tenants = await _db.Tenants
            .Where(t => string.IsNullOrEmpty(search) || t.Name.Contains(search) || t.Code.Contains(search))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new { t.Id, t.Name, t.Code, t.ContactEmail, t.IsActive, t.CreatedAtUtc })
            .ToListAsync(ct);

        var totalCount = await _db.Tenants.LongCountAsync(ct);

        return Ok(new { records = tenants, totalCount });
    }

    private async Task<IActionResult> GetStudentStatistics(CancellationToken ct)
    {
        var count = await _db.Students.LongCountAsync(ct);
        return Ok(new { recordCount = count, tableSizeBytes = count * 256, indexCount = 3, indexes = new[] { new { name = "IX_StudentId", columns = new[] { "Id" } } } });
    }

    private async Task<IActionResult> GetStaffMemberStatistics(CancellationToken ct)
    {
        var count = await _db.StaffMembers.LongCountAsync(ct);
        return Ok(new { recordCount = count, tableSizeBytes = count * 256, indexCount = 2, indexes = new[] { new { name = "IX_StaffMemberId", columns = new[] { "Id" } } } });
    }

    private async Task<IActionResult> GetSectionStatistics(CancellationToken ct)
    {
        var count = await _db.Sections.LongCountAsync(ct);
        return Ok(new { recordCount = count, tableSizeBytes = count * 256, indexCount = 2, indexes = new[] { new { name = "IX_SectionId", columns = new[] { "Id" } } } });
    }

    private async Task<IActionResult> GetFeeInvoiceStatistics(CancellationToken ct)
    {
        var count = await _db.FeeInvoices.LongCountAsync(ct);
        return Ok(new { recordCount = count, tableSizeBytes = count * 256, indexCount = 2, indexes = new[] { new { name = "IX_FeeInvoiceId", columns = new[] { "Id" } } } });
    }

    private async Task<IActionResult> GetPaymentTransactionStatistics(CancellationToken ct)
    {
        var count = await _db.PaymentTransactions.LongCountAsync(ct);
        return Ok(new { recordCount = count, tableSizeBytes = count * 256, indexCount = 2, indexes = new[] { new { name = "IX_PaymentTransactionId", columns = new[] { "Id" } } } });
    }

    private async Task<IActionResult> GetFeeCategoryStatistics(CancellationToken ct)
    {
        var count = await _db.FeeCategories.LongCountAsync(ct);
        return Ok(new { recordCount = count, tableSizeBytes = count * 256, indexCount = 1, indexes = new object[0] });
    }

    private async Task<IActionResult> GetFeeTemplateStatistics(CancellationToken ct)
    {
        var count = await _db.FeeTemplates.LongCountAsync(ct);
        return Ok(new { recordCount = count, tableSizeBytes = count * 256, indexCount = 1, indexes = new object[0] });
    }

    private async Task<IActionResult> GetCourseStatistics(CancellationToken ct)
    {
        var count = await _db.Courses.LongCountAsync(ct);
        return Ok(new { recordCount = count, tableSizeBytes = count * 256, indexCount = 2, indexes = new[] { new { name = "IX_CourseId", columns = new[] { "Id" } } } });
    }

    private async Task<IActionResult> GetGradeStatistics(CancellationToken ct)
    {
        var count = await _db.Grades.LongCountAsync(ct);
        return Ok(new { recordCount = count, tableSizeBytes = count * 256, indexCount = 2, indexes = new[] { new { name = "IX_GradeId", columns = new[] { "Id" } } } });
    }

    private async Task<IActionResult> GetParentStatistics(CancellationToken ct)
    {
        var count = await _db.Parents.LongCountAsync(ct);
        return Ok(new { recordCount = count, tableSizeBytes = count * 256, indexCount = 2, indexes = new[] { new { name = "IX_ParentId", columns = new[] { "Id" } } } });
    }

    private async Task<IActionResult> GetAnnouncementStatistics(CancellationToken ct)
    {
        var count = await _db.Announcements.LongCountAsync(ct);
        return Ok(new { recordCount = count, tableSizeBytes = count * 256, indexCount = 1, indexes = new object[0] });
    }

    private async Task<IActionResult> GetTenantStatistics(CancellationToken ct)
    {
        var count = await _db.Tenants.LongCountAsync(ct);
        return Ok(new { recordCount = count, tableSizeBytes = count * 256, indexCount = 2, indexes = new[] { new { name = "IX_TenantId", columns = new[] { "Id" } } } });
    }

    private async Task<IActionResult> InsertStudent([FromBody] JsonElement record, CancellationToken ct)
    {
        var json = JsonSerializer.Deserialize<Student>(record.GetRawText());
        if (json == null) return BadRequest("Invalid student data.");
        _db.Students.Add(json);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Student record inserted successfully" });
    }

    private async Task<IActionResult> InsertStaffMember([FromBody] JsonElement record, CancellationToken ct)
    {
        var json = JsonSerializer.Deserialize<StaffMember>(record.GetRawText());
        if (json == null) return BadRequest("Invalid staff member data.");
        _db.StaffMembers.Add(json);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Staff member record inserted successfully" });
    }

    private async Task<IActionResult> InsertSection([FromBody] JsonElement record, CancellationToken ct)
    {
        var json = JsonSerializer.Deserialize<Section>(record.GetRawText());
        if (json == null) return BadRequest("Invalid section data.");
        _db.Sections.Add(json);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Section record inserted successfully" });
    }

    private async Task<IActionResult> InsertFeeInvoice([FromBody] JsonElement record, CancellationToken ct)
    {
        var json = JsonSerializer.Deserialize<FeeInvoice>(record.GetRawText());
        if (json == null) return BadRequest("Invalid fee invoice data.");
        _db.FeeInvoices.Add(json);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Fee invoice record inserted successfully" });
    }

    private async Task<IActionResult> InsertPaymentTransaction([FromBody] JsonElement record, CancellationToken ct)
    {
        var json = JsonSerializer.Deserialize<PaymentTransaction>(record.GetRawText());
        if (json == null) return BadRequest("Invalid payment transaction data.");
        _db.PaymentTransactions.Add(json);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Payment transaction record inserted successfully" });
    }

    private async Task<IActionResult> InsertFeeCategory([FromBody] JsonElement record, CancellationToken ct)
    {
        var json = JsonSerializer.Deserialize<FeeCategory>(record.GetRawText());
        if (json == null) return BadRequest("Invalid fee category data.");
        _db.FeeCategories.Add(json);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Fee category record inserted successfully" });
    }

    private async Task<IActionResult> InsertFeeTemplate([FromBody] JsonElement record, CancellationToken ct)
    {
        var json = JsonSerializer.Deserialize<FeeTemplate>(record.GetRawText());
        if (json == null) return BadRequest("Invalid fee template data.");
        _db.FeeTemplates.Add(json);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Fee template record inserted successfully" });
    }

    private async Task<IActionResult> InsertCourse([FromBody] JsonElement record, CancellationToken ct)
    {
        var json = JsonSerializer.Deserialize<Course>(record.GetRawText());
        if (json == null) return BadRequest("Invalid course data.");
        _db.Courses.Add(json);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Course record inserted successfully" });
    }

    private async Task<IActionResult> InsertGrade([FromBody] JsonElement record, CancellationToken ct)
    {
        var json = JsonSerializer.Deserialize<Grade>(record.GetRawText());
        if (json == null) return BadRequest("Invalid grade data.");
        _db.Grades.Add(json);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Grade record inserted successfully" });
    }

    private async Task<IActionResult> InsertParent([FromBody] JsonElement record, CancellationToken ct)
    {
        var json = JsonSerializer.Deserialize<Parent>(record.GetRawText());
        if (json == null) return BadRequest("Invalid parent data.");
        _db.Parents.Add(json);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Parent record inserted successfully" });
    }

    private async Task<IActionResult> InsertAnnouncement([FromBody] JsonElement record, CancellationToken ct)
    {
        var json = JsonSerializer.Deserialize<Announcement>(record.GetRawText());
        if (json == null) return BadRequest("Invalid announcement data.");
        _db.Announcements.Add(json);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Announcement record inserted successfully" });
    }

    private async Task<IActionResult> InsertTenant([FromBody] JsonElement record, CancellationToken ct)
    {
        var json = JsonSerializer.Deserialize<Tenant>(record.GetRawText());
        if (json == null) return BadRequest("Invalid tenant data.");
        _db.Tenants.Add(json);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Tenant record inserted successfully" });
    }

    private async Task<IActionResult> DeleteStudent(string id, CancellationToken ct)
    {
        if (Guid.TryParse(id, out var studentId))
        {
            var student = await _db.Students.FindAsync([studentId], ct);
            if (student != null)
            {
                _db.Students.Remove(student);
                await _db.SaveChangesAsync(ct);
            }
        }
        return Ok(new { message = "Student record deleted" });
    }

    private async Task<IActionResult> DeleteStaffMember(string id, CancellationToken ct)
    {
        if (Guid.TryParse(id, out var staffId))
        {
            var staff = await _db.StaffMembers.FindAsync([staffId], ct);
            if (staff != null)
            {
                _db.StaffMembers.Remove(staff);
                await _db.SaveChangesAsync(ct);
            }
        }
        return Ok(new { message = "Staff member record deleted" });
    }

    private async Task<IActionResult> DeleteSection(string id, CancellationToken ct)
    {
        if (Guid.TryParse(id, out var sectionId))
        {
            var section = await _db.Sections.FindAsync([sectionId], ct);
            if (section != null)
            {
                _db.Sections.Remove(section);
                await _db.SaveChangesAsync(ct);
            }
        }
        return Ok(new { message = "Section record deleted" });
    }

    private async Task<IActionResult> DeleteFeeInvoice(string id, CancellationToken ct)
    {
        if (Guid.TryParse(id, out var invoiceId))
        {
            var invoice = await _db.FeeInvoices.FindAsync([invoiceId], ct);
            if (invoice != null)
            {
                _db.FeeInvoices.Remove(invoice);
                await _db.SaveChangesAsync(ct);
            }
        }
        return Ok(new { message = "Fee invoice record deleted" });
    }

    private async Task<IActionResult> DeletePaymentTransaction(string id, CancellationToken ct)
    {
        if (Guid.TryParse(id, out var paymentId))
        {
            var payment = await _db.PaymentTransactions.FindAsync([paymentId], ct);
            if (payment != null)
            {
                _db.PaymentTransactions.Remove(payment);
                await _db.SaveChangesAsync(ct);
            }
        }
        return Ok(new { message = "Payment transaction record deleted" });
    }

    private async Task<IActionResult> DeleteFeeCategory(string id, CancellationToken ct)
    {
        if (Guid.TryParse(id, out var categoryId))
        {
            var category = await _db.FeeCategories.FindAsync([categoryId], ct);
            if (category != null)
            {
                _db.FeeCategories.Remove(category);
                await _db.SaveChangesAsync(ct);
            }
        }
        return Ok(new { message = "Fee category record deleted" });
    }

    private async Task<IActionResult> DeleteFeeTemplate(string id, CancellationToken ct)
    {
        if (Guid.TryParse(id, out var templateId))
        {
            var template = await _db.FeeTemplates.FindAsync([templateId], ct);
            if (template != null)
            {
                _db.FeeTemplates.Remove(template);
                await _db.SaveChangesAsync(ct);
            }
        }
        return Ok(new { message = "Fee template record deleted" });
    }

    private async Task<IActionResult> DeleteCourse(string id, CancellationToken ct)
    {
        if (Guid.TryParse(id, out var courseId))
        {
            var course = await _db.Courses.FindAsync([courseId], ct);
            if (course != null)
            {
                _db.Courses.Remove(course);
                await _db.SaveChangesAsync(ct);
            }
        }
        return Ok(new { message = "Course record deleted" });
    }

    private async Task<IActionResult> DeleteGrade(string id, CancellationToken ct)
    {
        if (Guid.TryParse(id, out var gradeId))
        {
            var grade = await _db.Grades.FindAsync([gradeId], ct);
            if (grade != null)
            {
                _db.Grades.Remove(grade);
                await _db.SaveChangesAsync(ct);
            }
        }
        return Ok(new { message = "Grade record deleted" });
    }

    private async Task<IActionResult> DeleteParent(string id, CancellationToken ct)
    {
        if (Guid.TryParse(id, out var parentId))
        {
            var parent = await _db.Parents.FindAsync([parentId], ct);
            if (parent != null)
            {
                _db.Parents.Remove(parent);
                await _db.SaveChangesAsync(ct);
            }
        }
        return Ok(new { message = "Parent record deleted" });
    }

    private async Task<IActionResult> DeleteAnnouncement(string id, CancellationToken ct)
    {
        if (Guid.TryParse(id, out var announcementId))
        {
            var announcement = await _db.Announcements.FindAsync([announcementId], ct);
            if (announcement != null)
            {
                _db.Announcements.Remove(announcement);
                await _db.SaveChangesAsync(ct);
            }
        }
        return Ok(new { message = "Announcement record deleted" });
    }

    private async Task<IActionResult> DeleteTenant(string id, CancellationToken ct)
    {
        if (Guid.TryParse(id, out var tenantId))
        {
            var tenant = await _db.Tenants.FindAsync([tenantId], ct);
            if (tenant != null)
            {
                _db.Tenants.Remove(tenant);
                await _db.SaveChangesAsync(ct);
            }
        }
        return Ok(new { message = "Tenant record deleted" });
    }

    private async Task<IActionResult> ImportStudents(string[] lines, string strategy, ImportResult result, CancellationToken ct)
    {
        foreach (var line in lines.Skip(1))
        {
            result.Total++;
            var values = line.Split(',');
            if (values.Length < 7)
            {
                result.Skipped++;
                continue;
            }

            try
            {
                var student = new Student
                {
                    AdmissionNumber = values[0],
                    FirstName = values[1],
                    LastName = values[2],
                    Email = values[3],
                    EnrollmentDate = DateTime.TryParse(values[6], out var enrollDate) ? enrollDate : DateTime.UtcNow
                };
                _db.Students.Add(student);
                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {result.Total}: {ex.Message}");
                result.Skipped++;
            }
        }

        await _db.SaveChangesAsync(ct);
        return Ok(result);
    }

    private async Task<IActionResult> ImportStaffMembers(string[] lines, string strategy, ImportResult result, CancellationToken ct)
    {
        foreach (var line in lines.Skip(1))
        {
            result.Total++;
            var values = line.Split(',');
            if (values.Length < 4)
            {
                result.Skipped++;
                continue;
            }

            try
            {
                var staff = new StaffMember
                {
                    EmployeeId = values[0],
                    FirstName = values[1],
                    LastName = values[2],
                    Email = values[3],
                    Designation = values.Length > 4 ? values[4] : "",
                    Department = values.Length > 5 ? values[5] : ""
                };
                _db.StaffMembers.Add(staff);
                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {result.Total}: {ex.Message}");
                result.Skipped++;
            }
        }

        await _db.SaveChangesAsync(ct);
        return Ok(result);
    }

    private async Task<IActionResult> ImportSections(string[] lines, string strategy, ImportResult result, CancellationToken ct)
    {
        foreach (var line in lines.Skip(1))
        {
            result.Total++;
            var values = line.Split(',');
            if (values.Length < 2)
            {
                result.Skipped++;
                continue;
            }

            try
            {
                var section = new Section
                {
                    Name = values[0],
                    GradeLevel = values[1]
                };
                _db.Sections.Add(section);
                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {result.Total}: {ex.Message}");
                result.Skipped++;
            }
        }

        await _db.SaveChangesAsync(ct);
        return Ok(result);
    }

    private async Task<IActionResult> ExportStudents(int page, int pageSize, CancellationToken ct)
    {
        var students = await _db.Students
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var json = JsonSerializer.Serialize(students, new JsonSerializerOptions { WriteIndented = true });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        return File(bytes, "application/json", "students.json");
    }

    private async Task<IActionResult> ExportStaffMembers(int page, int pageSize, CancellationToken ct)
    {
        var staff = await _db.StaffMembers
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var json = JsonSerializer.Serialize(staff, new JsonSerializerOptions { WriteIndented = true });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        return File(bytes, "application/json", "staffmembers.json");
    }

    private async Task<IActionResult> ExportSections(int page, int pageSize, CancellationToken ct)
    {
        var sections = await _db.Sections
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var json = JsonSerializer.Serialize(sections, new JsonSerializerOptions { WriteIndented = true });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        return File(bytes, "application/json", "sections.json");
    }
}

public class ImportResult
{
    public int Total { get; set; }
    public int Imported { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Duplicates { get; set; } = new();
}