using System.Reflection.Metadata.Ecma335;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Models;
using Microsoft.EntityFrameworkCore;
using TmsApi.Dtos;
using System.Linq;
public interface ICourseService
{
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest course, CancellationToken ct);
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    //Task<IReadOnlyList<Course>> GetAllAsync();
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);

    Task<bool> DeleteAsync(int id);
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest
request, CancellationToken ct);
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



    public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Created course {CourseId} ({Code})", course.
        Id, course.Code);
        return (await GetByIdAsync(course.Id, ct))!;
    }

    public Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken
ct) => _context.Courses
.AsNoTracking()
.Where(c => c.Id == id)
.Select(c => new CourseResponseDto(
c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
.FirstOrDefaultAsync(ct);

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct)
    {
        IQueryable<Course> query = _context.Courses.AsNoTracking();
        if (!(request.Search is null))

            query = query.Where(c => EF.Functions.ILike(c.Title, $"%{request.Search}%") || EF.Functions.ILike(c.Code, $"%{request.Search}%"));
        var totalCount = await query.CountAsync(ct);

        IQueryable<Course> sortedQuery;

        switch (request.OrderBy)
        {
            case "Code":
                sortedQuery = request.Descending
                    ? query.OrderByDescending(c => c.Code)
                    : query.OrderBy(c => c.Code);
                break;

            case "MaxCapacity":
                sortedQuery = request.Descending
                    ? query.OrderByDescending(c => c.MaxCapacity)
                    : query.OrderBy(c => c.MaxCapacity);
                break;

            default:
                sortedQuery = request.Descending
                    ? query.OrderByDescending(c => c.Title)
                    : query.OrderBy(c => c.Title);
                break;
        }


        var items = await sortedQuery
    .Skip((request.Page - 1) * request.PageSize)
    .Take(request.PageSize)
    .Select(c => new CourseResponseDto(
        c.Id,
        c.Code,
        c.Title,
        c.MaxCapacity,
        c.Enrollments.Count))
    .ToListAsync(ct);
        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
        throw new NotImplementedException();
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>

         _context.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct);


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