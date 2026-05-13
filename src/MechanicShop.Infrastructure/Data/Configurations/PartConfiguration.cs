using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> part)
    {
        part.HasKey(p => p.Id).IsClustered(false);

        part.Property(rt => rt.Id).ValueGeneratedNever();

        part.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        part.Property(p => p.Cost)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        part.Property(p => p.Quantity)
            .IsRequired();
    }
}



