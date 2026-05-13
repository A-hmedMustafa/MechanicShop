using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries.GetInvoiceById;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetInvoiceByIdQueryHandlerTests: SubcutaneousTestBase
{
    public GetInvoiceByIdQueryHandlerTests(WebAppFactory factory) : base(factory) { }   
   
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
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
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
    public async Task Handle_WithExistingInvoice_ShouldReturnDto()
    {
        var invoice = await CreateIssuedInvoiceAsync();

        var result = await _mediator.Send(
            new GetInvoiceByIdQuery(invoice.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(invoice.Id, result.Value.InvoiceId);
        Assert.NotEmpty(result.Value.Items);
    }
    [Fact]
    public async Task Handle_WithMissingInvoice_ShouldFail()
    {
        var getInvoiceByIdQuery = new GetInvoiceByIdQuery(Guid.NewGuid());

        var getInvoiceByIdQueryResult = await _mediator.Send(getInvoiceByIdQuery, CancellationToken.None);

        Assert.False(getInvoiceByIdQueryResult.IsSuccess);
        Assert.Contains(getInvoiceByIdQueryResult.Errors, e => e.Code == "Invoice_Not_Found");
    }
}