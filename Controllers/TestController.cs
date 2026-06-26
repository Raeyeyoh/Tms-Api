using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using TmsApi.Data;
using Microsoft.EntityFrameworkCore;
namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building the query object (nodatabase contact)...");
        var query = context.Students.Where(s => s.GPA >= 3.0m);
        Console.WriteLine(">>> STEP 2: Appending a sorting clause...");
        var orderedQuery = query.OrderBy(s => s.Name);
        Console.WriteLine(">>> STEP 3: Materializing query into a C# List...");
        var results = orderedQuery.ToList();
        Console.WriteLine(">>> STEP 4: Materialization finished. List populated.\n");
        return Ok(results);
    }
    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }
    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> STEP 1: Running non-translatable query...");
        try
        {
            var students = context.Students
            .Where(s => IsHonorRoll(s.GPA))
.ToList();
            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");
            return BadRequest(new { Message = ex.Message });
        }
    }
    [HttpGet("getallstud")]
    public async Task<IActionResult> getallstud(int page_number = 1)
    {
        int pagesize = 20;
        CancellationToken cancellationToken = default;
        var stud = await context.Students.OrderBy(s => s.Name).Skip((page_number - 1) * pagesize).Take(pagesize).ToListAsync(cancellationToken);

        return Ok(stud);
    }
    public async Task<IActionResult> courbyenrollment()
    {
        var result = await context.Enrollments
    .GroupBy(e => new { e.CourseId, e.Course.Title })
    .Select(g => new
    {
        g.Key.Title,
        EnrollmentCount = g.Count()
    })
    .OrderByDescending(x => x.EnrollmentCount)
    .Take(5)
    .ToListAsync();
        return Ok(result);
    }
    [HttpGet("active")]
    public async Task<IActionResult> ACTIVESTUD()
    {
        var count = await context.Students.Where(s => s.IsActive && s.GPA >= 3.0m).CountAsync();
        return Ok(count);


    }
    [HttpGet("mostenrollment")]

    public async Task<IActionResult> mostenrollment()
    {
        var list = await context.Courses.Select(c => new
        {
            c.Title,
            EnrollmentCount = c.Enrollments.Count
        })
.OrderByDescending(x => x.EnrollmentCount)
.ToListAsync();
        return Ok(list);
    }
    [HttpGet("AvGpaPerCourse")]

    public async Task<IActionResult> AvGpaPerCourse()
    {
        var list = await context.Enrollments
.GroupBy(e => e.Course.Title)
.Select(g => new
{
    Course = g.Key,
    AverageGPA = g.Average(e => e.Student.GPA)
})
.ToListAsync();
        return Ok(list);
    }
    [HttpGet("StudWith0Enrollment")]

    public async Task<IActionResult> StudWith0Enrollment()
    {
        var list = await context.Students
.Where(s => !s.Enrollments.Any())
.Select(s => s.Name)
.ToListAsync();
        return Ok(list);

    }
    [HttpGet("StudWith0Enrollment1")]

    public async Task<IActionResult> StudWith0Enrollment1()
    {
        var list = await context.Students
      .LeftJoin(context.Enrollments,
      s => s.Id,
      e => e.StudentId,
      (s, e) => new { s, e })
      .Where(x => x.e == null)
      .Select(x => x.s.Name)
      .ToListAsync();
        return Ok(list);
    }
    [HttpGet("Stud")]

    public async void Stud()
    {
        CancellationToken cancellationToken = default;

        var students = await context.Students.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var s in students)
        {

            var count = await context.Enrollments
            .AsNoTracking()
            .CountAsync(e => e.StudentId == s.Id, cancellationToken);
            Console.WriteLine($"{s.Name}: {count} enrollments");
        }
    }

    [HttpGet("Stud1")]

    public async void Stud1()
    {
        CancellationToken cancellationToken = default;

        var report = await context.Students
.AsNoTracking()
.Select(s => new
{
    s.Name,
    EnrollmentCount = s.Enrollments.Count
})
.ToListAsync(cancellationToken);
        foreach (var r in report)
            Console.WriteLine($"{r.Name}: {r.EnrollmentCount} enrollments");
    }
    //Func<int, bool> sth = t => t > 10;
    [HttpGet("Stud2")]

    public async void Stud2()
    {
        CancellationToken cancellationToken = default;

        var students = await context.Students
.AsNoTracking()
.Include(s => s.Enrollments)
.ToListAsync(cancellationToken);
        foreach (var s in students)
            Console.WriteLine($"{s.Name}: {s.Enrollments.Count} enrollments");
    }
}