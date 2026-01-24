using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SigepDomain.Entities;

namespace SigepInfrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.IdentificationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.IdentificationNumber)
            .IsUnique();

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.BaseSalary)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.HasOne(e => e.Position)
            .WithMany(p => p.Employees)
            .HasForeignKey(e => e.PositionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Schedule)
            .WithMany(s => s.Employees)
            .HasForeignKey(e => e.ScheduleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
