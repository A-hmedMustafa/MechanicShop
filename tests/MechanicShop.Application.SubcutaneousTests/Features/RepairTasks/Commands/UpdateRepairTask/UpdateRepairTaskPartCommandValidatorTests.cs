using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.UpdateRepairTask;


public class UpdateRepairTaskPartCommandValidatorTests
{
    private readonly UpdateRepairTaskPartCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var updateRepairTaskPartCommand = new UpdateRepairTaskPartCommand(PartId: null, Name: "Filter", Cost: 12.50m, Quantity: 2);
        var updateRepairTaskPartCommandResult = _validator.Validate(updateRepairTaskPartCommand);
        Assert.True(updateRepairTaskPartCommandResult.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var updateRepairTaskPartCommand = new UpdateRepairTaskPartCommand(PartId: null, Name: "", Cost: 10m, Quantity: 1);
        var updateRepairTaskPartCommandResult = _validator.Validate(updateRepairTaskPartCommand);
        Assert.False(updateRepairTaskPartCommandResult.IsValid);
        Assert.Contains(updateRepairTaskPartCommandResult.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithCostBelowMinimum_ShouldFail()
    {
        var updateRepairTaskPartCommand = new UpdateRepairTaskPartCommand(PartId: null, Name: "Oil", Cost: 0.99m, Quantity: 1);
        var updateRepairTaskPartCommandResult = _validator.Validate(updateRepairTaskPartCommand);
        Assert.False(updateRepairTaskPartCommandResult.IsValid);
        Assert.Contains(updateRepairTaskPartCommandResult.Errors, e => e.PropertyName == "Cost");
    }

    [Fact]
    public void Validate_WithCostAboveMaximum_ShouldFail()
    {
        var updateRepairTaskPartCommand = new UpdateRepairTaskPartCommand(PartId: null, Name: "Oil", Cost: 10001m, Quantity: 1);
        var updateRepairTaskPartCommandResult = _validator.Validate(updateRepairTaskPartCommand);
        Assert.False(updateRepairTaskPartCommandResult.IsValid);
        Assert.Contains(updateRepairTaskPartCommandResult.Errors, e => e.PropertyName == "Cost");
    }

    [Fact]
    public void Validate_WithQuantityBelowMinimum_ShouldFail()
    {
        var updateRepairTaskPartCommand = new UpdateRepairTaskPartCommand(PartId: null, Name: "Oil", Cost: 10m, Quantity: 0);
        var updateRepairTaskPartCommandResult = _validator.Validate(updateRepairTaskPartCommand);
        Assert.False(updateRepairTaskPartCommandResult.IsValid);
        Assert.Contains(updateRepairTaskPartCommandResult.Errors, e => e.PropertyName == "Quantity");
    }

    [Fact]
    public void Validate_WithQuantityAboveMaximum_ShouldFail()
    {
        var updateRepairTaskPartCommand = new UpdateRepairTaskPartCommand(PartId: null, Name: "Oil", Cost: 10m, Quantity: 11);
        var updateRepairTaskPartCommandResult = _validator.Validate(updateRepairTaskPartCommand);
        Assert.False(updateRepairTaskPartCommandResult.IsValid);
        Assert.Contains(updateRepairTaskPartCommandResult.Errors, e => e.PropertyName == "Quantity");
    }

    [Fact]
    public void Validate_WithTooLongName_ShouldFail()
    {
        var updateRepairTaskPartCommand = new UpdateRepairTaskPartCommand(PartId: null, Name: new string('A', 101), Cost: 10m, Quantity: 1);
        var updateRepairTaskPartCommandResult = _validator.Validate(updateRepairTaskPartCommand);
        Assert.False(updateRepairTaskPartCommandResult.IsValid);
        Assert.Contains(updateRepairTaskPartCommandResult.Errors, e => e.PropertyName == "Name");
    }
}