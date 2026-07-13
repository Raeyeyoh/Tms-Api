using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

using TmsApi.Dtos;

public interface IStudentService
{
    Task<StudentResponseDto> CreateAsync(RegisterStudentDto student, CancellationToken ct);

    Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(PagedRequest request, CancellationToken ct);
    Task<bool> DeleteStudentAsync(int id);
    Task<bool> UpdateStudentAsync(UpdatestudentRequest request, CancellationToken ct);
    Task<IActionResult?> SoftDelteAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<StudentResponseDto?>> ShowDeletedAsync();
    Task<bool> CodeExistsAsync(string registrationNumber, CancellationToken ct);


}



public class StudentService : IStudentService
{

    private readonly ILogger<StudentService> _logger;
    private readonly TmsDbContext _context;
    public StudentService(ILogger<StudentService> logger, TmsDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<StudentResponseDto> CreateAsync(RegisterStudentDto stud, CancellationToken ct)
    {

        var student = new Student { RegistrationNumber = stud.RegistrationNumber, Name = stud.Name, GPA = stud.GPA };
        await _context.Students.AddAsync(student, ct);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation(
        "added {studid }  {ID} ", student.Id, student.RegistrationNumber);
        return (await GetByIdAsync(student.Id, ct))!;
    }

    public async Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(PagedRequest request, CancellationToken ct)
    {
        IQueryable<Student> query = _context.Students.AsNoTracking();
        if (!(request.Search is null))

            query = query.Where(s => EF.Functions.ILike(s.Name, $"%{request.Search}%") || EF.Functions.ILike(s.GPA.ToString(), $"%{request.Search}%"));
        var totalCount = await query.CountAsync(ct);

        IQueryable<Student> sortedQuery;

        switch (request.OrderBy)
        {
            case "RegistrationNumber":
                sortedQuery = request.Descending
                    ? query.OrderByDescending(s => s.RegistrationNumber)
                    : query.OrderBy(s => s.RegistrationNumber);
                break;

            case "GPA":
                sortedQuery = request.Descending
                    ? query.OrderByDescending(s => s.GPA)
                    : query.OrderBy(s => s.GPA);
                break;

            default:
                sortedQuery = request.Descending
                    ? query.OrderByDescending(s => s.Name)
                    : query.OrderBy(s => s.Name);
                break;
        }


        var items = await sortedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StudentResponseDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.GPA))
            .ToListAsync(ct);
        return new PagedResponse<StudentResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };


    }


    public Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct) => _context.Students.AsNoTracking().Where(s => s.Id == id).Select(s => new StudentResponseDto(s.Id, s.RegistrationNumber, s.Name, s.GPA)).FirstOrDefaultAsync(ct);


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

    public async Task<bool> UpdateStudentAsync(UpdatestudentRequest request, CancellationToken ct)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == request.IdNo, ct);
        if (student == null)
        { return false; }

        student.Name = request.Name;
        student.GPA = request.Gpa;
        _context.Entry(student)
            .Property("LastUpdated")
            .CurrentValue = DateTime.UtcNow;
        _context.Entry(student)
    .Property(s => s.Version)
    .OriginalValue = request.Version;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<StudentResponseDto?>> ShowDeletedAsync()
    {
        var students = await _context.Students.IgnoreQueryFilters().Where(s => s.IsDeleted == true).Select(s => new StudentResponseDto(s.Id, s.RegistrationNumber, s.Name, s.GPA)).ToListAsync();
        return students;
    }

    public Task<bool> CodeExistsAsync(string registrationNumber, CancellationToken ct) =>
        _context.Students.AsNoTracking().AnyAsync(s => s.RegistrationNumber == registrationNumber, ct);


}