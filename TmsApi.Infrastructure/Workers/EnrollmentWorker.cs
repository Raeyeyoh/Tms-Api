namespace TmsApi.Infrastructure.Workers;

using Microsoft.Extensions.DependencyInjection;
using Application.Dtos;
using Application.Interfaces;
public class EnrollmentWorker(IServiceScopeFactory scopeFactory)
{
    public async void processbatch()
    {
        using var scope = scopeFactory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
        CancellationToken ct = CancellationToken.None;
        EnrollStudentRequest? es = null;
        svc.CreateAsync(1, es, ct).Wait();
    }
}