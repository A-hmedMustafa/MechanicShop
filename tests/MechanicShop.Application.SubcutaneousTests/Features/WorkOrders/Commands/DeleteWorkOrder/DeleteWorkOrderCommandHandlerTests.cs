using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Emloyees;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.VisualBasic;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.DeleteWorkOrder;

public class DeleteWorkOrderCommandHandlerTests: SubcutaneousTestBase
{
    public DeleteWorkOrderCommandHandlerTests(WebAppFactory factory) : base(factory) { }

    private async Task<(Vehicle vehicle, Employee labor, RepairTask repairTask)> 
        SeedReferenceDataAsync()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        var labor = EmployeeFactory.CreateEmployee().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);
        TrackEntity(vehicle);
        TrackEntity(customer);
        TrackEntity(labor);
        TrackEntity(repairTask);

        return (vehicle, labor, repairTask);
    }

    private async Task<Guid> CreateWorkOrderAsync(
        Spot spot, Guid vehicleId, Guid laborId, Guid[] repairTaskIds, DateTimeOffset startsAt)
    {
        var command = new CreateWorkOrderCommand(
            spot,vehicleId,startsAt, repairTaskIds.ToList(), laborId);

        var commandResult = await _mediator.Send(command, CancellationToken.None);
        Assert.True(commandResult.IsSuccess,
            string.Join(", ", commandResult.Errors.Select(e => $"{e.Code} : {e.Description}")));    

        return commandResult.Value.WorkOrderId;
    }
    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
      
        var (vehicle, labor, repairTask) = await SeedReferenceDataAsync();

       
        var workOrderId = await CreateWorkOrderAsync(
            Spot.A, vehicle.Id, labor.Id, [repairTask.Id],
            DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10));

        var deleteCommand = new DeleteWorkOrderCommand(workOrderId);

     
        var result = await _mediator.Send(deleteCommand, CancellationToken.None);

        
        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task Handle_WithMissingWorkOrder_ShouldFail()
    {
        var workOrderDeletionCommand = new DeleteWorkOrderCommand(
            WorkOrderId: Guid.NewGuid()
        );

        var workOrderDeletionCommandResult = await _mediator.Send(workOrderDeletionCommand, CancellationToken.None);
        
        Assert.False(workOrderDeletionCommandResult.IsSuccess);
        Assert.Contains(workOrderDeletionCommandResult.Errors, error => error.Code == "ApplicationErrors.WorkOrders.NotFound");
    }

    [Fact]
    public async Task Handle_WithWorkOrderNotScheduled_ShouldFail()
    {
        var (vehicle, labor, repairTask) = await SeedReferenceDataAsync();

        var workOrderId = await CreateWorkOrderAsync(
            Spot.B, vehicle.Id, labor.Id, [repairTask.Id],
            DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10));

        var stateCommand = new UpdateWorkOrderStateCommand(workOrderId, WorkOrderState.InProgress);
        await _mediator.Send(stateCommand, CancellationToken.None);

        var deleteCommand = new DeleteWorkOrderCommand(workOrderId);
        var result = await _mediator.Send(deleteCommand, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.Code == "WorkOrderErrors.Readonly");

    }
}