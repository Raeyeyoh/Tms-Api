using TmsApi.Application.Courses.Commands;
using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces;

public interface ICachedCourseService
{
    public Task<CourseResponseDto> GetCourseAsync(string code, CancellationToken ct);
    public Task<List<CourseResponseDto>> GetAllCoursesAsync(CancellationToken ct);
    //  public Task<bool> UpdateAsync(CourseUpdateDto course, CancellationToken ct);
    public Task InvalidateCourseCacheAsync(CancellationToken ct);

}
