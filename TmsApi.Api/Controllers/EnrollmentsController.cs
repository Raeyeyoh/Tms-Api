using Microsoft.AspNetCore.Mvc;

namespace TmsApi.Api.Controllers;

using Application.Dtos;

using Application.Interfaces;
[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(ICourseService courseService, IEnrollmentService enrollmentService) : ControllerBase
{



    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one enrolment for a course")]
    public async Task<IActionResult> GetEnrollment(int courseId, int id,
    CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetByIdAsync(courseId, id, ct);
        return enrollment is not null ? Ok(enrollment) : NotFound();
    }


    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>),
    StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List enrolments for a course")]
    public async Task<IActionResult> GetEnrollments(int courseId,
        CancellationToken ct)
    {
        if (!(await courseService.GetByIdAsync(courseId, ct) is null))
        {
            var enrollment = await enrollmentService.GetEnrollmentByIdAsync(courseId,
                 ct);
            for (int i = 0; i < enrollment.Count; i++)
            {
                return enrollment is not null ? Ok(enrollment) : NotFound();
            }
        }
        return NotFound();
    }



    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Enrol a student in a course")]
    [EndpointDescription("Returns 404 if the course does not exist, 409 if the course has reached MaxCapacity.")]
    public async Task<IActionResult> EnrollStudent(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {

        var course = await courseService.GetByIdAsync(courseId, ct);


        if (course is null)
        {
            return NotFound();
        }

        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Detail = $"The course with id '{courseId}' has reached its maximum capacity.",
                Status = StatusCodes.Status409Conflict
            });

        }
        var enrollment = await enrollmentService.CreateAsync(courseId, request, ct);

        return CreatedAtAction(nameof(GetEnrollments), new { courseId = enrollment.Id }, enrollment);
        //throw new NotImplementedException();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]

    [EndpointSummary("Delete an enrollment by course id and ID")]
    [EndpointDescription("Deletes an enrollment by its ID. Returns 404 if the enrollment is not found.")]
    public async Task<IActionResult> Delete([FromQuery] int id, int eid, CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetByIdAsync(id, eid, ct);
        if (enrollment is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Enrollment not found",
                Detail = $"No enrollment with ID '{eid}' and course ID '{id}' was found.",
                Status = StatusCodes.Status404NotFound
            });

        }
        await enrollmentService.DeleteAsync(eid);
        return NoContent();
    }
    // [HttpPut]
    // public async Task<bool> updatebulk(CancellationToken ct)
    // {
    //     var enrollment = await enrollmentService.UpdateBulk(ct);
    //     return enrollment;
    // }
}