using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Models;
public interface IEnrollmentService
{
    Task<Enrollment> EnrollAsync(int studentId, int courseId);
    Task<Enrollment?> GetByIdAsync(int id);
    Task<IReadOnlyList<Enrollment>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateBulk(CancellationToken ct);

}


public class EnrollmentService : IEnrollmentService
{
    private readonly Dictionary<string, EnrollmentRecord> _store = new();
    private readonly ILogger<EnrollmentService> _logger;
    private readonly TmsDbContext _context;

    public EnrollmentService(ILogger<EnrollmentService> logger, TmsDbContext context)
    {
        _logger = logger;
        _context = context;
    }



    public async Task<Enrollment> EnrollAsync(int studentId, int courseID)
    {
        var existing = await _context.Enrollments.FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseID);
        if (existing is not null)
        {
            _logger.LogWarning(
            "Duplicate enrollment attempt {StudentId} already in {CourseCode} (record {EnrollmentId})", studentId, courseID, existing.Id);
            return existing;
        }
        //var id = Guid.NewGuid().ToString("N")[..8];
        var enrollment = new Enrollment { StudentId = studentId, CourseId = courseID, EnrolledAt = DateTime.UtcNow };
        await _context.Enrollments.AddAsync(enrollment);
        await _context.SaveChangesAsync();

        // _store[id] = record;
        _logger.LogInformation(
        "Enrolled {StudentId} in {CourseCode} record {EnrollmentId}", studentId, courseID, enrollment.Id);
        return enrollment;
    }
    public async Task<Enrollment?> GetByIdAsync(int id)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment is null)
        {
            _logger.LogWarning("Enrollment {EnrollmentId} not found", id);
        }
        return enrollment;
    }

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
}

public class TmsDatabaseException(string message) : Exception(message);
