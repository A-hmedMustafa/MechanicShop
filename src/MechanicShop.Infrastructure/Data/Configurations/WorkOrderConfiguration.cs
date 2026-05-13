using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> workOrder)
    {
        workOrder.HasKey(w => w.Id).IsClustered(false);

        workOrder.Property(w => w.LaborId)
               .IsRequired();

        workOrder.HasOne(w => w.Labor)
            .WithMany()
            .HasForeignKey(w => w.LaborId)
            .IsRequired();

        workOrder.HasOne(i => i.Invoice)
            .WithOne(w => w.WorkOrder)
            .HasForeignKey<Invoice>(i => i.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        workOrder.Property(w => w.State)
            .HasConversion<string>()
            .IsRequired();

        workOrder.Property(w => w.StartsAtUtc).IsRequired();

        workOrder.Property(w => w.EndsAtUtc).IsRequired();

        workOrder.Property(w => w.Tax).HasPrecision(18, 2);

        workOrder.Property(w => w.Discount).HasPrecision(18, 2);

        workOrder.Ignore(w => w.Total);

        workOrder.Ignore(w => w.TotalLaborCost);

        workOrder.Ignore(w => w.TotalPartsCost);

        workOrder.HasMany(w => w.RepairTasks)
            .WithMany()
            .UsingEntity(junc => junc.ToTable("WorkOrderRepairTask"));

        workOrder.HasOne(w => w.Vehicle)
            .WithMany()
            .HasForeignKey(w => w.VehicleId);

        workOrder.HasIndex(w => w.LaborId);

        workOrder.HasIndex(w => w.VehicleId);

        workOrder.HasIndex(w => w.State);

        workOrder.HasIndex(a => new { a.StartsAtUtc, a.EndsAtUtc });

        workOrder.Property(w => w.Spot)
            .HasConversion<string>()
            .IsRequired();
    }
}



