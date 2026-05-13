using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.RepairTasks.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.CreateRepairTask;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateRepairTaskCommandHandlerTests: SubcutaneousTestBase
{
    public CreateRepairTaskCommandHandlerTests(WebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
        var createRepairTaskCommand = new CreateRepairTaskCommand(
            Name: "Brake Replacements",
            LaborCost: 75m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min60,
            Parts:
            [
                new CreateRepairTaskPartCommand(Name: "Brakes Pads", Cost: 40m, Quantity: 2),
                new CreateRepairTaskPartCommand(Name: "Brakes Fluid", Cost: 10m, Quantity: 1)
            ]);

        var createRepairTaskCommandResult = await _mediator.Send(createRepairTaskCommand, CancellationToken.None);

        Assert.True(createRepairTaskCommandResult.IsSuccess);
        Assert.NotNull(createRepairTaskCommandResult.Value);
        Assert.Equal("Brake Replacements", createRepairTaskCommandResult.Value.Name);
        await _context.RepairTasks
            .Where(rt => rt.Id == createRepairTaskCommandResult.Value.RepairTaskId)
            .ExecuteDeleteAsync();
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldFail()
    {
        var existing = RepairTaskFactory.CreateRepairTask(name: "Tire Rotation").Value;
        await _context.RepairTasks.AddAsync(existing, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);
        TrackEntity(existing);
        var createRepairTaskCommand = new CreateRepairTaskCommand(
            Name: "Tire Rotation",
            LaborCost: 25m,
            EstimatedDurationInMins: RepairDurationInMinutes.Min15,
            Parts: [new CreateRepairTaskPartCommand("Weight", 2m, 4)]);

        var createRepairTaskCommandResult = await _mediator.Send(createRepairTaskCommand, CancellationToken.None);

        Assert.False(createRepairTaskCommandResult.IsSuccess);
        Assert.Contains(createRepairTaskCommandResult.Errors, e => e.Code == "RepairTaskPart.Duplicate");
    }
}