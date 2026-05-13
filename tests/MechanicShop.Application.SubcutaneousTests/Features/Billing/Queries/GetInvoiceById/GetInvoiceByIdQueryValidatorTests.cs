using MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryValidatorTests
{
    private readonly GetInvoiceByIdQueryValidator _validator;

    public GetInvoiceByIdQueryValidatorTests()
    {
         _validator = new GetInvoiceByIdQueryValidator();
    }

    [Fact]
    public void Validate_WithValidInvoiceId_ShouldSucceed()
    {
        var getInvoiceQuery = new GetInvoiceByIdQuery(Guid.NewGuid());

        var getInvoiceQueryResult = _validator.Validate(getInvoiceQuery);

        Assert.True(getInvoiceQueryResult.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyInvoiceId_ShouldFail()
    {
        var getInvoiceQuery = new GetInvoiceByIdQuery(Guid.Empty);

        var getInvoiceQueryResult = _validator.Validate(getInvoiceQuery);

        Assert.False(getInvoiceQueryResult.IsValid);
        Assert.Contains(getInvoiceQueryResult.Errors, e => e.PropertyName == "InvoiceId");
    }
}