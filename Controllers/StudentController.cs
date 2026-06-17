using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/students")]
public class StudentController(IStudentService studentservice) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await studentservice.GetAllStudentsAsync();
        return Ok(students);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await studentservice.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> AddStudentAsync([FromBody] CreatestudentRequest request)
    {
        var student = await studentservice.AddStudentAsync(request.IdNo, request.name, request.age, request.gpa);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }
    public record CreatestudentRequest(string IdNo, string name, int age, decimal gpa);

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await studentservice.DeleteStudentAsync(id);
        return deleted ? NoContent() : NotFound();
    }

}

