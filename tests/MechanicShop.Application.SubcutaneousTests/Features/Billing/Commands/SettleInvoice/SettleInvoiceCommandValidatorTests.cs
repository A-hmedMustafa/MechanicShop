using MechanicShop.Application.Features.Billing.Commands.SettleInvoice;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.SettleInvoice; 

public class SettleInvoiceCommandValidatorTests
{
    private readonly SettleInvoiceCommandValidator _validator;

    public SettleInvoiceCommandValidatorTests()
    {
        _validator = new SettleInvoiceCommandValidator();
    }

    [Fact]
    public void Validate_WithValidInvoiceId_ShouldSucceed()
    {
        var settleInvoiceCommand = new SettleInvoiceCommand(Guid.NewGuid());

        var settleInvoiceCommandResult = _validator.Validate(settleInvoiceCommand);

        Assert.True(settleInvoiceCommandResult.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyInvoiceId_ShouldFail()
    {
        var settleInvoiceCommand = new SettleInvoiceCommand(Guid.Empty);

        var settleInvoiceCommandResult = _validator.Validate(settleInvoiceCommand);

        Assert.False(settleInvoiceCommandResult.IsValid);
        Assert.Contains(settleInvoiceCommandResult.Errors, e => e.PropertyName == "InvoiceId");
    }
}