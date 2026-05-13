using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using MechanicShop.Application.SubcutaneousTests.Common;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries.GetRepairTaskById;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetRepairTaskByIdQueryHandlerTests: SubcutaneousTestBase
{
    public GetRepairTaskByIdQueryHandlerTests(WebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Handle_WithExistingRepairTask_ShouldReturnDto()
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);
        TrackEntity(repairTask); 
        
        var getRepairTaskByIdQuery = new GetRepairTaskByIdQuery(repairTask.Id);

     
        var getRepairTaskByIdQueryResult = await _mediator.Send(getRepairTaskByIdQuery, CancellationToken.None);

        
        Assert.True(getRepairTaskByIdQueryResult.IsSuccess);
        Assert.NotNull(getRepairTaskByIdQueryResult.Value);
        Assert.Equal(repairTask.Id, getRepairTaskByIdQueryResult.Value.RepairTaskId);
        Assert.Equal(repairTask.Name, getRepairTaskByIdQueryResult.Value.Name);
    }

    [Fact]
    public async Task Handle_WhenRepairTaskNotFound_ShouldFail()
    {
        var getRepairTaskByIdQuery = new GetRepairTaskByIdQuery(Guid.NewGuid());

        var getRepairTaskByIdQueryResult = await _mediator.Send(getRepairTaskByIdQuery, CancellationToken.None);

        Assert.False(getRepairTaskByIdQueryResult.IsSuccess);
        Assert.Contains(getRepairTaskByIdQueryResult.Errors, e => e.Code == "RepairTask.NotFound");
    }
}