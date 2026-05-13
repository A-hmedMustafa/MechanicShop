using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicShop.Domain.RepairTasks.Enums;
using Xunit;

public class UpdateRepairTaskCommandValidatorTests
{
    private readonly UpdateRepairTaskCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var updateRepairTaskCommand = new UpdateRepairTaskCommand(
            RepairTaskId: Guid.NewGuid(),
            Name: "Oil Change",
            LaborCost: 50m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: [new UpdateRepairTaskPartCommand(null, "Filter", 10m, 1)]);

        var updateRepairTaskCommandResult = _validator.Validate(updateRepairTaskCommand);
        Assert.True(updateRepairTaskCommandResult.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyRepairTaskId_ShouldFail()
    {
        var updateRepairTaskCommand = new UpdateRepairTaskCommand(
            RepairTaskId: Guid.Empty,
            Name: "Oil Change",
            LaborCost: 50m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: [new UpdateRepairTaskPartCommand(null, "Filter", 10m, 1)]);

        var updateRepairTaskCommandResult = _validator.Validate(updateRepairTaskCommand);
        Assert.False(updateRepairTaskCommandResult.IsValid);
        Assert.Contains(updateRepairTaskCommandResult.Errors, e => e.PropertyName == "RepairTaskId");
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var updateRepairTaskCommand = new UpdateRepairTaskCommand(
            RepairTaskId: Guid.NewGuid(),
            Name: "",
            LaborCost: 50m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: [new UpdateRepairTaskPartCommand(null, "Filter", 10m, 1)]);

        var updateRepairTaskCommandResult = _validator.Validate(updateRepairTaskCommand);
        Assert.False(updateRepairTaskCommandResult.IsValid);
        Assert.Contains(updateRepairTaskCommandResult.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithLaborCostBelowMinimum_ShouldFail()
    {
        var updateRepairTaskCommand = new UpdateRepairTaskCommand(
            RepairTaskId: Guid.NewGuid(),
            Name: "Oil Change",
            LaborCost: 0,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: [new UpdateRepairTaskPartCommand(null, "Filter", 10m, 1)]);

        var updateRepairTaskCommandResult = _validator.Validate(updateRepairTaskCommand);
        Assert.False(updateRepairTaskCommandResult.IsValid);
        Assert.Contains(updateRepairTaskCommandResult.Errors, e => e.PropertyName == "LaborCost");
    }

    
    [Fact]
    public void Validate_WithInvalidDuration_ShouldFail()
    {
        var updateRepairTaskCommand = new UpdateRepairTaskCommand(
            RepairTaskId: Guid.NewGuid(),
            Name: "Oil Change",
            LaborCost: 0,
            EstimatedDurationInMins: (RepairDurationInMinutes)999,
            Parts: [new UpdateRepairTaskPartCommand(null, "Filter", 10m, 1)]);

        var updateRepairTaskCommandResult = _validator.Validate(updateRepairTaskCommand);
        Assert.False(updateRepairTaskCommandResult.IsValid);
        Assert.Contains(updateRepairTaskCommandResult.Errors, e => e.PropertyName == "EstimatedDurationInMins");
    }

    [Fact]
    public void Validate_WithLaborCostAboveMaximum_ShouldFail()
    {
        var updateRepairTaskCommand = new UpdateRepairTaskCommand(
            RepairTaskId: Guid.NewGuid(),
            Name: "Oil Change",
            LaborCost: 10001m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: [new UpdateRepairTaskPartCommand(null, "Filter", 10m, 1)]);

        var updateRepairTaskCommandResult = _validator.Validate(updateRepairTaskCommand);
        Assert.False(updateRepairTaskCommandResult.IsValid);
        Assert.Contains(updateRepairTaskCommandResult.Errors, e => e.PropertyName == "LaborCost");
    }

    [Fact]
    public void Validate_WithEmptyPartsList_ShouldFail()
    {
        var updateRepairTaskCommand = new UpdateRepairTaskCommand(
            RepairTaskId: Guid.NewGuid(),
            Name: "Oil Change",
            LaborCost: 50m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: []);

        var updateRepairTaskCommandResult = _validator.Validate(updateRepairTaskCommand);
        Assert.False(updateRepairTaskCommandResult.IsValid);
        Assert.Contains(updateRepairTaskCommandResult.Errors, e => e.PropertyName == "Parts");
    }

    [Fact]
    public void Validate_WithInvalidPart_ShouldFail()
    {
        var updateRepairTaskCommand = new UpdateRepairTaskCommand(
            RepairTaskId: Guid.NewGuid(),
            Name: "Oil Change",
            LaborCost: 50m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: [new UpdateRepairTaskPartCommand(null, "", 0, 0)]);

        var updateRepairTaskCommandResult = _validator.Validate(updateRepairTaskCommand);
        Assert.False(updateRepairTaskCommandResult.IsValid);
    }
}