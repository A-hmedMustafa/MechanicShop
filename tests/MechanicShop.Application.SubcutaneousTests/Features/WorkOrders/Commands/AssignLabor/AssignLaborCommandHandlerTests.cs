using System;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Emloyees;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.AssignLabor;

public class AssignLaborCommandHandlerTests: SubcutaneousTestBase
{
    public AssignLaborCommandHandlerTests(WebAppFactory factory) : base(factory) { }

    private async Task<(Customer customer, Vehicle vehicle, Employee labor, RepairTask repairTask)>
        SeedReferenceDataAsync()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var labor = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(repairTask);
        TrackEntity(labor);
        return (customer, vehicle, labor, repairTask);
    }


    private async Task<Guid> CreateWorkOrderAsync(
        Spot spot, Guid vehicleId, Guid laborId, Guid[] repairTaskIds, DateTimeOffset startAt)
    {
        var command = new CreateWorkOrderCommand(spot, vehicleId, startAt, repairTaskIds.ToList(), laborId);
        var result = await _mediator.Send(command, CancellationToken.None);
        Assert.True(result.IsSuccess, string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));
        return result.Value.WorkOrderId;
    }
    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
        
        var (_, vehicle, labor, repairTask) = await SeedReferenceDataAsync();

        var workOrderId = await CreateWorkOrderAsync(
            Spot.A, vehicle.Id, labor.Id, [repairTask.Id],
            DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10));

        var assigningCommand = new AssignLaborCommand(workOrderId, labor.Id);

        
        var result = await _mediator.Send(assigningCommand, CancellationToken.None);

        
        Assert.True(result.IsSuccess);
        
    }
    
    [Fact]
    public async Task Handle_WhenMissingWorkOrder_ShouldSucceed()
    {
        var fakeWorkOrderId = Guid.NewGuid();

        var fakeLaborId = Guid.NewGuid();
        var assigningCommand = new AssignLaborCommand(
            WorkOrderId: fakeWorkOrderId,
            LaborId: fakeLaborId
        );

        var assigningCommandResult = await _mediator.Send(assigningCommand, CancellationToken.None);

        Assert.False(assigningCommandResult.IsSuccess);
        Assert.Contains(assigningCommandResult.Errors, error => error.Code == "ApplicationErrors.WorkOrders.NotFound");
    }
    [Fact]
    public async Task Handle_WhenMissingLabor_ShouldSucceed()
    {
       var (_, vehicle, labor, repairTask) = await SeedReferenceDataAsync();
       var workOrderId = await CreateWorkOrderAsync(
        Spot.A, 
        vehicle.Id,
        labor.Id,
        [repairTask.Id],
        DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(11));

        var fakeLaborId = Guid.NewGuid();
        var assigningCommand = new AssignLaborCommand(
            WorkOrderId: workOrderId,
            LaborId: fakeLaborId
        );

        var assigningCommandResult = await _mediator.Send(assigningCommand, CancellationToken.None);

        Assert.False(assigningCommandResult.IsSuccess);
        Assert.Contains(assigningCommandResult.Errors, error => error.Code == "Employee.LaborNotFound");
    }

     [Fact]
    public async Task Handle_WhenLaborOccupied_ShouldFail()
    {
    
    var (_, vehicle, labor1, repairTask) = await SeedReferenceDataAsync();

    var vehicle2 = VehicleFactory.CreateVehicle().Value;
    var customer2 = CustomerFactory.CreateCustomer(vehicles: [vehicle2]).Value;
    var labor2 = EmployeeFactory.CreateEmployee().Value;
    
    await _context.Employees.AddAsync(labor2, CancellationToken.None);
    await _context.Customers.AddAsync(customer2, CancellationToken.None);
    await _context.Vehicles.AddAsync(vehicle2, CancellationToken.None);
    await _context.SaveChangesAsync(CancellationToken.None);
    TrackEntity(labor2);
    TrackEntity(vehicle2);

    
    var start = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(14);

   
    var wo1Id = await CreateWorkOrderAsync(
        Spot.A, vehicle.Id, labor1.Id, [repairTask.Id], start);

    
    var wo2Id = await CreateWorkOrderAsync(
        Spot.B, vehicle2.Id, labor2.Id, [repairTask.Id], start);

    
    var assignCommand = new AssignLaborCommand(wo2Id, labor1.Id);
    var result = await _mediator.Send(assignCommand, CancellationToken.None);

  
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Code == "Employee.LaborOccupied");
    }
}

