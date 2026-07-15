using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/students")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class StudentController(IStudentService studentservice) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStudents([FromQuery] PagedRequest request, CancellationToken ct)
    {

        var students = await studentservice.GetStudentsAsync(request, ct);
        return Ok(students);
    }

    [HttpGet("{id:int}", Name = nameof(GetStudentById))]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentById(int id, CancellationToken ct)
    {

        var student = await studentservice.GetByIdAsync(id, ct);
        return student is not null ? Ok(student) : NotFound(new ProblemDetails
        {
            Title = "Student not found",
            Detail = $"No student with ID {id} was found.",
            Status = StatusCodes.Status404NotFound
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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


    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdatestudentRequest req, CancellationToken ct)
    {
        var student = await studentservice.UpdateStudentAsync(req, ct);
        return Ok(student);
    }

    [ProducesResponseType(typeof(IEnumerable<StudentResponseDto>), StatusCodes.Status200OK)]
    [HttpGet("deleted")]
    public async Task<IActionResult> ShowDeleted()
    {
        var stu = await studentservice.ShowDeletedAsync();
        return Ok(stu);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [EndpointSummary("Soft delete a student by ID")]
    [EndpointDescription("Marks a student as deleted by its ID. Returns 404 if the student is not found.")]
    public async Task<IActionResult?> SoftDelete(int id, CancellationToken ct)
    {
        var student = await studentservice.GetByIdAsync(id, ct);
        if (student is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Student not found",
                Detail = $"No student with ID {id} was found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        await studentservice.SoftDelteAsync(id, ct);
        return NoContent();
    }

}