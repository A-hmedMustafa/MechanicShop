using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.Billing.Commands.SettleInvoice;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.SettleInvoice;

[Collection(WebAppFactoryCollection.CollectionName)]
public class SettleInvoiceCommandHandlerTests: SubcutaneousTestBase{
    public SettleInvoiceCommandHandlerTests(WebAppFactory factory) : base(factory) { }

  private async Task<Invoice> CreateIssuedInvoiceAsync()
    {
        
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var labor = EmployeeFactory.CreateEmployee().Value;
        var tomorrow = DateTimeOffset.UtcNow.Date.AddDays(1);
        var futureStart = new DateTimeOffset(
            tomorrow.Year, tomorrow.Month, tomorrow.Day, 10, 0, 0, TimeSpan.Zero);
        await _context.Customers.AddAsync(customer, CancellationToken.None);
        // await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        
        TrackEntity(vehicle);
        TrackEntity(customer);
        TrackEntity(repairTask);
        TrackEntity(labor);

      var createCommand = new CreateWorkOrderCommand(
            Spot.A, vehicle.Id, futureStart, [repairTask.Id], labor.Id);
        var createResult = await _mediator.Send(createCommand, CancellationToken.None);
        Assert.True(createResult.IsSuccess,
            string.Join(", ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}")));

        var workOrderId = createResult.Value.WorkOrderId;

        await _mediator.Send(
            new UpdateWorkOrderStateCommand(workOrderId, WorkOrderState.InProgress),
            CancellationToken.None);

        var completedResult = await _mediator.Send(
            new UpdateWorkOrderStateCommand(workOrderId, WorkOrderState.Completed),
            CancellationToken.None);
        Assert.True(completedResult.IsSuccess,
            string.Join(", ", completedResult.Errors.Select(e => $"{e.Code}: {e.Description}")));

        var issueResult = await _mediator.Send(
            new IssueInvoiceCommand(workOrderId), CancellationToken.None);
        Assert.True(issueResult.IsSuccess,
            string.Join(", ", issueResult.Errors.Select(e => $"{e.Code}: {e.Description}")));

        var invoice = await _context.Invoices.FindAsync(issueResult.Value.InvoiceId);
        TrackEntity(invoice!);
        return invoice!;
    }
    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
        var invoice = await CreateIssuedInvoiceAsync();

        var settleInvoiceCommand = new SettleInvoiceCommand(invoice.Id);
        var settleInvoiceCommandResult = await _mediator.Send(settleInvoiceCommand, CancellationToken.None);

        Assert.True(settleInvoiceCommandResult.IsSuccess);
    }

    [Fact]
    public async Task Handle_WithMissingInvoice_ShouldFail()
    {
        var settleInvoiceCommand = new SettleInvoiceCommand(Guid.NewGuid());

        var settleInvoiceCommandResult = await _mediator.Send(settleInvoiceCommand, CancellationToken.None);

        Assert.False(settleInvoiceCommandResult.IsSuccess);
        Assert.Contains(settleInvoiceCommandResult.Errors, e => e.Code == "ApplicationErrors.Invoice.NotFound");
    }

    [Fact]
    public async Task Handle_WithAlreadyPaidInvoice_ShouldFail()
    {
        var invoice = await CreateIssuedInvoiceAsync();

     
        var firstSettle = new SettleInvoiceCommand(invoice.Id);
        var firstResult = await _mediator.Send(firstSettle, CancellationToken.None);
        Assert.True(firstResult.IsSuccess);

       
        var secondSettle = new SettleInvoiceCommand(invoice.Id);
        var secondResult = await _mediator.Send(secondSettle, CancellationToken.None);

        Assert.False(secondResult.IsSuccess);
        Assert.Contains(secondResult.Errors, e => e.Code == "Invoice.Locked");
    }
}