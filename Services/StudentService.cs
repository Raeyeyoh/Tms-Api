using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Models;
using System.Linq;
using System.Globalization;

public interface IStudentService
{
    Task<Student> AddStudentAsync(int IdNo, string regno, string name, int age, decimal gpa);

    Task<Student?> GetByIdAsync(int id);
    Task<IReadOnlyList<Student?>> GetAllStudentsAsync();
    Task<bool> DeleteStudentAsync(int id);
    Task<bool> UpdateStudentAsync(int id, string name, decimal gpa, uint version, CancellationToken ct);
    Task<IActionResult?> SoftDelteAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<Student?>> ShowDeletedAsync();

}



public class StudentService : IStudentService
{

    private readonly Dictionary<string, StudentModel> _store = new();
    private readonly ILogger<StudentService> _logger;
    private readonly TmsDbContext _context;
    public StudentService(ILogger<StudentService> logger, TmsDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<Student> AddStudentAsync(int IdNo, string regno, string name, int age, decimal gpa)
    {
        var existing = await _context.Students.FirstOrDefaultAsync(e => e.Id == IdNo);
        if (existing is not null)
        {
            _logger.LogWarning(
            "Duplicate student attempt {id} already exists", existing.Id);
            return existing;
        }
        var student = new Student { RegistrationNumber = regno, Id = IdNo, Name = name, GPA = gpa };
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();
        // _store[id] = record;
        _logger.LogInformation(
        "added {studid } record {ID} ", IdNo, student.Id);
        return student;
    }

    public async Task<IReadOnlyList<Student?>> GetAllStudentsAsync()
    {
        IReadOnlyList<Student> all = await _context.Students.AsNoTracking().ToListAsync();
        return all;
    }


    public async Task<Student?> GetByIdAsync(int id)
    {
        var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (student is null)
        {
            _logger.LogWarning("student {id} not found", id);
        }
        return student;
    }
    public async Task<IActionResult?> SoftDelteAsync(int id, CancellationToken ct)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student == null)
        { return null; }
        student.IsDeleted = true;
        return null;


    }
    public async Task<bool> DeleteStudentAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);

        if (student is null)
        {
            _logger.LogWarning("Delete failed Student {id} not found", id);
            return false;
        }
        else
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted Student {id}", id);


            return true;
        }


    }

    public async Task<bool> UpdateStudentAsync(int id, string name, decimal gpa, uint version, CancellationToken ct)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student == null)
        { return false; }

        student.Name = name;
        student.GPA = gpa;
        _context.Entry(student)
            .Property("LastUpdated")
            .CurrentValue = DateTime.UtcNow;
        _context.Entry(student)
.Property(s => s.Version)
.OriginalValue = version;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<Student?>> ShowDeletedAsync()
    {
        var students = await _context.Students.IgnoreQueryFilters().Where(s => s.IsDeleted == true).ToListAsync();
        return students;
    }
}