using MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries.GetInvoicePDF;

public class GetInvoicePdfCommandValidatorTests
{
    private readonly GetInvoicePdfQueryValidator _validator;

    public GetInvoicePdfCommandValidatorTests()
    {
        _validator = new GetInvoicePdfQueryValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var getInvoicePdfCommand = new GetInvoicePdfQuery(Guid.NewGuid());

        var getInvoicePdfCommandResult = _validator.Validate(getInvoicePdfCommand);

        Assert.True(getInvoicePdfCommandResult.IsValid);
    }

    [Fact]
    public void Validate_WithInvoiceIdEmpty_ShouldFail()
    {
        var getInvoicePdfCommand = new GetInvoicePdfQuery(Guid.Empty);

        var getInvoicePdfCommandResult = _validator.Validate(getInvoicePdfCommand);

        Assert.False(getInvoicePdfCommandResult.IsValid);
        Assert.Contains(getInvoicePdfCommandResult.Errors, failure => failure.PropertyName == "InvoiceId");
    }
}