using MechanicShop.Domain.Emloyees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> employee)
    {
        employee.HasKey(e => e.Id).IsClustered(false);

        employee.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        employee.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(50);

        employee.Property(e => e.Role)
            .HasConversion<string>()
            .IsRequired();
    }
}



