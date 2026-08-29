using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
//[Authorize(Roles = "Instructor,Admin")]
public class CoursesController(ICachedCourseService service, IMediator mediator, TmsDbContext context, IAuthorizationService _authorizationService, ICourseService courseService) : ControllerBase
{



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
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
    {
        var course = await context.Courses.FindAsync(id);
        if (course == null) return NotFound();
        var authResult = await
        _authorizationService.AuthorizeAsync(User, course, "CanEditCourse");
        if (!authResult.Succeeded)
        {
            return Forbid();
        }
        course.Title = dto.Title;
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]

    [EndpointSummary("Delete a course by ID")]
    [EndpointDescription("Deletes a course by its ID. Returns 404 if the course is not found.")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        if (course is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Course not found",
                Detail = $"No course with ID '{id}' was found.",
                Status = StatusCodes.Status404NotFound
            });

        }
        await courseService.DeleteAsync(id);

        return NoContent();

    }
    [HttpPost]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {

        var course = courseService.CodeExistsAsync(request.Code, ct);
        if (course.Result)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict

            });
        }

        var result = await courseService.CreateAsync(request, ct);
        return Ok(result);
    }
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
// public async Task<IActionResult> Update(
// int id,
// CourseUpdateDto course,
// CancellationToken ct)
// {
//     if (id != course.Id)
//         return BadRequest("Route id does not match body id.");

//     var updated = await mediator.Send(new UpdateCourseCommand(course), ct);

//     if (!updated)
//         return NotFound();

//     return NoContent();
// }
// public async Task<IActionResult> GetCourses(
//         CancellationToken ct)
// {

//     var result = await service.GetAllCoursesAsync(ct);
//     return Ok(result);
// }