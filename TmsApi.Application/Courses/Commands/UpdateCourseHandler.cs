using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
namespace TmsApi.Application.Courses.Commands;

public class UpdateCourseHandler(
    ICourseService repo,
    ICachedCourseService cachedService)
    : IRequestHandler<UpdateCourseCommand, bool>
{
    public async Task<bool> Handle(UpdateCourseCommand command, CancellationToken ct)
    {
        var updated = await repo.UpdateAsync(command.course, ct);

        if (!updated)
            return false;

        await cachedService.InvalidateCourseCacheAsync(ct);

        return true;
    }
}