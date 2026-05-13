using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.DeleteWorkOrder;

public class DeleteWorkOrderCommandValidatorTests
{

    private readonly DeleteWorkOrderCommandValidator _validator;

    public DeleteWorkOrderCommandValidatorTests( )
    {
        _validator = new DeleteWorkOrderCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var deletionCommand = new DeleteWorkOrderCommand(
            WorkOrderId: Guid.NewGuid()
        );

        var deletionCommandResult = _validator.Validate(deletionCommand);

        Assert.True(deletionCommandResult.IsValid);
    }
    [Fact]
    public void Validate_WithWorkOrderIdEmpty_ShouldFail()
    {
        var deletionCommand = new DeleteWorkOrderCommand(
            WorkOrderId: Guid.Empty
        );

        var deletionCommandResult = _validator.Validate(deletionCommand);

        Assert.False(deletionCommandResult.IsValid);
        Assert.Contains(deletionCommandResult.Errors, failure => failure.PropertyName == "WorkOrderId");
    }
}