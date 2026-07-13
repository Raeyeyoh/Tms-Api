using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;
public interface IEnrollmentService
{
    Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
    Task<EnrollmentResponseDto?> GetEnrollmentByIdAsync(int courseId, CancellationToken ct);

    Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);
    Task<IReadOnlyList<Enrollment>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateBulk(CancellationToken ct);

}


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
 .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
 .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Enrollment>> GetAllAsync()
    {
        IReadOnlyList<Enrollment> all = await _context.Enrollments.ToListAsync();
        return all;
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

    public Task<EnrollmentResponseDto?> GetEnrollmentByIdAsync(int courseId, CancellationToken ct)
    => _context.Enrollments
 .AsNoTracking()
 .Where(e => e.CourseId == courseId)
 .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
 .FirstOrDefaultAsync(ct);

}

public class TmsDatabaseException(string message) : Exception(message);
