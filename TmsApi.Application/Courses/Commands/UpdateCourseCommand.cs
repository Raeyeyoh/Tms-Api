using MediatR;
using TmsApi.Application.Dtos;

namespace TmsApi.Application.Courses.Commands;

public record UpdateCourseCommand(CourseUpdateDto course) : IRequest<bool>;