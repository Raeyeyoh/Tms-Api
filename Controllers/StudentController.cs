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
    public async Task<IActionResult> GetById(int id)
    {
        var record = await studentservice.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> AddStudentAsync([FromBody] CreatestudentRequest request)
    {
        var student = await studentservice.AddStudentAsync(request.IdNo, request.regno, request.name, request.age, request.gpa);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }
    public record CreatestudentRequest(int IdNo, string regno, string name, int age, decimal gpa);

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await studentservice.DeleteStudentAsync(id);
        return deleted ? NoContent() : NotFound();
    }
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdatestudentRequest req, CancellationToken ct)
    {
        var student = await studentservice.UpdateStudentAsync(req.IdNo, req.Name, req.Gpa, req.Version, ct);
        return Ok(student);
    }
    public record UpdatestudentRequest(
        int IdNo,
        string Name,
        decimal Gpa,
        uint Version);
    [HttpGet("deleted")]
    public async Task<IActionResult> showdeleted()
    {
        var stu = await studentservice.ShowDeletedAsync();
        return Ok(stu);
    }

}

