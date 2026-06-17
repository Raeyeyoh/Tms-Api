using TmsApi.Models;
public interface IStudentService
{
    Task<StudentModel> AddStudentAsync(String IdNo, string name, int age, decimal gpa);

    Task<StudentModel?> GetByIdAsync(String id);
    Task<IReadOnlyList<StudentModel>> GetAllStudentsAsync();
    Task<bool> DeleteStudentAsync(String id);

}



public class StudentService : IStudentService
{

    private readonly Dictionary<string, StudentModel> _store = new();
    private readonly ILogger<StudentService> _logger;
    public StudentService(ILogger<StudentService> logger)
    {
        _logger = logger;
    }

    public Task<StudentModel> AddStudentAsync(string IdNo, string name, int age, decimal gpa)
    {
        var existing = _store.Values
           .FirstOrDefault(e => e.IdNo == IdNo);
        if (existing is not null)
        {
            _logger.LogWarning(
            "Duplicate student attempt {id} already exists", existing.Id);
            return Task.FromResult(existing);
        }
        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new StudentModel(id, IdNo, name, age, gpa);

        _store[id] = record;
        _logger.LogInformation(
        "added {studid } record {ID} ", IdNo, id);
        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<StudentModel>> GetAllStudentsAsync()
    {
        IReadOnlyList<StudentModel> all = _store.Values.ToList();
        return Task.FromResult(all);
    }


    public Task<StudentModel?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var record);
        if (record is null)
        {
            _logger.LogWarning("student {id} not found", id);
        }
        return Task.FromResult(record);
    }
    public Task<bool> DeleteStudentAsync(string id)
    {
        var removed = _store.Remove(id);
        if (removed)
            _logger.LogInformation("Deleted Student {id}", id);
        else
            _logger.LogWarning("Delete failed Student {id} not found", id);
        return Task.FromResult(removed);
    }
}