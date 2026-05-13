using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.IssueInvoice;


public class IssueInvoiceCommandValidatorTests
{
    private readonly IssueInvoiceCommandValidator _validator;

    public IssueInvoiceCommandValidatorTests()
    {
        _validator = new IssueInvoiceCommandValidator();
    }

    [Fact]
    public void Validate_WithValidWorkOrderId_ShouldSucceed()
    {
        var issueInvoiceCommand = new IssueInvoiceCommand(WorkOrderId: Guid.NewGuid());

        var issueInvoiceCommandResult = _validator.Validate(issueInvoiceCommand);

        Assert.True(issueInvoiceCommandResult.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyWorkOrderId_ShouldFail()
    {
        var issueInvoiceCommand = new IssueInvoiceCommand(WorkOrderId: Guid.Empty);

        var issueInvoiceCommandResult = _validator.Validate(issueInvoiceCommand);

        Assert.False(issueInvoiceCommandResult.IsValid);
        Assert.Contains(issueInvoiceCommandResult.Errors, e => e.PropertyName == "WorkOrderId");
    }
}