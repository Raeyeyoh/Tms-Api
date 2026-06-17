using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.OpenApi;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAuthentication("Training").AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
//builder.Services.AddExceptionHandler();
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();
builder.Services.AddSingleton<IStudentService, StudentService>();
builder.Services.AddSingleton<ICourseService, CourseService>();

builder.Services.AddOptions<PaymentOptions>().BindConfiguration("Payments")
.ValidateDataAnnotations()
.ValidateOnStart();
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();


var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();

//app.UseHttpsRedirection();
app.UseRouting();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});
// app.MapGet("/api/assessments/results", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001",
//     letterGrade = "A"
// })).RequireAuthorization();

// app.MapPost("/api/enrollments", async (string studentId, string courseCode, IEnrollmentService svc) =>
// {var record = await svc.EnrollAsync(studentId, courseCode);
// });
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    Console.WriteLine("Running in development mode");
}
if (app.Environment.IsProduction())
{
    Console.WriteLine("Running in production mode");
    app.UseExceptionHandler();
}
//app.UseRateLimiter(4);

app.Run();

