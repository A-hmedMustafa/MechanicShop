using MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.AssignLabor;

public class AssignLaborCommandValidatorTests
{
    private readonly AssignLaborCommandValidator _validator;
    public AssignLaborCommandValidatorTests()
    {
        _validator = new AssignLaborCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var assigningCommand = new AssignLaborCommand(
            WorkOrderId: Guid.NewGuid(),
            LaborId: Guid.NewGuid()
        );

        var assigningCommandResult = _validator.Validate(assigningCommand);

        Assert.True(assigningCommandResult.IsValid);

    }
    
    [Fact]
    public void Validate_WithWorkOrderIdEmpty_ShouldSucceed()
    {
        var assigningCommand = new AssignLaborCommand(
            WorkOrderId: Guid.Empty,
            LaborId: Guid.NewGuid()
        );

        var assigningCommandResult = _validator.Validate(assigningCommand);

        Assert.False(assigningCommandResult.IsValid);
        Assert.Contains(assigningCommandResult.Errors, failure => failure.PropertyName == "WorkOrderId");
        
    }
    
    [Fact]
    public void Validate_WithLaborIdEmpty_ShouldSucceed()
    {
        var assigningCommand = new AssignLaborCommand(
            WorkOrderId: Guid.NewGuid(),
            LaborId: Guid.Empty
        );

        var assigningCommandResult = _validator.Validate(assigningCommand);

        Assert.False(assigningCommandResult.IsValid);
        Assert.Contains(assigningCommandResult.Errors, failure => failure.PropertyName == "LaborId");
        
    }
}