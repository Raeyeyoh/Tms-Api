using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Courses.Commands;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;
namespace TmsApi.Infrastructure.Services;


public class CachedCourseService(
HybridCache cache,
ICourseService repo,
ILogger<CachedCourseService> logger) : ICachedCourseService
{
    public async Task<CourseResponseDto> GetCourseAsync(string code, CancellationToken ct)
    {
        var key = CacheKeys.Course(code);
        var dbHit = false;
        var dto = await cache.GetOrCreateAsync(key, (repo, code),
        async (state, token) =>
        {
            dbHit = true;
            logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
            var course = await state.repo.GetByCodeAsync(state.code, token) ?? throw new KeyNotFoundException($"Course {state.code} not found.");
            return new CourseResponseDto(
            course.Id, course.Title, course.Code,
            course.MaxCapacity, course.Enrollments.Count);
        },
        tags: [CacheKeys.CoursesTag],
          cancellationToken: ct);
        if (!dbHit)
            logger.LogInformation("Cache HIT for {Key}", key);
        return dto;
    }
    public async Task<List<CourseResponseDto>> GetAllCoursesAsync(CancellationToken ct)
    {
        var key = CacheKeys.CoursesAll;
        var dbHit = false;
        var list = await cache.GetOrCreateAsync(key, repo,
        async (state, token) =>
        {
            dbHit = true;
            logger.LogInformation("Cache MISS for {Key} fetching from DB", key);

            var courses = await state.GetCourses1Async(token);
            return courses.Select(c => new CourseResponseDto(
            c.Id, c.Title, c.Code,
            c.MaxCapacity, c.EnrollmentCount)).ToList();
        },
    tags: [CacheKeys.CoursesTag], cancellationToken: ct);
        if (!dbHit)
            logger.LogInformation("Cache HIT for {Key}", key);
        return list;
    }
    public async Task InvalidateCourseCacheAsync(CancellationToken ct)
    {
        logger.LogInformation("Invalidating cache tag {Tag}", CacheKeys.
        CoursesTag);
        await cache.RemoveByTagAsync(CacheKeys.CoursesTag, ct);
    }

    // public async Task<bool> UpdateAsync(CourseUpdateDto dto, CancellationToken ct)
    // {
    //     var course = await context.Courses
    //         .FirstOrDefaultAsync(c => c.Id == dto.Id, ct);

    //     if (course == null)
    //         return false;

    //     course.Title = dto.Title;
    //     course.Code = dto.Code;
    //     course.MaxCapacity = dto.MaxCapacity;

    //     await context.SaveChangesAsync(ct);

    //     return true;
    // }
}


