using MechanicShop.Domain.Customers.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> vehicle)
    {
        vehicle.HasKey(v => v.Id).IsClustered(false);

        vehicle.Property(v => v.Id).ValueGeneratedNever();

        vehicle.Property(v => v.Make)
               .IsRequired()
               .HasMaxLength(100);

        vehicle.Property(v => v.Model)
               .IsRequired()
               .HasMaxLength(100);

        vehicle.HasOne(v => v.Customer)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CustomerId);

        vehicle.Property(v => v.Year).IsRequired();

        vehicle.Property(v => v.LicensePlate).IsRequired();
    }
}



