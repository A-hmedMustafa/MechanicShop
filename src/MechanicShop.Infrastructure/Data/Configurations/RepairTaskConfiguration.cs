using MechanicShop.Domain.RepairTasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class RepairTaskConfiguration : IEntityTypeConfiguration<RepairTask>
{
    public void Configure(EntityTypeBuilder<RepairTask> repairTask)
    {
        repairTask.HasKey(rt => rt.Id).IsClustered(false);

        repairTask.Property(rt => rt.Id).ValueGeneratedNever();

        repairTask.Property(rt => rt.Name)
            .IsRequired()
            .HasMaxLength(100);

        repairTask.Property(rt => rt.EstimatedDurationInMins)
            .HasConversion<string>()
            .IsRequired();
    
        repairTask.Property(rt => rt.LaborCost)
               .IsRequired()
               .HasPrecision(18, 2);

        repairTask.HasMany(c => c.Parts)
           .WithOne()
           .HasForeignKey("RepairTaskId")
           .OnDelete(DeleteBehavior.Cascade);

        repairTask.Navigation(c => c.Parts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}



