using System.Reflection.Metadata.Ecma335;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Models;
using Microsoft.EntityFrameworkCore;
public interface ICourseService
{
    Task<Course> AddCourseAsync(string courseCode, string title, int capacity, int enrolledCount);
    Task<Course?> GetByIdAsync(int id);
    Task<IReadOnlyList<Course>> GetAllAsync();
    Task<bool> DeleteAsync(int id);

}


public class CourseService : ICourseService
{



    private readonly Dictionary<string, CourseModel> _store = new();
    private readonly TmsDbContext _context;
    private readonly ILogger<CourseService> _logger;
    public CourseService(ILogger<CourseService> logger, TmsDbContext context)
    {
        _context = context;
        _logger = logger;
    }



    public async Task<Course> AddCourseAsync(string courseCode, string title, int capacity, int enrolledCount)
    {
        var existing = await _context.Courses
            .FirstOrDefaultAsync(e => e.Code == courseCode);
        if (existing is not null)
        {
            _logger.LogWarning(
            "Duplicate course attempt {CourseCode} already exists", existing.Code);
            return existing;
        }
        // var id = Guid.NewGuid().ToString("N")[..8];
        var course = new Course()
        {
            Code = courseCode,
            Title = title,
            Capacity = capacity
        };

        // _store[id] = record;
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();
        _logger.LogInformation(
        "added {CourseCode}in record {id} ", courseCode, course.Id);
        return course;
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course is null)
        {
            _logger.LogWarning("Course {id} not found", id);
        }
        return course;
    }

    public async Task<IReadOnlyList<Course>> GetAllAsync()
    {
        IReadOnlyList<Course> all = await _context.Courses.ToListAsync();
        return all;
    }


    public async Task<bool> DeleteAsync(int id)
    {
        var course = await _context.Courses.FindAsync(id);

        if (course is null)
        {
            _logger.LogWarning("Delete failed. Course {Id} not found", id);
            return false;
        }

        _context.Courses.Remove(course);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted Course {Id}", id);

        return true;
    }
}