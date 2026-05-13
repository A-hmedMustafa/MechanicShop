using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.RemoveRepairTask;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RemoveRepairTaskCommandHandlerTests: SubcutaneousTestBase
{
    public RemoveRepairTaskCommandHandlerTests(WebAppFactory factory) : base(factory) { }
    [Fact]
    public async Task Handle_WithUnusedRepairTask_ShouldSucceed()
    {
        
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);
        TrackEntity(repairTask);

        var removeRepairTaskCommand = new RemoveRepairTaskCommand(repairTask.Id);

      
        var removeRepairTaskCommandResult = await _mediator.Send(removeRepairTaskCommand, CancellationToken.None);

        
        Assert.True(removeRepairTaskCommandResult.IsSuccess);
    }

    
    [Fact]
    public async Task Handle_WhenRepairTaskNotFound_ShouldFail()
    {
        var removeRepairTaskCommand = new RemoveRepairTaskCommand(Guid.NewGuid());

        var removeRepairTaskCommandResult = await _mediator.Send(removeRepairTaskCommand, CancellationToken.None);

        Assert.False(removeRepairTaskCommandResult.IsSuccess);
        Assert.Contains(removeRepairTaskCommandResult.Errors, e => e.Code == "RepairTask.NotFound");
    }

    [Fact]
    public async Task Handle_WhenRepairTaskInUse_ShouldFail()
    {
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);

        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        var labor = EmployeeFactory.CreateEmployee().Value;

        await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(repairTask);
        TrackEntity(vehicle);
        TrackEntity(customer);
        TrackEntity(labor);

        
        var createWorkOrderCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle.Id,
            StartsAt: DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10),
            RepairTaskIds: [repairTask.Id],
            LaborId: labor.Id);

        await _mediator.Send(createWorkOrderCommand, CancellationToken.None);

        var removeRepairTaskCommand = new RemoveRepairTaskCommand(repairTask.Id);

        var removeRepairTaskCommandResult = await _mediator.Send(removeRepairTaskCommand, CancellationToken.None);

        Assert.False(removeRepairTaskCommandResult.IsSuccess);
        Assert.Contains(removeRepairTaskCommandResult.Errors, e => e.Code == "RepairTask.InUse");
    }
}