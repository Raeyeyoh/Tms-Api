using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/courses")]
public class CourseController(ICourseService courseService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await courseService.GetAllAsync();
        return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await courseService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> AddCourse([FromBody] CreateAddCourseRequest request)
    {
        var course = await courseService.AddCourseAsync(request.courseCode, request.title, request.capacity, request.enrolledCount);
        return CreatedAtAction(nameof(GetById), new { id = course.id }, course);
    }
    public record CreateAddCourseRequest(string courseCode, string title, int capacity, int enrolledCount);

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string coursecode)
    {
        var deleted = await courseService.DeleteAsync(coursecode);
        return deleted ? NoContent() : NotFound();
    }

}