using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.IssueInvoice;

[Collection(WebAppFactoryCollection.CollectionName)]
public class IssueInvoiceCommandHandlerTests: SubcutaneousTestBase
{
    public IssueInvoiceCommandHandlerTests(WebAppFactory factory) : base(factory) { }

    private async Task<Guid> CreateCompletedWorkOrderAsync(
        Spot spot, Guid vehicleId, Guid laborId, Guid[] repairTaskIds, DateTimeOffset startAt)
    {
        var createCommand = new CreateWorkOrderCommand(
            spot, vehicleId, startAt, repairTaskIds.ToList(), laborId);

        var createResult = await _mediator.Send(createCommand, CancellationToken.None);
        Assert.True(createResult.IsSuccess, string.Join(", ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}")));
        
        var workOrderId = createResult.Value.WorkOrderId;

        var inProgressCommand = new UpdateWorkOrderStateCommand(workOrderId, WorkOrderState.InProgress);
        await _mediator.Send(inProgressCommand, CancellationToken.None);

        var completedCommand = new UpdateWorkOrderStateCommand(workOrderId, WorkOrderState.Completed);
        
        var completedResult = await _mediator.Send(completedCommand, CancellationToken.None);
        Assert.True(completedResult.IsSuccess,
            string.Join(", ", completedResult.Errors.Select(e => $"{e.Code}: {e.Description}")));

        return workOrderId;
    }
    
    [Fact]
    public async Task Handle_WithCompletedWorkOrder_ShouldSucceed()
    {
       
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var labor = EmployeeFactory.CreateEmployee().Value;
        var start = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10); 

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        // await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(repairTask);
        TrackEntity(labor);

        var workOrderId = await CreateCompletedWorkOrderAsync(Spot.A, vehicle.Id, labor.Id, [repairTask.Id], start);

        var issueInvoiceCommand = new IssueInvoiceCommand(WorkOrderId: workOrderId);

       
        var issueInvoiceCommandResult = await _mediator.Send(issueInvoiceCommand, CancellationToken.None);

        Assert.True(issueInvoiceCommandResult.IsSuccess);
        Assert.NotNull(issueInvoiceCommandResult.Value);
        Assert.Equal(workOrderId, issueInvoiceCommandResult.Value.WorkOrderId);
         await _context.Invoices.Where(inv => inv.Id == issueInvoiceCommandResult.Value.InvoiceId)
            .ExecuteDeleteAsync();
    }

  
    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ShouldFail()
    {
        var issueInvoiceCommand = new IssueInvoiceCommand(WorkOrderId: Guid.NewGuid());

        var issueInvoiceCommandResult = await _mediator.Send(issueInvoiceCommand, CancellationToken.None);

        Assert.False(issueInvoiceCommandResult.IsSuccess);
        Assert.Contains(issueInvoiceCommandResult.Errors, e => e.Code == "ApplicationErrors.WorkOrders.NotFound");
    }

  
    [Fact]
    public async Task Handle_WhenWorkOrderNotCompleted_ShouldFail()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var labor = EmployeeFactory.CreateEmployee().Value;
        var start = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10); 

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        // await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(repairTask);
        TrackEntity(labor);

        var createWorkOrderCommand = new CreateWorkOrderCommand(Spot.A, vehicle.Id, start, [repairTask.Id], labor.Id);
        var createWorkOrderCommandResult = await _mediator.Send(createWorkOrderCommand, CancellationToken.None);
        var workOrderId = createWorkOrderCommandResult.Value.WorkOrderId;

        var issueInvoiceCommand = new IssueInvoiceCommand(WorkOrderId: workOrderId);

     
        var issueInvoiceCommandResult = await _mediator.Send(issueInvoiceCommand, CancellationToken.None);

     
        Assert.False(issueInvoiceCommandResult.IsSuccess);
        Assert.Contains(issueInvoiceCommandResult.Errors, e => e.Code == "WorkOrder.InvoiceIssuance.InvalidState");
    }
}