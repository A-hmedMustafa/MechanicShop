using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using Xunit;

namespace MechanicShop.Domain.UnitTests.RepairTasks;

public class RepairTasksTests
{
    [Fact]
    public void CreateRepairTask_WithValidData_ShouldSucceed()
    {
        Guid id = Guid.NewGuid();
        const string name = "Oil Change";
        const decimal laborCost = 150m;
        const decimal partCost = 70m;
        const int partQuantity = 1;
        const RepairDurationInMinutes repairDuration = RepairDurationInMinutes.Min30;
        List<Part> parts = [PartFactory.CreatePart(cost:partCost,quantity:partQuantity).Value];
        var totalCost = (partCost * partQuantity) + laborCost;

        var repairTaskCreationResult = RepairTaskFactory.CreateRepairTask(
            id: id,
            name: name,
            laborCost: laborCost,
            repairDurationInMinutes: repairDuration,
            parts: parts
        );

        var newRepairTask = repairTaskCreationResult.Value;

        Assert.True(repairTaskCreationResult.IsSuccess);
        Assert.NotNull(newRepairTask);
        Assert.IsType<RepairTask>(newRepairTask);
        Assert.Equal(id, newRepairTask.Id);
        Assert.Equal(name, newRepairTask.Name);
        Assert.Equal(laborCost, newRepairTask.LaborCost);
        Assert.Equal(repairDuration, newRepairTask.EstimatedDurationInMins);
        Assert.Equal(totalCost, newRepairTask.TotalCost);
        Assert.Single(newRepairTask.Parts);
    }
    
    [Fact]
    public void CreateRepairTask_WithEmptyName_ShouldFail()
    {
        const string name = " ";

        var repairTaskCreationResult = RepairTaskFactory.CreateRepairTask(
                    id: Guid.NewGuid(),
                    name: name,
                    laborCost: 100,
                    repairDurationInMinutes: RepairDurationInMinutes.Min30,
                    parts: [PartFactory.CreatePart().Value]);

        Assert.True(repairTaskCreationResult.IsError);

        Assert.Equal(RepairTaskErrors.NameRequired.Code, repairTaskCreationResult.TopError.Code);
    }

    [Fact]
    public void CreateRepairTask_WithInvalidLaborCost_ShouldFail()
    {
        const int laborCost = 0;

        var result = RepairTaskFactory.CreateRepairTask(
                    id: Guid.NewGuid(),
                    name: "Brake Inspection",
                    laborCost: laborCost,
                    repairDurationInMinutes: RepairDurationInMinutes.Min30,
                    parts: [PartFactory.CreatePart().Value]);

        Assert.True(result.IsError);

        Assert.Equal(RepairTaskErrors.LaborCostInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateRepairTask_WithInvalidDuration_ShouldFail()
    {
        const RepairDurationInMinutes repairDuration = (RepairDurationInMinutes)900;
    
        var repairTaskCreationResult = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: repairDuration
        );

        Assert.True(repairTaskCreationResult.IsError);
        Assert.Equal(RepairTaskErrors.DurationInvalid.Code, repairTaskCreationResult.TopError.Code);

    }

    [Fact]
    public void UpsertParts_AddsNewPart_WhenNotExisting()
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var part1 = PartFactory.CreatePart().Value;
        var upsertPartResult = repairTask.UpsertParts([part1]);

        Assert.True(upsertPartResult.IsSuccess);
        Assert.Contains(part1, repairTask.Parts);
    }

    [Fact]
    public void UpsertParts_UpdatesExistingPart_WhenExisting()
    {
        var id = Guid.NewGuid();
        var part1 = PartFactory.CreatePart(id: id, name: "Old", cost: 10, quantity: 2).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask(parts: [part1]).Value;
        var part1Updated = PartFactory.CreatePart(id: id, name: "New", cost: 20, quantity: 5).Value;

        var result = repairTask.UpsertParts([part1Updated]);
        var updated = repairTask.Parts.First(p => p.Id == id);

        Assert.True(result.IsSuccess);
        Assert.Equal("New", updated.Name);
        Assert.Equal(20m, updated.Cost);
        Assert.Equal(5, updated.Quantity);
    }

    [Fact]
    public void UpsertParts_RemovesMissingParts()
    {
        var part1 = PartFactory.CreatePart().Value;
        var part2 = PartFactory.CreatePart().Value;
        var task = RepairTaskFactory.CreateRepairTask(parts: [part1, part2]).Value;

        var result = task.UpsertParts([part2]);

        Assert.True(result.IsSuccess);
        Assert.Single(task.Parts, part2);
    }
    [Fact]
    public void UpdateRepairTask_WithValidValues_ShouldSucceed()
    {
        var task = RepairTaskFactory.CreateRepairTask().Value;

        var repairTaskCreationResult = task.Update("Valid", 123m, RepairDurationInMinutes.Min30);

        Assert.True(repairTaskCreationResult.IsSuccess);
        Assert.Equal("Valid", task.Name);
        Assert.Equal(123m, task.LaborCost);
        Assert.Equal(RepairDurationInMinutes.Min30, task.EstimatedDurationInMins);
    }

    [Theory]
    [InlineData("", 1, RepairDurationInMinutes.Min30, false)]
    [InlineData("  ", 1, RepairDurationInMinutes.Min30, false)]
    [InlineData("valid", 0, RepairDurationInMinutes.Min30, false)]
    [InlineData("valid", 10001, RepairDurationInMinutes.Min30, false)]
    public void UpdateRepairTask_WithInvalidNameOrCost_ShouldFail(
        string name, decimal cost, RepairDurationInMinutes repairDuration, bool expected)
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        var repairTaskUpdateResult = repairTask.Update(name, cost, repairDuration);

        Assert.Equal(expected, repairTaskUpdateResult.IsSuccess);
    }

    
    [Fact]
    public void UpdateRepairTask_WithInvalidDuration_ShouldFail()
    {
        var task = RepairTaskFactory.CreateRepairTask().Value;
        const RepairDurationInMinutes invalid = (RepairDurationInMinutes)999;

        var repairTaskUpdateResult = task.Update("Name", 1m, invalid);

        Assert.False(repairTaskUpdateResult.IsSuccess);
        Assert.Equal(RepairTaskErrors.DurationInvalid.Code, repairTaskUpdateResult.TopError.Code);
    }
}