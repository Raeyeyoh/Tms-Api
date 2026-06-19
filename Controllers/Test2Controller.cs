using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TmsApi.Data;
using TmsApi.Entities;
namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]

public class Test2Controller(TmsDbContext context) : ControllerBase
{
    [HttpGet("active")]
    public async Task<IActionResult> ACTIVESTUD()
    {
        var count = await context.Students.Where(s => s.IsActive && s.GPA >= 3.0m).CountAsync();
        return Ok(count);


    }
    [HttpGet("actmostenrollmentive")]

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

    //Func<int, bool> sth = t => t > 10;
}
