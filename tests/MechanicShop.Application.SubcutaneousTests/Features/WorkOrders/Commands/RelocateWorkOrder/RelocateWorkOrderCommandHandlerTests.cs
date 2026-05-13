using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.RelocateWorkOrder;

public class RelocateWorkOrderCommandHandlerTests: SubcutaneousTestBase
{
    public RelocateWorkOrderCommandHandlerTests(WebAppFactory factory) : base(factory) { }
    
    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
       
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min75).Value;
        var employee = EmployeeFactory.CreateEmployee().Value;
        var currentSchedule = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(11);
        var newSchedule = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(15);

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(employee, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(repairTask);
        TrackEntity(employee);

        var workOrderCreationCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle.Id,
            StartsAt: currentSchedule,
            RepairTaskIds: [repairTask.Id],
            LaborId: employee.Id
        );

        var creationCommandResult = await _mediator.Send(workOrderCreationCommand, CancellationToken.None);

        var relocationCommand = new RelocateWorkOrderCommand(
            WorkOrderId: creationCommandResult.Value.WorkOrderId,
            NewStartAt: newSchedule,
            NewSpot: Spot.A
        );

        var relocationCommandResult = await _mediator.Send(relocationCommand, CancellationToken.None);

        Assert.True(relocationCommandResult.IsSuccess);
        
    }

    [Fact]
    public async Task Handle_WhenWorkOrderMissing_ShouldFail()
    {
        var newSchedule = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(11);
        var relocationCommand = new RelocateWorkOrderCommand(
            WorkOrderId: Guid.NewGuid(),
            NewStartAt: newSchedule,
            NewSpot: Spot.A
        );

        var relocationCommandResult = await _mediator.Send(relocationCommand, CancellationToken.None);

        Assert.False(relocationCommandResult.IsSuccess);
        Assert.Contains(relocationCommandResult.Errors, error => error.Code == "ApplicationErrors.WorkOrders.NotFound");

    }

    [Fact]
    public async Task Handle_WhenSpotUnavailable_ShouldFail()
    {
        var vehicle1 = VehicleFactory.CreateVehicle().Value;  
        var vehicle2 = VehicleFactory.CreateVehicle().Value;  
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min75).Value;
        var labor = EmployeeFactory.CreateEmployee().Value;
        var currentSchedule = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(11);
        var newSchedule = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(14);

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle1, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle2, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle1);
        TrackEntity(vehicle2);
        TrackEntity(repairTask);
        TrackEntity(labor);

        var workOrder1CreationCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle1.Id,
            StartsAt: currentSchedule,
            RepairTaskIds: [repairTask.Id],
            LaborId: labor.Id
        );

        var workOrder2CreationCommand = new CreateWorkOrderCommand(
            Spot: Spot.B,
            VehicleId: vehicle2.Id,
            StartsAt: newSchedule,
            RepairTaskIds: [repairTask.Id],
            LaborId: labor.Id
        );
        await _mediator.Send(workOrder2CreationCommand, CancellationToken.None);
        var creation1CommandResult = await _mediator.Send(workOrder1CreationCommand, CancellationToken.None);

        var workOrder1relocationCommand = new RelocateWorkOrderCommand(
            WorkOrderId: creation1CommandResult.Value.WorkOrderId,
            NewStartAt: newSchedule,
            NewSpot: Spot.B
        );

        var relocationCommandResult = await _mediator.Send(workOrder1relocationCommand, CancellationToken.None);

        Assert.False(relocationCommandResult.IsSuccess);
    }

    [Fact]
    public async Task Handle_WithLaborConflict_ShouldFail()
    {
        var vehicle1 = VehicleFactory.CreateVehicle().Value;  
        var vehicle2 = VehicleFactory.CreateVehicle().Value;  
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min75).Value;
        var labor = EmployeeFactory.CreateEmployee().Value;
        var currentSchedule = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(11);
        var newSchedule = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(14);

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle1, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle2, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle1);
        TrackEntity(vehicle2);
        TrackEntity(repairTask);
        TrackEntity(labor);

        var workOrder1CreationCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle1.Id,
            StartsAt: currentSchedule,
            RepairTaskIds: [repairTask.Id],
            LaborId: labor.Id
        );

        var workOrder2CreationCommand = new CreateWorkOrderCommand(
            Spot: Spot.B,
            VehicleId: vehicle2.Id,
            StartsAt: newSchedule,
            RepairTaskIds: [repairTask.Id],
            LaborId: labor.Id
        );
        await _mediator.Send(workOrder2CreationCommand, CancellationToken.None);
        var creation1CommandResult = await _mediator.Send(workOrder1CreationCommand, CancellationToken.None);

        var workOrder1relocationCommand = new RelocateWorkOrderCommand(
            WorkOrderId: creation1CommandResult.Value.WorkOrderId,
            NewStartAt: newSchedule,
            NewSpot: Spot.C
        );

        var relocationCommandResult = await _mediator.Send(workOrder1relocationCommand, CancellationToken.None);

        Assert.False(relocationCommandResult.IsSuccess);
        Assert.Contains(relocationCommandResult.Errors, error => error.Code == "Employee.LaborOccupied");

    }
    [Fact]
    public async Task Handle_WithVehicleConflict_ShouldFail()
    {
        var vehicle1 = VehicleFactory.CreateVehicle().Value;  
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1]).Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min75).Value;
        var labor1 = EmployeeFactory.CreateEmployee().Value;
        var labor2 = EmployeeFactory.CreateEmployee().Value;
        var currentSchedule = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(11);
        var newSchedule = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(14);

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle1, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor1, CancellationToken.None);
        await _context.Employees.AddAsync(labor2, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);
        
        TrackEntity(customer);
        TrackEntity(vehicle1);
        TrackEntity(repairTask);
        TrackEntity(labor1);
        TrackEntity(labor2);

        var workOrder1CreationCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle1.Id,
            StartsAt: currentSchedule,
            RepairTaskIds: [repairTask.Id],
            LaborId: labor1.Id
        );
        
        var workOrder2CreationCommand = new CreateWorkOrderCommand(
            Spot: Spot.B,
            VehicleId: vehicle1.Id,
            StartsAt: newSchedule,
            RepairTaskIds: [repairTask.Id],
            LaborId: labor2.Id
        );
        await _mediator.Send(workOrder2CreationCommand, CancellationToken.None);
        var creation1CommandResult = await _mediator.Send(workOrder1CreationCommand, CancellationToken.None);

        var workOrder1relocationCommand = new RelocateWorkOrderCommand(
            WorkOrderId: creation1CommandResult.Value.WorkOrderId,
            NewStartAt: newSchedule,
            NewSpot: Spot.C
        );
        var relocationCommandResult = await _mediator.Send(workOrder1relocationCommand, CancellationToken.None);

        Assert.False(relocationCommandResult.IsSuccess);
        Assert.Contains(relocationCommandResult.Errors, error => error.Code == "Vehicle_Overlapping_WorkOrder");

    }

    [Fact]
    public async Task Handle_WithRelocationOutsideOperatingHour_ShouldFail()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min75).Value;
        var employee = EmployeeFactory.CreateEmployee().Value;
        var currentSchedule = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(11);
        var newSchedule = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(17);

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            id:Guid.NewGuid(),
            vehicleId: vehicle.Id,
            startsAt: currentSchedule,
            endsAt: currentSchedule.Add(TimeSpan.FromMinutes(75)),
            laborId: employee.Id,
            spot: Spot.A,
            repairTasks: [repairTask]).Value;
        
        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.WorkOrders.AddAsync(workOrder, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(employee, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(repairTask);
        TrackEntity(employee);

        // var workOrderCreationCommand = new CreateWorkOrderCommand(
        //     Spot: Spot.A,
        //     VehicleId: vehicle.Id,
        //     StartsAt: currentSchedule,
        //     RepairTaskIds: [repairTask.Id],
        //     LaborId: employee.Id
        // );

        // var creationCommandResult = await _mediator.Send(workOrderCreationCommand, CancellationToken.None);

        var relocationCommand = new RelocateWorkOrderCommand(
            WorkOrderId: workOrder.Id,
            NewStartAt: newSchedule,
            NewSpot: Spot.A
        );

        var relocationCommandResult = await _mediator.Send(relocationCommand, CancellationToken.None);

        Assert.True(relocationCommandResult.IsError);
        Assert.Contains(relocationCommandResult.Errors, error => error.Code == "ApplicationErrors.WorkOrders.Outside.OperatingHours");
    }
}