using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;
using MechanicShop.Application.SubcutaneousTests.Common;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries.GetRepairTasks;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetRepairTasksQueryHandlerTests: SubcutaneousTestBase
{
    public GetRepairTasksQueryHandlerTests(WebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Handle_WhenRepairTasksExist_ShouldReturnList()
    {
        var repairTask1 = RepairTaskFactory.CreateRepairTask().Value;
        var repairTask2 = RepairTaskFactory.CreateRepairTask().Value;

        await _context.RepairTasks.AddAsync(repairTask1, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask2, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);
        TrackEntity(repairTask1);
        TrackEntity(repairTask2);

        var getRepairTasksQuery = new GetRepairTasksQuery();

   
        var getRepairTasksQueryResult = await _mediator.Send(getRepairTasksQuery, CancellationToken.None);

        
        Assert.True(getRepairTasksQueryResult.IsSuccess);
        Assert.NotNull(getRepairTasksQueryResult.Value);
        Assert.Contains(getRepairTasksQueryResult.Value, r => r.RepairTaskId == repairTask1.Id);
        Assert.Contains(getRepairTasksQueryResult.Value, r => r.RepairTaskId == repairTask2.Id);
    }
}