using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Courses.Commands;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence.Data;
namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(ICachedCourseService service, IMediator mediator, TmsDbContext context) : ControllerBase
{

    // public async Task<IActionResult> GetCourses(
    //         CancellationToken ct)
    // {

    //     var result = await service.GetAllCoursesAsync(ct);
    //     return Ok(result);
    // }

    [HttpGet]
    public async Task<IActionResult> GetCourses(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var baseQuery = context.Courses.AsNoTracking();
        var totalCount = await baseQuery.CountAsync(ct);
        var rows = await baseQuery
        .OrderBy(c => c.Title)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(c => new
        {
            c.Id,
            c.Title,
            c.Code,
            c.MaxCapacity,
            EnrollmentCount = c.Enrollments.Count
        })
        .ToListAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var hasNext = page < totalPages;
        var hasPrevious = page > 1;


        return Ok(new
        {
            data = rows,
            meta = new
            {
                totalCount,
                page,
                pageSize,
                totalPages,
                hasNext,
                hasPrevious
            },
            links = new
            {
                self = $"/api/v2/courses?page={page}&pageSize={pageSize} ",
                next = hasNext ? $"/api/v2/courses?page={page + 1}&pageSize={pageSize}" : (string?)null,
                prev = hasPrevious ? $" / api / v2 / courses ? page ={page - 1} & pageSize ={pageSize}" : (string?)null,
                enroll = "/api/v2/enrollments"
            }

        });
    }
    //     [HttpGet("search")]
    //     [EnableRateLimiting("search")]
    //     public async Task<IActionResult> SearchCourses(
    // [FromQuery] string? term, CancellationToken ct)
    //     {
    //         var results = await mediator.Send(new SearchCoursesQuery(term), ct);
    //         return Ok(results);
    //     }
    // [HttpPut("{id}")]
    public async Task<IActionResult> Update(
    int id,
    CourseUpdateDto course,
    CancellationToken ct)
    {
        if (id != course.Id)
            return BadRequest("Route id does not match body id.");

        var updated = await mediator.Send(new UpdateCourseCommand(course), ct);

        if (!updated)
            return NotFound();

        return NoContent();
    }
}