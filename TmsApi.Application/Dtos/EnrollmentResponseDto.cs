
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Dtos;

public record EnrollmentResponseDto(
int Id,
int CourseId,
int StudentId,
Course Course,
DateTime EnrolledAt);