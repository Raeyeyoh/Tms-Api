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
    public async Task<IActionResult> GetById(int id)
    {
        var course = await courseService.GetByIdAsync(id);
        return course is not null ? Ok(course) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> AddCourse([FromBody] CreateAddCourseRequest request)
    {
        var course = await courseService.AddCourseAsync(request.courseCode, request.title, request.capacity, request.enrolledCount);
        return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
    }
    public record CreateAddCourseRequest(string courseCode, string title, int capacity, int enrolledCount);

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await courseService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

}