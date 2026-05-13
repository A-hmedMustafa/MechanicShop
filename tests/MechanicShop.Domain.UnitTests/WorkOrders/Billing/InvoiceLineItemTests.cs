using MechanicShop.Domain.WorkOrders.Billing;
using Xunit;

namespace MechanicShop.Domain.UnitTests.Billing;

public class InvoiceLineItemTests
{
    [Fact]
    public void CreateLineItem_WithValidData_ShouldSucceed()
    {
        var invoiceId = Guid.NewGuid();
        const int lineNumber = 1;
        const string description = "Brake Pad";
        const int quantity = 2;
        const decimal unitPrice = 50m;

        var lineItemCreationResult = InvoiceLineItem.Create(
            invoiceId, lineNumber, description, quantity, unitPrice);

        var item = lineItemCreationResult.Value;
        Assert.True(lineItemCreationResult.IsSuccess);
        Assert.Equal(invoiceId, item.InvoiceId);
        Assert.Equal(lineNumber, item.LineNumber);
        Assert.Equal(description, item.Description);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(unitPrice, item.UnitPrice);
        Assert.Equal(100m, item.LineTotal);
    }

    [Fact]
    public void CreateLineItem_WithEmptyInvoiceId_ShouldFail()
    {
        var lineItemCreationResult = InvoiceLineItem.Create(Guid.Empty, 1, "Item", 1, 10m);

        Assert.True(lineItemCreationResult.IsError);
        Assert.Equal(InvoiceLineItemErrors.InvoiceIdRequired.Code, lineItemCreationResult.TopError.Code);
        Assert.Equal(InvoiceLineItemErrors.InvoiceIdRequired.Description, lineItemCreationResult.TopError.Description);
    }

    [Fact]
    public void CreateLineItem_WithInvalidLineNumber_ShouldFail()
    {
        var lineItemCreationResult = InvoiceLineItem.Create(Guid.NewGuid(), 0, "Item", 1, 10m);

        Assert.True(lineItemCreationResult.IsError);
        Assert.Equal(InvoiceLineItemErrors.LineNumberInvalid.Code, lineItemCreationResult.TopError.Code);
        Assert.Equal(InvoiceLineItemErrors.LineNumberInvalid.Description, lineItemCreationResult.TopError.Description);
    }

    [Fact]
    public void CreateLineItem_WithEmptyDescription_ShouldFail()
    {
        var lineItemCreationResult = InvoiceLineItem.Create(Guid.NewGuid(), 1, " ", 1, 10m);

        Assert.True(lineItemCreationResult.IsError);
        Assert.Equal(InvoiceLineItemErrors.DescriptionRequired.Code, lineItemCreationResult.TopError.Code);
        Assert.Equal(InvoiceLineItemErrors.DescriptionRequired.Description, lineItemCreationResult.TopError.Description);
    }

    [Fact]
    public void CreateLineItem_WithInvalidQuantity_ShouldFail()
    {
        var lineItemCreationResult = InvoiceLineItem.Create(Guid.NewGuid(), 1, "Item", 0, 10m);

        Assert.True(lineItemCreationResult.IsError);
        Assert.Equal(InvoiceLineItemErrors.QuantityInvalid.Code, lineItemCreationResult.TopError.Code);
        Assert.Equal(InvoiceLineItemErrors.QuantityInvalid.Description, lineItemCreationResult.TopError.Description);
    }

    [Fact]
    public void CreateLineItem_WithInvalidUnitPrice_ShouldFail()
    {
        var lineItemCreationResult = InvoiceLineItem.Create(Guid.NewGuid(), 1, "Item", 1, 0m);

        Assert.True(lineItemCreationResult.IsError);
        Assert.Equal(InvoiceLineItemErrors.UnitPriceInvalid.Code, lineItemCreationResult.TopError.Code);
        Assert.Equal(InvoiceLineItemErrors.UnitPriceInvalid.Description, lineItemCreationResult.TopError.Description);
    }
}