using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderByIdQuery;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrderById;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetWorkOrderByIdQueryHandlerTests: SubcutaneousTestBase
{
    public GetWorkOrderByIdQueryHandlerTests(WebAppFactory factory) : base(factory) { }

     
    [Fact]
    public async Task Handle_WithExistingWorkOrder_ShouldReturnDto()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var labor = EmployeeFactory.CreateEmployee().Value;
        var start = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10);

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(repairTask);
        TrackEntity(labor);

        var createWorkOrderCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle.Id,
            StartsAt: start,
            RepairTaskIds: [repairTask.Id],
            LaborId: labor.Id);

        var createWorkOrderCommandResult = await _mediator.Send(createWorkOrderCommand, CancellationToken.None);
        var workOrderId = createWorkOrderCommandResult.Value.WorkOrderId;

        var getWorkOrderQuery = new GetWorkOrderByIdQuery(workOrderId);

        
        var getWorkOrderQueryResult = await _mediator.Send(getWorkOrderQuery, CancellationToken.None);
        var workOrderDto = getWorkOrderQueryResult.Value;
       
        Assert.True(getWorkOrderQueryResult.IsSuccess);
        Assert.NotNull(workOrderDto);
        Assert.Equal(workOrderId, workOrderDto.WorkOrderId);
        Assert.Equal(vehicle.Id, workOrderDto.Vehicle!.VehicleId);
        Assert.Equal(labor.Id, workOrderDto.Labor!.LaborId);
        Assert.Single(workOrderDto.RepairTasks);
        Assert.Equal(repairTask.Id, workOrderDto.RepairTasks[0].RepairTaskId);
    }

  
    [Fact]
    public async Task Handle_WithMissingWorkOrder_ShouldFail()
    {
        var getWorkOrderQuery = new GetWorkOrderByIdQuery(Guid.NewGuid());

        var getWorkOrderQueryResult = await _mediator.Send(getWorkOrderQuery, CancellationToken.None);

        Assert.False(getWorkOrderQueryResult.IsSuccess);
        Assert.Contains(getWorkOrderQueryResult.Errors, error => error.Code == "ApplicationErrors.WorkOrders.NotFound");
    }
}