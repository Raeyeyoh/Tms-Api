
namespace TmsApi.Application.Interfaces;

using Dtos;
using Domain.Entities;

public interface ICourseService
{
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest course, CancellationToken ct);
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<Course?> GetByCodeAsync(string code, CancellationToken ct);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct);

    Task<bool> DeleteAsync(int id);
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest
        request, CancellationToken ct);
    Task<List<CourseResponseDto>> GetCourses1Async(CancellationToken ct);
    Task<bool> UpdateAsync(CourseUpdateDto course, CancellationToken ct);


}