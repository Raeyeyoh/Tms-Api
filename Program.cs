using Microsoft.AspNetCore.Authentication;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

 builder.Services.AddControllers();
builder.Services.AddAuthentication("Training").AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
//builder.Services.AddExceptionHandler();
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
 builder.Services.AddOptions<PaymentOptions>().BindConfiguration("Payments")
.ValidateDataAnnotations()
.ValidateOnStart();
builder.Host.UseDefaultServiceProvider(options =>
{
options.ValidateScopes = true;
options.ValidateOnBuild = true;
});
// app.MapControllers();
//app.UseExceptionHandler();

var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>();
//app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
courseCode = "CS-101",
studentId = "S-001",
letterGrade = "A"
})).RequireAuthorization();

app.MapPost("/api/enrollments", async (string studentId, string courseCode, IEnrollmentService svc) =>
{var record = await svc.EnrollAsync(studentId, courseCode);
});
app.Run();
