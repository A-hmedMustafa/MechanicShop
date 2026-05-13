using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderByIdQuery;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrderById;

public class GetWorkOrderByIdQueryValidatorTests
{
    private readonly GetAppointmentByIdQueryValidator _validator;
    public GetWorkOrderByIdQueryValidatorTests()
    {
        _validator = new GetAppointmentByIdQueryValidator();
    }

    [Fact]
    public void Validate_WithValidWorkOrderId_ShouldSucceed()
    {
        var query = new GetWorkOrderByIdQuery(Guid.NewGuid());

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyWorkOrderId_ShouldFail()
    {
        var query = new GetWorkOrderByIdQuery(Guid.Empty);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "WorkOrderId");
    }

}