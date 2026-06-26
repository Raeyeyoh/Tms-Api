public class EnrollmentWorker(IServiceScopeFactory scopeFactory)
{
    public async void processbatch()
    {
        using var scope = scopeFactory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
        svc.EnrollAsync(1, 1).Wait();
    }
}