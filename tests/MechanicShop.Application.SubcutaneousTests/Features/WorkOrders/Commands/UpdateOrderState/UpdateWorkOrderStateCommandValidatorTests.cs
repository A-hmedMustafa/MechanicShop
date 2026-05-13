using MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;
using MechanicShop.Domain.WorkOrders.Enums;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateOrderState;

public class UpdateWorkOrderStateCommandValidatorTests
{
    private readonly UpdateWorkOrderStateCommandValidator _validator;

    public UpdateWorkOrderStateCommandValidatorTests()
    {
        _validator = new UpdateWorkOrderStateCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var stateUpdateCommand = new UpdateWorkOrderStateCommand(
            WorkOrderId: Guid.NewGuid(),
            State: WorkOrderState.InProgress
        );
        
        var stateUpdateCommandResult = _validator.Validate(stateUpdateCommand);
        Assert.True(stateUpdateCommandResult.IsValid);
    }

    [Fact]
    public void Validate_WithInvalidState_ShouldFail()
    {
        var stateUpdateCommand = new UpdateWorkOrderStateCommand(
            WorkOrderId: Guid.NewGuid(),
            State: (WorkOrderState)650
        );
        
        var stateUpdateCommandResult = _validator.Validate(stateUpdateCommand);
        Assert.False(stateUpdateCommandResult.IsValid);
        Assert.Contains(stateUpdateCommandResult.Errors, e => e.PropertyName == "State");
    }

    
}