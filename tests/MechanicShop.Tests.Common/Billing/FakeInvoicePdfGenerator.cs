using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Tests.Common.Billing;

public class FakeInvoicePdfGenerator : IInvoicePdfGenerator
{
    public byte[] Generate(Invoice invoice)
    {
       return [1, 2, 3];
    }
}