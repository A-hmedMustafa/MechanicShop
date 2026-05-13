using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common;
using MechanicShop.Tests.Common.Billing;
using Xunit;

namespace MechanicShop.Domain.UnitTests.Billing;

public class InvoiceTests
{
    [Fact]
    public void CreateInvoice_WithValidData_ShouldSucceed()
    {
        var id = Guid.NewGuid();
        var workOrderId = Guid.NewGuid();
        var items = new List<InvoiceLineItem>
        {
            InvoiceLineItem.Create(Guid.NewGuid(), 1, "Oil Change", 2, 50).Value
        };

        var time = new FakeTimeProvider();
        time.SetUtcNow(DateTimeOffset.Parse("2024-01-01T00:00:00Z"));

        var invoiceCreationResult = InvoiceFactory.CreateInvoice(
            id: id, 
            workOrderId: workOrderId, 
            items: items,
            discountAmount: 10, 
            taxAmount: 5, 
            timeProvider: time);

        var newInvoice = invoiceCreationResult.Value;
    
        Assert.True(invoiceCreationResult.IsSuccess);
        Assert.Equal(id, newInvoice.Id);
        Assert.Equal(workOrderId, newInvoice.WorkOrderId);
        Assert.Equal(InvoiceStatus.Unpaid, newInvoice.Status);
        Assert.Equal(10, newInvoice.DiscountAmount);
        Assert.Equal(5, newInvoice.TaxAmount);
        Assert.Equal(100, newInvoice.SubTotal);
        Assert.Equal(95, newInvoice.Total);
        Assert.Equal(time.GetUtcNow(), newInvoice.IssuedAtUtc);
    }
    
    [Fact]
    public void CreateInvoice_WithEmptyItems_ShouldFail()
    {
        List<InvoiceLineItem> items = [];
        var result = InvoiceFactory.CreateInvoice(items: items);
        Assert.True(result.IsError);
        Assert.Equal(InvoiceErrors.LineItemsEmpty.Code, result.TopError.Code);
    }

    [Fact]
    public void ApplyDiscount_WhenUnpaid_ShouldUpdateDiscount()
    {
        const int discount = 15;
        var invoice = InvoiceFactory.CreateInvoice().Value;
        var totalWithoutDiscount = invoice.Total;
        var applyDiscountResult = invoice.ApplyDiscount(discount);

        Assert.True(applyDiscountResult.IsSuccess);
        Assert.Equal(discount, invoice.DiscountAmount);
        Assert.NotEqual(totalWithoutDiscount, invoice.Total); 
    }
    [Fact]
    public void ApplyDiscount_WithNegativeAmount_ShouldFail()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;
        var applyDiscountResult = invoice.ApplyDiscount(-10);
        Assert.True(applyDiscountResult.IsError);
        Assert.Equal(InvoiceErrors.DiscountNegative.Code, applyDiscountResult.TopError.Code);
    }

    [Fact]
    public void ApplyDiscount_GreaterThanSubtotal_ShouldFail()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;
        var excessiveDiscount = invoice.SubTotal + 1;
        var applyDiscountResult = invoice.ApplyDiscount(excessiveDiscount);
       
        Assert.True(applyDiscountResult.IsError);
        Assert.Equal(InvoiceErrors.DiscountExceedsSubtotal.Code, applyDiscountResult.TopError.Code);
    }

    [Fact]
    public void ApplyDiscount_ValidAmount_ShouldSucceed()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;
        const decimal validDiscount = 20m;
        var applyDiscountResult = invoice.ApplyDiscount(validDiscount);
        
        Assert.True(applyDiscountResult.IsSuccess);
        Assert.Equal(validDiscount, invoice.DiscountAmount);
    }

    [Fact]
    public void ApplyDiscount_WhenPaid_ShouldFail()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;
        Assert.True(invoice.MarkAsPaid(TimeProvider.System).IsSuccess);
      
        var applyDiscountResult = invoice.ApplyDiscount(10);
      
        Assert.True(applyDiscountResult.IsError);
        Assert.Equal(InvoiceErrors.InvoiceLocked.Code, applyDiscountResult.TopError.Code);
    }

    [Fact]
    public void MarkAsPaid_WhenUnpaid_ShouldSucceed()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;
        var time = new FakeTimeProvider();
        time.SetUtcNow(DateTimeOffset.Parse("2024-01-01T00:00:00Z"));
        var markAsPaidResult = invoice.MarkAsPaid(time);
        Assert.True(markAsPaidResult.IsSuccess);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(time.GetUtcNow(), invoice.PaidAtUtc);
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyPaid_ShouldFail()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;
        Assert.True(invoice.MarkAsPaid(TimeProvider.System).IsSuccess);
        var markAsPaidResult = invoice.MarkAsPaid(TimeProvider.System);
        Assert.True(markAsPaidResult.IsError);
        Assert.Equal(InvoiceErrors.InvoiceLocked.Code, markAsPaidResult.TopError.Code);
    }
}