using MechanicShop.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> customer)
    {
        customer.HasKey(c => c.Id)
            .IsClustered(false);

        customer.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);
   
        customer.Property(c => c.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        customer.Property(c => c.Email)
            .HasMaxLength(150);

        customer.HasMany(c => c.Vehicles)
            .WithOne()
            .HasForeignKey(v => v.CustomerId);

        customer.Navigation(c => c.Vehicles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

    }
}



