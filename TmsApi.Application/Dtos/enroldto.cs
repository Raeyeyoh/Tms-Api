namespace TmsApi.Application.Dtos;

public record EnrollmentResponseDto1(
    int Id,
    int CourseId,
    int StudentId,
    string StudentName,
    string CourseName,
    string Status,
    DateTime EnrolledAt
);