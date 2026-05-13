using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.RepairTasks;
using Microsoft.VisualBasic;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class RepairTaskMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var part = PartFactory.CreatePart(cost: 300, quantity: 3).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask(parts: [part]).Value;

        var totalPartsCost = part.Cost * part.Quantity;
        var totalLaborCost = repairTask.LaborCost;
        var totalCost = totalPartsCost + totalLaborCost;
        var repairTaskDto = repairTask.ToDto();

        Assert.Equal(repairTask.Id, repairTaskDto.RepairTaskId);
        Assert.Equal(repairTask.Name, repairTaskDto.Name);
        Assert.Equal(totalLaborCost, repairTaskDto.LaborCost);
        Assert.Equal(totalCost, repairTaskDto.TotalCost);
        Assert.Equal(repairTask.EstimatedDurationInMins, repairTaskDto.EstimatedDurationInMinutes);
        
        Assert.Single(repairTaskDto.Parts);
        var partDto = repairTaskDto.Parts[0];
        Assert.Equal(part.Id, partDto.PartId);
        Assert.Equal(part.Name, partDto.Name);
        Assert.Equal(part.Cost, partDto.Cost);
        Assert.Equal(part.Quantity, partDto.Quantity);
    }

    [Fact]
    public void ToDtos_ShouldMapCorrectly()
    {
        var part = PartFactory.CreatePart(cost: 300, quantity: 3).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask(parts: [part]).Value;

        var totalPartsCost = part.Cost * part.Quantity;
        var totalLaborCost = repairTask.LaborCost;
        var totalCost = totalPartsCost + totalLaborCost;
    
        var repairTasks = new List<RepairTask> {repairTask};
        var repairTasksDtos = repairTasks.ToDtos();

        Assert.Single(repairTasksDtos);

        var repairTaskDto = repairTasksDtos[0];

        Assert.Equal(repairTask.Id, repairTaskDto.RepairTaskId);
        Assert.Equal(repairTask.Name, repairTaskDto.Name);
        Assert.Equal(totalLaborCost, repairTaskDto.LaborCost);
        Assert.Equal(totalCost, repairTaskDto.TotalCost);
        Assert.Equal(repairTask.EstimatedDurationInMins, repairTaskDto.EstimatedDurationInMinutes);
        
        Assert.Single(repairTaskDto.Parts);
        var partDto = repairTaskDto.Parts[0];
        
        Assert.Equal(part.Id, partDto.PartId);
        Assert.Equal(part.Name, partDto.Name);
        Assert.Equal(part.Cost, partDto.Cost);
        Assert.Equal(part.Quantity, partDto.Quantity);
    }
}