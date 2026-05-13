using MechanicShop.Domain.WorkOrders.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> invoice)
    {
        invoice.ToTable("Invoices");

        invoice.HasKey(e => e.Id).IsClustered(false);

        invoice.Property(inv => inv.Id).ValueGeneratedNever();

        invoice.Property(inv => inv.IssuedAtUtc).IsRequired();

        invoice.Property(inv => inv.PaidAtUtc);

        invoice.Property(inv => inv.DiscountAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        invoice.Property(inv => inv.TaxAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        invoice.Property(inv => inv.PaidAtUtc);

        invoice.Property(inv => inv.Status)
            .HasConversion<string>()
            .IsRequired();

        invoice.Navigation(inv => inv.LineItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        invoice.OwnsMany(inv => inv.LineItems, lines =>
        {
            lines.ToTable("InvoiceLineItems");

            lines.WithOwner()
                .HasForeignKey(line => line.InvoiceId);
            
            lines.Property(line => line.LineNumber).ValueGeneratedNever();
            lines.HasKey(line => new {line.LineNumber, line.InvoiceId});

            lines.Property(line => line.Description)
                .HasMaxLength(200)
                .IsRequired();

            lines.Property(line => line.Quantity)
                .IsRequired();

            lines.Property(i => i.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();       
        });
    }


}



