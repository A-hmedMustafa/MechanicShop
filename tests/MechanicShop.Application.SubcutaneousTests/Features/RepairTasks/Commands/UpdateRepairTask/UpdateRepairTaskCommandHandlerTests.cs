using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.UpdateRepairTask;


[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateRepairTaskCommandHandlerTests: SubcutaneousTestBase
{
    public UpdateRepairTaskCommandHandlerTests(WebAppFactory factory) : base(factory) { }

    private async Task<(RepairTask repairTask, Guid part1Id, Guid part2Id)> SeedRepairTaskWithPartsAsync()
    {
        var repairTask = RepairTaskFactory.CreateRepairTask(
            name: "Original Task",
            laborCost: 100m,
            repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;

        var part1 = PartFactory.CreatePart(name: "Part A", cost: 20m, quantity: 2).Value;
        var part2 = PartFactory.CreatePart(name: "Part B", cost: 15m, quantity: 1).Value;

        repairTask.UpsertParts([part1, part2]);

        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(repairTask);
        TrackEntity(part1);
        TrackEntity(part2);

        return (repairTask, part1.Id, part2.Id);
    }


    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
        
        var (repairTask, part1Id, part2Id) = await SeedRepairTaskWithPartsAsync();

        var updateRepairTaskCommand = new UpdateRepairTaskCommand(
            RepairTaskId: repairTask.Id,
            Name: "Updated Task",
            LaborCost: 120m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min75,
            Parts:
            [
                new UpdateRepairTaskPartCommand(PartId: part1Id, Name: "Part A", Cost: 25m, Quantity: 3),
                new UpdateRepairTaskPartCommand(PartId: null, Name: "Part C", Cost: 30m, Quantity: 1)
            ]);

      
        var updateRepairTaskCommandResult = await _mediator.Send(updateRepairTaskCommand, CancellationToken.None);

        
        Assert.True(updateRepairTaskCommandResult.IsSuccess);
        await _mediator.Send(new RemoveRepairTaskCommand(repairTask.Id), CancellationToken.None);
    }

    
    [Fact]
    public async Task Handle_WhenRepairTaskNotFound_ShouldFail()
    {
        var updateRepairTaskCommand = new UpdateRepairTaskCommand(
            RepairTaskId: Guid.NewGuid(),
            Name: "Ghost Task",
            LaborCost: 50m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            Parts: [new UpdateRepairTaskPartCommand(null, "Part", 10m, 1)]);

        var updateRepairTaskCommandResult = await _mediator.Send(updateRepairTaskCommand, CancellationToken.None);

        Assert.False(updateRepairTaskCommandResult.IsSuccess);
        Assert.Contains(updateRepairTaskCommandResult.Errors, e => e.Code == "RepairTask.NotFound");
    }

  
}