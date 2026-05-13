using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.RemoveRepairTask;

public class RemoveRepairTaskCommandValidatorTests
{
    private readonly RemoveRepairTaskCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidRepairTaskId_ShouldSucceed()
    {
        var removeRepairTaskCommand = new RemoveRepairTaskCommand(Guid.NewGuid());

        var removeRepairTaskCommandResult = _validator.Validate(removeRepairTaskCommand);

        Assert.True(removeRepairTaskCommandResult.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyRepairTaskId_ShouldFail()
    {
        var removeRepairTaskCommand = new RemoveRepairTaskCommand(Guid.Empty);

        var removeRepairTaskCommandResult = _validator.Validate(removeRepairTaskCommand);

        Assert.False(removeRepairTaskCommandResult.IsValid);
        Assert.Contains(removeRepairTaskCommandResult.Errors, e => e.PropertyName == "RepairTaskId");
    }
}