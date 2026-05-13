using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.CreateRepairTask;

public class CreateRepairTaskPartCommandValidatorTests
{
    private readonly CreateRepairTaskPartCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var createRepairTaskPartCommand = new CreateRepairTaskPartCommand(
            Name: "Brake Pad",
            Cost: 49.99m,
            Quantity: 2);

        var createRepairTaskPartCommandResult = _validator.Validate(createRepairTaskPartCommand);

        Assert.True(createRepairTaskPartCommandResult.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var createRepairTaskPartCommand = new CreateRepairTaskPartCommand(
            Name: "",
            Cost: 10m,
            Quantity: 1);

        var createRepairTaskPartCommandResult = _validator.Validate(createRepairTaskPartCommand);

        Assert.False(createRepairTaskPartCommandResult.IsValid);
        Assert.Contains(createRepairTaskPartCommandResult.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithZeroCost_ShouldFail()
    {
        var createRepairTaskPartCommand = new CreateRepairTaskPartCommand(
            Name: "Oil Filter",
            Cost: 0,
            Quantity: 1);

        var createRepairTaskPartCommandResult = _validator.Validate(createRepairTaskPartCommand);

        Assert.False(createRepairTaskPartCommandResult.IsValid);
        Assert.Contains(createRepairTaskPartCommandResult.Errors, e => e.PropertyName == "Cost");
    }

    [Fact]
    public void Validate_WithZeroQuantity_ShouldFail()
    {
        var createRepairTaskPartCommand = new CreateRepairTaskPartCommand(
            Name: "Oil Filter",
            Cost: 5m,
            Quantity: 0);

        var createRepairTaskPartCommandResult = _validator.Validate(createRepairTaskPartCommand);

        Assert.False(createRepairTaskPartCommandResult.IsValid);
        Assert.Contains(createRepairTaskPartCommandResult.Errors, e => e.PropertyName == "Quantity");
    }

    [Fact]
    public void Validate_WithTooLongName_ShouldFail()
    {
        var createRepairTaskPartCommand = new CreateRepairTaskPartCommand(
            Name: new string('A', 101),
            Cost: 10m,
            Quantity: 1);

        var createRepairTaskPartCommandResult = _validator.Validate(createRepairTaskPartCommand);

        Assert.False(createRepairTaskPartCommandResult.IsValid);
        Assert.Contains(createRepairTaskPartCommandResult.Errors, e => e.PropertyName == "Name");
    }
}