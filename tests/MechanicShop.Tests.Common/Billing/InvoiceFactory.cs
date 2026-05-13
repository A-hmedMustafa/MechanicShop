using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Tests.Common.Billing;

public static class InvoiceFactory
{
    public static Result<Invoice> CreateInvoice(
        Guid? id = null,
        Guid? workOrderId = null,
        List<InvoiceLineItem>? items = null,
        decimal? discountAmount = null,
        decimal? taxAmount = null,
        TimeProvider? timeProvider = null
    )
    {
        return Invoice.Create(
            id ?? Guid.NewGuid(),
            workOrderId ?? Guid.NewGuid(),
            discountAmount ?? 0,
            taxAmount ?? 0,
            items ?? [InvoiceLineItem.Create(Guid.NewGuid(), 1, "Oil Change", 3, 50).Value],
            timeProvider ?? TimeProvider.System
        );
    }
}