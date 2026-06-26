using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var enrollments = await enrollmentService.GetAllAsync();
        return Ok(enrollments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var enrollment = await enrollmentService.GetByIdAsync(id);
        return enrollment is not null ? Ok(enrollment) : NotFound();
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        var enrollment = await enrollmentService.EnrollAsync(request.StudentId, request.CourseId);
        return CreatedAtAction(nameof(GetById), new { id = enrollment.Id }, enrollment);
    }
    public record CreateEnrollmentRequest(int StudentId, int CourseId);

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await enrollmentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
    [HttpPut]
    public async Task<bool> updatebulk(CancellationToken ct)
    {
        var enrollment = await enrollmentService.UpdateBulk(ct);
        return enrollment;
    }
}