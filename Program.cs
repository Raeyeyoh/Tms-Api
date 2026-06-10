using Microsoft.AspNetCore.Authentication.JwtBearer;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

 builder.Services.AddControllers();
builder.Services.AddAuthentication("Training").AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
//builder.Services.AddExceptionHandler();




// app.MapControllers();

var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>();
//app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
courseCode = "CS-101",
studentId = "S-001",
letterGrade = "A"
})).RequireAuthorization();

app.Run();
