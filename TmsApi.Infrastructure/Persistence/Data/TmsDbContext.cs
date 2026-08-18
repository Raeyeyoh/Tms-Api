using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Data;

public class TmsDbContext(DbContextOptions<TmsDbContext> options) : DbContext(options)
{

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(
          typeof(TmsDbContext).Assembly);
    }

}