using TmsApi.Models;
public interface ICourseService
{
    Task<CourseModel> AddCourseAsync(string courseCode, string title, int capacity, int enrolledCount);
    Task<CourseModel?> GetByIdAsync(string id);
    Task<IReadOnlyList<CourseModel>> GetAllAsync();
    Task<bool> DeleteAsync(string code);

}


public class CourseService : ICourseService
{



    private readonly Dictionary<string, CourseModel> _store = new();
    private readonly ILogger<CourseService> _logger;
    public CourseService(ILogger<CourseService> logger)
    {

        _logger = logger;
    }



    public Task<CourseModel> AddCourseAsync(string courseCode, string title, int capacity, int enrolledCount)
    {
        var existing = _store.Values
            .FirstOrDefault(e => e.Code == courseCode);
        if (existing is not null)
        {
            _logger.LogWarning(
            "Duplicate course attempt {CourseCode} already exists", existing.Code);
            return Task.FromResult(existing);
        }
        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new CourseModel(id, courseCode, title, capacity, enrolledCount);

        _store[id] = record;
        _logger.LogInformation(
        "added {CourseCode}in record {id} ", courseCode, id);
        return Task.FromResult(record);
    }

    public Task<CourseModel?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var course);
        if (course is null)
        {
            _logger.LogWarning("Course {id} not found", id);
        }
        return Task.FromResult(course);
    }

    public Task<IReadOnlyList<CourseModel>> GetAllAsync()
    {
        IReadOnlyList<CourseModel> all = _store.Values.ToList();
        return Task.FromResult(all);
    }


    public Task<bool> DeleteAsync(string code)
    {
        var removed = _store.Remove(code);
        if (removed)
            _logger.LogInformation("Deleted Course {code}", code);
        else
            _logger.LogWarning("Delete failed Course {code} not found", code);
        return Task.FromResult(removed);
    }
}