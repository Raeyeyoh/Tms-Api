using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> b)
    {
        b.HasKey(s => s.Id);
        b.Property(s => s.Name).IsRequired().HasMaxLength(200);
        b.Property(s => s.RegistrationNumber).IsRequired().HasMaxLength(200);

        b.HasMany(s => s.Enrollments)
.WithOne(s => s.Student)
.HasForeignKey(s => s.StudentId)
.OnDelete(DeleteBehavior.Restrict);
        b.Property<DateTime>("LastUpdated");
        b.Property(s => s.Version).IsRowVersion();
        b.HasQueryFilter(s => !(s.IsDeleted));
    }
}