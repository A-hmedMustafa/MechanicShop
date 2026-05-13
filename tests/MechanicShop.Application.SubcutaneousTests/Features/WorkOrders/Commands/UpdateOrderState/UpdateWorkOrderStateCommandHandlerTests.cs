using Docker.DotNet.Models;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Emloyees;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateOrderState;

// [Collection(WebAppFactoryCollection.CollectionName)]
// public class UpdateWorkOrderStateCommandHandlerTests: SubcutaneousTestBase
// {
//     public UpdateWorkOrderStateCommandHandlerTests(WebAppFactory factory) : base(factory) { }

//     [Fact]
//     public async Task Handle_WithValidData_ShouldSucceed()
//     {
//         var workOrder = WorkOrderFactory.CreateWorkOrder(startsAt: DateTimeOffset.UtcNow.AddMinutes(-10)).Value;
        

//         await _context.WorkOrders.AddAsync(workOrder, CancellationToken.None);
//         await _context.SaveChangesAsync(CancellationToken.None);

//         var statusUpdateCommand = new UpdateWorkOrderStateCommand(
//             WorkOrderId: workOrder.Id,
//             State: WorkOrderState.InProgress
//         );

//         var statusUpdateCommandResult = await _mediator.Send(statusUpdateCommand, CancellationToken.None);

//         Assert.True(statusUpdateCommandResult.IsSuccess);
//     }

//     [Fact]
//     public async Task Handle_WithMissingWorkOrder_ShouldFail()
//     {
//         var statusUpdateCommand = new UpdateWorkOrderStateCommand(
//             WorkOrderId: Guid.Empty,
//             State: WorkOrderState.InProgress
//         );

//         var statusUpdateCommandResult = await _mediator.Send(statusUpdateCommand, CancellationToken.None);

//         Assert.False(statusUpdateCommandResult.IsSuccess);
//         Assert.Contains(statusUpdateCommandResult.Errors, error => error.Code == "ApplicationErrors.WorkOrders.NotFound");

//     }

//     [Fact]
//     public async Task Handle_WhenTransitionNotAllowed_ShouldFail()
//     {
//         var workOrder = WorkOrderFactory.CreateWorkOrder(startsAt: DateTimeOffset.UtcNow.AddHours(1)).Value;
        

//         await _context.WorkOrders.AddAsync(workOrder, CancellationToken.None);
//         await _context.SaveChangesAsync(CancellationToken.None);

//         var statusUpdateCommand = new UpdateWorkOrderStateCommand(
//             WorkOrderId: workOrder.Id,
//             State: WorkOrderState.InProgress
//         );

//         var statusUpdateCommandResult = await _mediator.Send(statusUpdateCommand, CancellationToken.None);

//         Assert.False(statusUpdateCommandResult.IsSuccess);
//         Assert.Contains(statusUpdateCommandResult.Errors, error => error.Code == "WorkOrderErrors.StateTransitionNotAllowed");

//     }

//     [Fact]
//     public async Task Handle_WhenInvalidTransition_ShouldFail()
//     {
//         var workOrder = WorkOrderFactory.CreateWorkOrder(startsAt: DateTimeOffset.UtcNow.AddMinutes(-10)).Value;
        

//         await _context.WorkOrders.AddAsync(workOrder, CancellationToken.None);
//         await _context.SaveChangesAsync(CancellationToken.None);

//         var statusUpdateCommand = new UpdateWorkOrderStateCommand(
//             WorkOrderId: workOrder.Id,
//             State: WorkOrderState.Completed
//         );

//         var statusUpdateCommandResult = await _mediator.Send(statusUpdateCommand, CancellationToken.None);

//         Assert.False(statusUpdateCommandResult.IsSuccess);
//         Assert.Contains(statusUpdateCommandResult.Errors, error => error.Code == "WorkOrderErrors.InvalidStateTransition");
//     }

// }
public class UpdateWorkOrderStateCommandHandlerTests : SubcutaneousTestBase
{
    public UpdateWorkOrderStateCommandHandlerTests(WebAppFactory factory) : base(factory) { }

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

    // Creates work order directly in DB bypassing validator
    // Use this when you need a past start time
    private async Task<WorkOrder> SeedWorkOrderWithPastStartAsync(
        Guid vehicleId,
        Guid laborId,
        List<RepairTask> repairTasks)
    {
        var yesterday = DateTimeOffset.UtcNow.Date.AddDays(-1);
        var pastStart = new DateTimeOffset(
            yesterday.Year, yesterday.Month, yesterday.Day,
            10, 0, 0, TimeSpan.Zero);

        var duration = TimeSpan.FromMinutes(
            repairTasks.Sum(rt => (int)rt.EstimatedDurationInMins));

        var workOrder = WorkOrder.Create(
            Guid.NewGuid(),
            vehicleId,
            pastStart,
            pastStart.Add(duration),
            laborId,
            Spot.A,
            repairTasks).Value;

        await _context.WorkOrders.AddAsync(workOrder, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(workOrder);

        return workOrder;
    }

    // tomorrow at 10am UTC — always in the future, always within operating hours
    private static DateTimeOffset FutureStart()
    {
        var tomorrow = DateTimeOffset.UtcNow.Date.AddDays(1);
        return new DateTimeOffset(
            tomorrow.Year, tomorrow.Month, tomorrow.Day,
            10, 0, 0, TimeSpan.Zero);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
        var (_, vehicle, labor, repairTask) = await SeedReferenceDataAsync();

        // bypass validator — directly seed with past start
        var workOrder = await SeedWorkOrderWithPastStartAsync(
            vehicle.Id, labor.Id, [repairTask]);

        var statusUpdateCommand = new UpdateWorkOrderStateCommand(
            workOrder.Id, WorkOrderState.InProgress);

        var result = await _mediator.Send(statusUpdateCommand, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WithMissingWorkOrder_ShouldFail()
    {
        var statusUpdateCommand = new UpdateWorkOrderStateCommand(
            Guid.NewGuid(), WorkOrderState.InProgress);

        var result = await _mediator.Send(statusUpdateCommand, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.WorkOrders.NotFound");
    }

   [Fact]
public async Task Handle_WhenTransitionNotAllowed_ShouldFail()
{
    var (_, vehicle, labor, repairTask) = await SeedReferenceDataAsync();

    // use mediator — future start is valid for creation
    var createCommand = new CreateWorkOrderCommand(
        Spot.A, vehicle.Id, FutureStart(), [repairTask.Id], labor.Id);

    var createResult = await _mediator.Send(createCommand, CancellationToken.None);
    Assert.True(createResult.IsSuccess,
        string.Join(", ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}")));

    // handler blocks Cancelled for future work orders
    var statusUpdateCommand = new UpdateWorkOrderStateCommand(
        createResult.Value.WorkOrderId, WorkOrderState.Cancelled);

    var result = await _mediator.Send(statusUpdateCommand, CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Code == "WorkOrderErrors.StateTransitionNotAllowed");
}

    [Fact]
    public async Task Handle_WhenInvalidTransition_ShouldFail()
    {
        var (_, vehicle, labor, repairTask) = await SeedReferenceDataAsync();

        // bypass validator — directly seed with past start
        var workOrder = await SeedWorkOrderWithPastStartAsync(
            vehicle.Id, labor.Id, [repairTask]);

        // Scheduled → Completed is invalid transition
        var statusUpdateCommand = new UpdateWorkOrderStateCommand(
            workOrder.Id, WorkOrderState.Completed);

        var result = await _mediator.Send(statusUpdateCommand, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrderErrors.InvalidStateTransition");
    }
}