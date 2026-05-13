using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Domain.RepairTasks.Enums;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.CreateRepairTask;

public class CreateRepairTaskCommandValidatorTests
{
    private readonly CreateRepairTaskCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var createRepairTaskPartCommand = new CreateRepairTaskCommand(
            Name: "Oil Change",
            LaborCost: 30m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: [new CreateRepairTaskPartCommand("Oil Filter", 5m, 1)]);

        var createRepairTaskPartCommandResult = _validator.Validate(createRepairTaskPartCommand);

        Assert.True(createRepairTaskPartCommandResult.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var createRepairTaskPartCommand = new CreateRepairTaskCommand(
            Name: "",
            LaborCost: 30m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: [new CreateRepairTaskPartCommand("Oil Filter", 5m, 1)]);

        var createRepairTaskPartCommandResult = _validator.Validate(createRepairTaskPartCommand);

        Assert.False(createRepairTaskPartCommandResult.IsValid);
        Assert.Contains(createRepairTaskPartCommandResult.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithZeroLaborCost_ShouldFail()
    {
        var createRepairTaskPartCommand = new CreateRepairTaskCommand(
            Name: "Oil Change",
            LaborCost: 0,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: [new CreateRepairTaskPartCommand("Oil Filter", 5m, 1)]);

        var createRepairTaskPartCommandResult = _validator.Validate(createRepairTaskPartCommand);

        Assert.False(createRepairTaskPartCommandResult.IsValid);
        Assert.Contains(createRepairTaskPartCommandResult.Errors, e => e.PropertyName == "LaborCost");
    }

    [Fact]
    public void Validate_WithNullEstimatedDuration_ShouldFail()
    {
        var createRepairTaskPartCommand = new CreateRepairTaskCommand(
            Name: "Oil Change",
            LaborCost: 30m,
            EstimatedDurationInMins: null,
            Parts: [new CreateRepairTaskPartCommand("Oil Filter", 5m, 1)]);

        var createRepairTaskPartCommandResult = _validator.Validate(createRepairTaskPartCommand);

        Assert.False(createRepairTaskPartCommandResult.IsValid);
        Assert.Contains(createRepairTaskPartCommandResult.Errors, e => e.PropertyName == "EstimatedDurationInMins");
    }

    [Fact]
    public void Validate_WithNoParts_ShouldFail()
    {
        var createRepairTaskPartCommand = new CreateRepairTaskCommand(
            Name: "Oil Change",
            LaborCost: 30m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: []);

        var createRepairTaskPartCommandResult = _validator.Validate(createRepairTaskPartCommand);

        Assert.False(createRepairTaskPartCommandResult.IsValid);
        Assert.Contains(createRepairTaskPartCommandResult.Errors, e => e.PropertyName == "Parts");
    }

    [Fact]
    public void Validate_WithInvalidPart_ShouldFail()
    {
        var createRepairTaskPartCommand = new CreateRepairTaskCommand(
            Name: "Oil Change",
            LaborCost: 30m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: [new CreateRepairTaskPartCommand("", 0, 0)]); 

        var createRepairTaskPartCommandResult = _validator.Validate(createRepairTaskPartCommand);

        Assert.False(createRepairTaskPartCommandResult.IsValid);
        
    }
}