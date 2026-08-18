using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence.Data;

namespace TmsApi.Infrastructure.Services;



public class EnrollmentService : IEnrollmentService
{
    //private readonly Dictionary<string, EnrollmentRecord> _store = new();
    private readonly ILogger<EnrollmentService> _logger;
    private readonly TmsDbContext _context;

    public EnrollmentService(ILogger<EnrollmentService> logger, TmsDbContext context)
    {
        _logger = logger;
        _context = context;
    }



    public async Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {

        var enrollment = new Enrollment { StudentId = request.StudentId, CourseId = courseId, EnrolledAt = DateTime.UtcNow };
        await _context.Enrollments.AddAsync(enrollment, ct);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Enrolled {StudentId} in {CourseCode} record {EnrollmentId}", request.StudentId, courseId, enrollment.Id);
        return await GetByIdAsync(enrollment.CourseId, enrollment.Id, ct);
    }
    public Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct) => _context.Enrollments
        .AsNoTracking()
        .Where(e => e.Id == id && e.CourseId == courseId)
        .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.Course, e.EnrolledAt))
        .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Enrollment>> GetAllAsync()
    {
        IReadOnlyList<Enrollment> all = await _context.Enrollments.ToListAsync();
        return all;
    }
    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetByStudentIdAsync(int studentId, CancellationToken ct)
    {
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.Course, e.EnrolledAt))
            .ToListAsync(ct);

        return enrollments;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);

        if (enrollment is null)
        {
            _logger.LogWarning("Delete failed enrollment {EnrollmentId} not found", id);
            return false;
        }
        else
            _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);

        return true;


    }
    public async Task<bool> UpdateBulk(CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow;

        var enrollments = await _context.Enrollments
            .Where(e => e.EnrolledAt < cutoff)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(e => e.IsArchived, true), ct);

        return enrollments > 0;
    }

    public Task<List<EnrollmentResponseDto1>> GetEnrollmentByIdAsync(int courseId,
    CancellationToken ct) => _context.Enrollments
        .AsNoTracking()
        .Where(e => e.CourseId == courseId)
        .Select(e => new EnrollmentResponseDto1(
    e.Id,
    e.CourseId,
    e.StudentId,
    e.Student.Name,
    e.Course.Title,
    e.Status,
    e.EnrolledAt
))
        .ToListAsync(ct);

    public Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct)
    {
        return _context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId && e.Course.Code == courseCode)
            .AnyAsync(ct);
    }
    public async Task<Enrollment> AddAsync(Enrollment request, CancellationToken ct)
    {
        await _context.Enrollments.AddAsync(request, ct);
        await _context.SaveChangesAsync(ct);
        return request;
    }

    public async Task<bool> ApproveAsync(int id, CancellationToken ct)
    {
        var enrollment = await _context.Enrollments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (enrollment != null)

        {
            enrollment.Status = "Approved";
            _context.Enrollments.Update(enrollment);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        return false;

    }

    public Task<EnrollmentResponseDto1?> GetByIdEnrolmentAsync(int id, CancellationToken ct)
    {
        return _context.Enrollments.AsNoTracking().Where(e => e.Id == id).Select(e => new EnrollmentResponseDto1(
   e.Id,
   e.CourseId,
   e.StudentId,
   e.Student.Name,
   e.Course.Title,
   e.Status,
   e.EnrolledAt
)).FirstOrDefaultAsync(ct);
    }
}

public class TmsDatabaseException(string message) : Exception(message);