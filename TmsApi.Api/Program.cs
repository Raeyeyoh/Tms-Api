using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;
//using Microsoft.AspNetCore.Antiforgery;

using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Services;
using TmsApi.Api.Filters;
using TmsApi.Api.Middleware;
using TmsApi.Api.Options;
using TmsApi.Infrastructure.Persistence.Data;
using TmsApi.Application.Enrollments.Commands;
using FluentValidation;
using MediatR;
using TmsApi.Application.Behaviors;
using TmsApi.Api.ExceptionHandlers;
using Microsoft.Extensions.Caching.Hybrid;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Api.RateLimiting;
using TmsApi.Infrastructure.Transcripts;
using System.Threading.Channels;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Workers;
using TmsApi.Api.Hubs;
using TmsApi.Application.Notifications;
using TmsApi.Api.Notifications;
using Microsoft.AspNetCore.Antiforgery;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TmsDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")).LogTo(Console.WriteLine, LogLevel.Information)
.EnableSensitiveDataLogging());
var cs = builder.Configuration.GetConnectionString("TmsDatabase");
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});
builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ITranscriptNotificationService, SignalRTranscriptNotificationService>();
Console.WriteLine($"Connection string: {cs}");
builder.Services.AddControllers();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});
// Production-only leave commented in lab
// builder.Services.AddStackExchangeRedisCache(options =>
// {options.Configuration = builder.Configuration.GetConnectionStrin

//g("Redis");

//options.InstanceName = "tms:";

// });
// builder.Services.AddHybridCache();
builder.Services.AddSingleton(Channel.CreateBounded<TranscriptRequest>(
new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait
}));
builder.Services.AddHostedService<TranscriptWorker>();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext,
    string>(httpContext =>
    {
        var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);
        return tier switch
        {
            ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter
            (partitionKey: $"paid:{partitionKey}",
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 200,
                TokensPerPeriod = 100,
                ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }),
            ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter
            (
            partitionKey: $"free:{partitionKey}",
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 30,
                TokensPerPeriod = 10,
                ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }),
            _ => RateLimitPartition.GetTokenBucketLimiter(
        partitionKey: $"anon:{partitionKey}",
        factory: _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 10,
            TokensPerPeriod = 5,
            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
            QueueLimit = 0,
            AutoReplenishment = true
        })
        };
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;


    options.OnRejected = async (context, ct) =>
    {
        var retryAfter = "10";
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ts))
            retryAfter = ((int)ts.TotalSeconds).ToString();
        context.HttpContext.Response.Headers.RetryAfter = retryAfter;
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Rate limit exceeded",
            Detail = $"Too many requests. Retry after {retryAfter} seconds.",
            Status = StatusCodes.Status429TooManyRequests,
            Type = "https://tms.local/errors/rate_limit_exceeded"
        }, ct);


    };
});
builder.Services.AddAuthentication("Training").AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
//builder.Services.AddExceptionHandler();
//builder.Services.AddScoped<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddOptions<PaymentOptions>().BindConfiguration("Payments")
.ValidateDataAnnotations()
.ValidateOnStart();
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});



builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
    description.GroupName == "v1";
});
builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description =>
    description.GroupName == "v2";
});
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
new UrlSegmentApiVersionReader(),
new HeaderApiVersionReader("X-Api-Version"));

})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddMediatR(cfg =>
cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(EnrollStudentValidator).Assembly);
// LoggingBehavior FIRST—it must wrap ValidationBehavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddRateLimiter(options =>
{
    // ... GlobalLimiter from Step 2 stays as-is ...
    options.AddConcurrencyLimiter("transcripts", opt =>
    {
        opt.PermitLimit = 5;

        opt.QueueLimit = 20;
        // 5 in-flight transcripts maximum
        // queue up to 20 more
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});
var allowedOrigins = builder.Configuration
.GetSection("AllowedOrigins").Get<string[]>()
?? ["http://localhost:4200"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});
builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscriptStatusStore>();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.MapHub<TmsHub>("/hubs/tms").RequireCors("TmsClient");

// app.MapHealthChecks("/health/live").DisableRateLimiting();
// app.MapHealthChecks("/health/ready").DisableRateLimiting();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("TmsClient");

app.UseRateLimiter();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true || context.
    Request.Cookies.ContainsKey("tms_auth"))
    {
        var antiforgery = context.RequestServices
        .GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
        new CookieOptions
        {
            HttpOnly = false,
            Secure = !builder.Environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict
        });
    }
    await next(context);
});
app.UseMiddleware<V1DeprecationMiddleware>();
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
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("TMS API Reference")
        .WithTheme(ScalarTheme.DeepSpace)
        .WithDefaultHttpClient(ScalarTarget.CSharp,
        ScalarClient.HttpClient);
        options
        .AddDocument("v1", "API Version 1.0")
        .AddDocument("v2", "API Version 2.0");
    });

    Console.WriteLine("Running in development mode");
}
if (app.Environment.IsProduction())
{
    Console.WriteLine("Running in production mode");
    app.UseExceptionHandler();
}
//app.UseRateLimiter(4);
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
//     context.Database.Migrate();
//     if (!context.Students.Any())
//     {
//         var students = new List<Student>
// {
// new() { RegistrationNumber = "TMS-2026-0001", Name = "AliceSmith", GPA = 3.8m, IsActive = true },
// new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones", GPA = 2.9m, IsActive = true },
// new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
// new() { RegistrationNumber = "TMS-2026-0004", Name = "DianaPrince", GPA = 3.9m, IsActive = true },
// new() { RegistrationNumber = "TMS-2026-0005", Name = "EvanWright", GPA = 2.5m, IsActive = true }
// };
//         context.Students.AddRange(students);
//         var courses = new List<Course>
// {
// new() { Code = "CS-101", Title = "Introduction to ComputerScience", MaxCapacity = 30 },
// new() { Code = "CS-201", Title = "Data Structures and Algorithms", MaxCapacity = 25 },
// new() { Code = "MAT-101", Title = "Calculus I", MaxCapacity =40 }
// };
//         context.Courses.AddRange(courses);
//         context.SaveChanges();
//         var enrollments = new List<Enrollment>
// {
// new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
// new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
// new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
// new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
// };
//         context.Enrollments.AddRange(enrollments);
//         context.SaveChanges();
//     }
// }


app.Run();

