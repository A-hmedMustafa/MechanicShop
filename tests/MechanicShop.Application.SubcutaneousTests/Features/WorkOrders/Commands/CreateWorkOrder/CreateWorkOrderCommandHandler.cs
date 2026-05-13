using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Xunit;



namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.CreateWorkOrder;

public class CreateWorkOrderCommandHandlerTests: SubcutaneousTestBase
{
    public CreateWorkOrderCommandHandlerTests(WebAppFactory factory) : base(factory) { }


    [Fact]
    public async Task Handle_WithValidDate_ShouldSucceed()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var employee = EmployeeFactory.CreateEmployee().Value;
        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10);
        
        await _context.Customers.AddAsync(customer,CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle,CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask,CancellationToken.None);
        await _context.Employees.AddAsync(employee, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);
        
        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(repairTask);
        TrackEntity(employee);

        var commandCreation = new CreateWorkOrderCommand(
            Spot: Spot.B,
            VehicleId: vehicle.Id,
            StartsAt: scheduledAt,
            RepairTaskIds: [repairTask.Id],
            LaborId: employee.Id
        );

        var commandCreationResult = await _mediator.Send(commandCreation, CancellationToken.None);
        
        Assert.True(commandCreationResult.IsSuccess);
        var command = commandCreationResult.Value;
        Assert.Equal(vehicle.Id, command.Vehicle!.VehicleId);
        Assert.Equal(employee.Id, command.Labor!.LaborId);
        Assert.Equal(Spot.B, command.Spot);
        Assert.Single(command.RepairTasks);
        Assert.Equal(repairTask.Id, command.RepairTasks[0].RepairTaskId);
    }
    [Fact]
    public async Task Handle_WhenMissingRepairTask_ShouldFail()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var employee = EmployeeFactory.CreateEmployee().Value;
        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(11);
        var fakeRepairTaskId = Guid.NewGuid();

        await _context.Customers.AddAsync(customer,CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle,CancellationToken.None);
        await _context.Employees.AddAsync(employee, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);


        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(employee);

        var commandCreation = new CreateWorkOrderCommand(
            Spot: Spot.C,
            VehicleId: vehicle.Id,
            StartsAt: scheduledAt,
            RepairTaskIds: [fakeRepairTaskId],
            LaborId: employee.Id
        );

        var commandCreationResult = await _mediator.Send(commandCreation, CancellationToken.None);

        Assert.True(commandCreationResult.IsError);

    }

    [Fact]
    public async Task Handle_WithOutsideOperatingHours_ShouldFail()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var employee = EmployeeFactory.CreateEmployee().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(19);

        await _context.Customers.AddAsync(customer,CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle,CancellationToken.None);
        await _context.Employees.AddAsync(employee, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(employee);
        TrackEntity(repairTask);

        var commandCreation = new CreateWorkOrderCommand(
            Spot: Spot.D,
            VehicleId: vehicle.Id,
            StartsAt: scheduledAt,
            RepairTaskIds: [repairTask.Id],
            LaborId: employee.Id
        );

        var commandCreationResult = await _mediator.Send(commandCreation, CancellationToken.None);

        Assert.True(commandCreationResult.IsError);

    }
    
    [Fact]
    public async Task Handle_WhenRepairTaskDurationTooShort_ShouldFail()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var employee = EmployeeFactory.CreateEmployee().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min15).Value;
        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(11);

        await _context.Customers.AddAsync(customer,CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle,CancellationToken.None);
        await _context.Employees.AddAsync(employee, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(employee);
        TrackEntity(repairTask);

        var commandCreation = new CreateWorkOrderCommand(
            Spot: Spot.D,
            VehicleId: vehicle.Id,
            StartsAt: scheduledAt,
            RepairTaskIds: [repairTask.Id],
            LaborId: employee.Id
        );

        var commandCreationResult = await _mediator.Send(commandCreation, CancellationToken.None);

        Assert.True(commandCreationResult.IsError);    
        Assert.Contains(commandCreationResult.Errors, error => error.Code == "WorkOrder_TooShort");
    }

    [Fact]
    public async Task Handle_WhenMissingVehicle_ShouldFail()
    {
      
        var employee = EmployeeFactory.CreateEmployee().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(13);
        var fakeVehicleId = Guid.NewGuid();
       
        await _context.Employees.AddAsync(employee, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(employee);
        TrackEntity(repairTask);

        var commandCreation = new CreateWorkOrderCommand(
            Spot: Spot.C,
            VehicleId: fakeVehicleId,
            StartsAt: scheduledAt,
            RepairTaskIds: [repairTask.Id],
            LaborId: employee.Id
        );

        var commandCreationResult = await _mediator.Send(commandCreation, CancellationToken.None);

        Assert.True(commandCreationResult.IsError);
    }
    
    [Fact]
    public async Task Handle_WhenMissingLabor_ShouldFail()
    {
      
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(13);
        var fakeLaborId = Guid.NewGuid();
       
        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(repairTask);

        var commandCreation = new CreateWorkOrderCommand(
            Spot: Spot.C,
            VehicleId: vehicle.Id,
            StartsAt: scheduledAt,
            RepairTaskIds: [repairTask.Id],
            LaborId: fakeLaborId
        );

        var commandCreationResult = await _mediator.Send(commandCreation, CancellationToken.None);

        Assert.True(commandCreationResult.IsError);
    }
    
    [Fact]
    public async Task Handle_WithVehicleConflict_ShouldFail()
    {
      
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(13);
        var labor1 = EmployeeFactory.CreateEmployee().Value;
        var labor2 = EmployeeFactory.CreateEmployee().Value;
       
        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor1, CancellationToken.None);
        await _context.Employees.AddAsync(labor2, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);
        
        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(repairTask);
        TrackEntity(labor1);
        TrackEntity(labor2);

        var commandCreation1 = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle.Id,
            StartsAt: scheduledAt,
            RepairTaskIds: [repairTask.Id],
            LaborId: labor1.Id
        );

        var commandCreation2 = new CreateWorkOrderCommand(
            Spot: Spot.B,
            VehicleId: vehicle.Id,
            StartsAt: scheduledAt,
            RepairTaskIds: [repairTask.Id],
            LaborId: labor2.Id
        );

        await _mediator.Send(commandCreation1, CancellationToken.None);

        var commandCreation2Result = await _mediator.Send(commandCreation2, CancellationToken.None);

        Assert.True(commandCreation2Result.IsError);
        Assert.Equal("Vehicle_Overlapping_WorkOrders", commandCreation2Result.TopError.Code);

    }

    [Fact]
    public async Task Handle_WithLaborConflict_ShouldFail()
    {
      
        var customer1 = CustomerFactory.CreateCustomer().Value;
        var vehicle1 = customer1.Vehicles.First();
        var customer2 = CustomerFactory.CreateCustomer().Value;
        var vehicle2 = customer2.Vehicles.First();

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(13);
        var labor = EmployeeFactory.CreateEmployee().Value;
       
        await _context.Customers.AddAsync(customer1, CancellationToken.None);
        await _context.Customers.AddAsync(customer2, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle1, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle2, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer1);
        TrackEntity(customer2);
        TrackEntity(vehicle1);
        TrackEntity(vehicle2);
        TrackEntity(repairTask);
        TrackEntity(labor);

        var commandCreation1 = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle1.Id,
            StartsAt: scheduledAt,
            RepairTaskIds: [repairTask.Id],
            LaborId: labor.Id
        );

        var commandCreation2 = new CreateWorkOrderCommand(
            Spot: Spot.B,
            VehicleId: vehicle2.Id,
            StartsAt: scheduledAt,
            RepairTaskIds: [repairTask.Id],
            LaborId: labor.Id
        );

        await _mediator.Send(commandCreation1, CancellationToken.None);

        var commandCreation2Result = await _mediator.Send(commandCreation2, CancellationToken.None);

        Assert.True(commandCreation2Result.IsError);
        Assert.Equal("Labor_Occupied", commandCreation2Result.TopError.Code);
        
    }
    
    [Fact]
    public async Task Handle_WithSpotConflict_ShouldFail()
    {
      
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(17);
        var labor1 = EmployeeFactory.CreateEmployee().Value;
        var labor2 = EmployeeFactory.CreateEmployee().Value;
       
        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle1, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle2, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor1, CancellationToken.None);
        await _context.Employees.AddAsync(labor2, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle1);
        TrackEntity(vehicle2);
        TrackEntity(repairTask);
        TrackEntity(labor1);
        TrackEntity(labor2);
        
        var commandCreation1 = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle1.Id,
            StartsAt: scheduledAt,
            RepairTaskIds: [repairTask.Id],
            LaborId: labor1.Id
        );

        var commandCreation2 = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle2.Id,
            StartsAt: scheduledAt,
            RepairTaskIds: [repairTask.Id],
            LaborId: labor2.Id
        );

        await _mediator.Send(commandCreation1, CancellationToken.None);

        var commandCreation2Result = await _mediator.Send(commandCreation2, CancellationToken.None);

        Assert.True(commandCreation2Result.IsError);
        Assert.Equal("MechanicShop_Spot_Full", commandCreation2Result.TopError.Code);
        
    }
}