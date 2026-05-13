using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public class UpdateWorkOrderRepairTasksCommandHandlerTests : SubcutaneousTestBase
{
    public UpdateWorkOrderRepairTasksCommandHandlerTests(WebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        var repairTask1 = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: RepairDurationInMinutes.Min30).Value;
        var labor = EmployeeFactory.CreateEmployee().Value;
        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10);

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask1, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(repairTask1);
        TrackEntity(labor);

        var createCommand = new CreateWorkOrderCommand(
            Spot.A, vehicle.Id, scheduledAt, [repairTask1.Id], labor.Id);
        var createResult = await _mediator.Send(createCommand, CancellationToken.None);
        Assert.True(createResult.IsSuccess,
            string.Join(", ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}")));

        var repairTask2 = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: RepairDurationInMinutes.Min45).Value;
        await _context.RepairTasks.AddAsync(repairTask2, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        var updateCommand = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: createResult.Value.WorkOrderId,
            RepairTaskIds: [repairTask2.Id]);
        var updateResult = await _mediator.Send(updateCommand, CancellationToken.None);

        Assert.True(updateResult.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenMissingWorkOrder_ShouldFail()
    {
        var updateCommand = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: Guid.NewGuid(),
            RepairTaskIds: [Guid.NewGuid()]);
        var updateResult = await _mediator.Send(updateCommand, CancellationToken.None);

        Assert.False(updateResult.IsSuccess);
        Assert.Contains(updateResult.Errors,
            error => error.Code == "ApplicationErrors.WorkOrders.NotFound");
    }

    [Fact]
    public async Task Handle_WithNonExistentRepairTasks_ShouldFail()
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

        var createCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle.Id,
            StartsAt: DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10),
            RepairTaskIds: [repairTask.Id],
            LaborId: labor.Id);
        var createResult = await _mediator.Send(createCommand, CancellationToken.None);
        Assert.True(createResult.IsSuccess,
            string.Join(", ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}")));

        var updateCommand = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: createResult.Value.WorkOrderId,
            RepairTaskIds: [Guid.NewGuid(), repairTask.Id]);
        var updateResult = await _mediator.Send(updateCommand, CancellationToken.None);

        Assert.False(updateResult.IsSuccess);
        Assert.Contains(updateResult.Errors, error => error.Code == "RepairTask.NotFound");
    }

    [Fact]
    public async Task Handle_WhenWorkOrderReadonly_ShouldFail()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        var repairTask1 = RepairTaskFactory.CreateRepairTask().Value;
        var labor = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask1, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(repairTask1);
        TrackEntity(labor);

        // bypass validator — directly seed with past start
        var yesterday = DateTimeOffset.UtcNow.Date.AddDays(-1);
        var pastStart = new DateTimeOffset(
            yesterday.Year, yesterday.Month, yesterday.Day,
            10, 0, 0, TimeSpan.Zero);

        var duration = TimeSpan.FromMinutes((int)repairTask1.EstimatedDurationInMins);

        var workOrder = WorkOrder.Create(
            Guid.NewGuid(),
            vehicle.Id,
            pastStart,
            pastStart.Add(duration),
            labor.Id,
            Spot.A,
            [repairTask1]).Value;

        await _context.WorkOrders.AddAsync(workOrder, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);
        TrackEntity(workOrder);

        // update state to InProgress — past start allows this
        var stateUpdateCommand = new UpdateWorkOrderStateCommand(
            workOrder.Id, WorkOrderState.InProgress);
        var stateUpdateResult = await _mediator.Send(stateUpdateCommand, CancellationToken.None);
        Assert.True(stateUpdateResult.IsSuccess,
            string.Join(", ", stateUpdateResult.Errors.Select(e => $"{e.Code}: {e.Description}")));

        var repairTask2 = RepairTaskFactory.CreateRepairTask().Value;
        await _context.RepairTasks.AddAsync(repairTask2, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        var updateCommand = new UpdateWorkOrderRepairTasksCommand(
            workOrder.Id, [repairTask2.Id]);
        var updateResult = await _mediator.Send(updateCommand, CancellationToken.None);

        Assert.False(updateResult.IsSuccess);
        Assert.Contains(updateResult.Errors,
            error => error.Code == "WorkOrderErrors.Readonly");
    }

    [Fact]
    public async Task Handle_WhenWorkOrderExceedsOperatingHours_ShouldFail()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        // Min30 meets minimum requirement and fits within hours at 17:00 (ends 17:30)
        var shortTask = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: RepairDurationInMinutes.Min30).Value;
        // Min75 at 17:00 ends 18:15 — outside operating hours
        var longTask = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: RepairDurationInMinutes.Min75).Value;
        var labor = EmployeeFactory.CreateEmployee().Value;
        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(17);

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.RepairTasks.AddAsync(shortTask, CancellationToken.None);
        await _context.RepairTasks.AddAsync(longTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(shortTask);
        TrackEntity(longTask);
        TrackEntity(labor);

        var createCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle.Id,
            StartsAt: scheduledAt,
            RepairTaskIds: [shortTask.Id],
            LaborId: labor.Id);
        var createResult = await _mediator.Send(createCommand, CancellationToken.None);
        Assert.True(createResult.IsSuccess,
            string.Join(", ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}")));

        var updateCommand = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: createResult.Value.WorkOrderId,
            RepairTaskIds: [longTask.Id]);
        var updateResult = await _mediator.Send(updateCommand, CancellationToken.None);

        Assert.False(updateResult.IsSuccess);
        // error code comes from the handler directly
        Assert.Contains(updateResult.Errors,
            error => error.Code == "WorkOrder_Outside_OperatingHours");
    }

    [Fact]
    public async Task Handle_WhenWorkOrderExceedsSpotAvailability_ShouldFail()
    {
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;

        var task60 = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;
        var task45 = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: RepairDurationInMinutes.Min45).Value;
        var labor = EmployeeFactory.CreateEmployee().Value;

        var start1400 = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(14);
        var start1530 = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(15).AddMinutes(30);

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle1, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle2, CancellationToken.None);
        await _context.RepairTasks.AddAsync(task60, CancellationToken.None);
        await _context.RepairTasks.AddAsync(task45, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle1);
        TrackEntity(vehicle2);
        TrackEntity(task60);
        TrackEntity(task45);
        TrackEntity(labor);

        var createCommand1 = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle1.Id,
            StartsAt: start1400,
            RepairTaskIds: [task60.Id],
            LaborId: labor.Id);
        var createResult1 = await _mediator.Send(createCommand1, CancellationToken.None);
        Assert.True(createResult1.IsSuccess,
            string.Join(", ", createResult1.Errors.Select(e => $"{e.Code}: {e.Description}")));

        var createCommand2 = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle2.Id,
            StartsAt: start1530,
            RepairTaskIds: [task60.Id],
            LaborId: labor.Id);
        var createResult2 = await _mediator.Send(createCommand2, CancellationToken.None);
        Assert.True(createResult2.IsSuccess,
            string.Join(", ", createResult2.Errors.Select(e => $"{e.Code}: {e.Description}")));

        var updateCommand = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: createResult1.Value.WorkOrderId,
            RepairTaskIds: [task60.Id, task45.Id]);
        var updateResult = await _mediator.Send(updateCommand, CancellationToken.None);

        Assert.False(updateResult.IsSuccess);
        Assert.Contains(updateResult.Errors,
            error => error.Code == "MechanicShop_Spot_Full");
    }

    [Fact]
    public async Task Handle_WhenWorkOrderExceedsLaborAvailability_ShouldFail()
    {
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;

        var task60 = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;
        var task45 = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: RepairDurationInMinutes.Min45).Value;
        var labor = EmployeeFactory.CreateEmployee().Value;

        var start1400 = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(14);
        var start1530 = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(15).AddMinutes(30);

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle1, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle2, CancellationToken.None);
        await _context.RepairTasks.AddAsync(task60, CancellationToken.None);
        await _context.RepairTasks.AddAsync(task45, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle1);
        TrackEntity(vehicle2);
        TrackEntity(task60);
        TrackEntity(task45);
        TrackEntity(labor);

        var createCommand1 = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle1.Id,
            StartsAt: start1400,
            RepairTaskIds: [task60.Id],
            LaborId: labor.Id);
        var createResult1 = await _mediator.Send(createCommand1, CancellationToken.None);
        Assert.True(createResult1.IsSuccess,
            string.Join(", ", createResult1.Errors.Select(e => $"{e.Code}: {e.Description}")));

        var createCommand2 = new CreateWorkOrderCommand(
            Spot: Spot.B,
            VehicleId: vehicle2.Id,
            StartsAt: start1530,
            RepairTaskIds: [task60.Id],
            LaborId: labor.Id);
        var createResult2 = await _mediator.Send(createCommand2, CancellationToken.None);
        Assert.True(createResult2.IsSuccess,
            string.Join(", ", createResult2.Errors.Select(e => $"{e.Code}: {e.Description}")));

        var updateCommand = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: createResult1.Value.WorkOrderId,
            RepairTaskIds: [task60.Id, task45.Id]);
        var updateResult = await _mediator.Send(updateCommand, CancellationToken.None);

        Assert.False(updateResult.IsSuccess);
        Assert.Contains(updateResult.Errors,
            e => e.Code == "Employee.LaborOccupied");
    }

    [Fact]
    public async Task Handle_WithDuplicateRepairTaskIds_ShouldFail()
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

        var createCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle.Id,
            StartsAt: DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10),
            RepairTaskIds: [repairTask.Id],
            LaborId: labor.Id);
        var createResult = await _mediator.Send(createCommand, CancellationToken.None);
        Assert.True(createResult.IsSuccess,
            string.Join(", ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}")));

        var updateCommand = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: createResult.Value.WorkOrderId,
            RepairTaskIds: [repairTask.Id, repairTask.Id]);
        var updateResult = await _mediator.Send(updateCommand, CancellationToken.None);

        Assert.False(updateResult.IsSuccess);
        Assert.Contains(updateResult.Errors,
            e => e.Code == "WorkOrderErrors.RepairTaskAlreadyAdded");
    }
}