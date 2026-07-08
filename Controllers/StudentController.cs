using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;

[ApiController]
[Route("api/students")]
public class StudentController(IStudentService studentservice) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStudents([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var students = await studentservice.GetStudentsAsync(request, ct);
        return Ok(students);
    }

    [HttpGet("{id:int}", Name = nameof(GetStudentById))]

    public async Task<IActionResult> GetStudentById(int id, CancellationToken ct)
    {
        var student = await studentservice.GetByIdAsync(id, ct);
        return student is not null ? Ok(student) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> AddStudentAsync(RegisterStudentDto request, CancellationToken ct)
    {
        var studentExists = await studentservice.CodeExistsAsync(request.RegistrationNumber, ct);
        if (studentExists)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Student registration number already exists",
                Detail = $"A student with registration number '{request.RegistrationNumber}' is already registered.",
                Status = StatusCodes.Status409Conflict


            });
        }

        var student = await studentservice.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, student);


    }

    [HttpDelete("archive/{id}")]
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
    public async Task<IActionResult> ShowDeleted()
    {
        var stu = await studentservice.ShowDeletedAsync();
        return Ok(stu);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult?> SoftDelete(int id, CancellationToken ct)
    {
        var del = await studentservice.SoftDelteAsync(id, ct);
        return del;
    }

}

