using System.ComponentModel.DataAnnotations;
namespace TmsApi.Dtos;

public record CreateStudentDto
{
    [Required, RegularExpression(@"^[A-Z]{3}-\d{3}$",
    ErrorMessage = "Code must follow the pattern XXX-000 (e.g., CSE-101).")]
    public required string RegistrationNumber { get; set; }
    [Required, MaxLength(100)]
    public required string Name { get; set; }
    [Range(0, 4, ErrorMessage = "GPA must be between 0 and 4.")]
    public decimal GPA { get; set; }
}