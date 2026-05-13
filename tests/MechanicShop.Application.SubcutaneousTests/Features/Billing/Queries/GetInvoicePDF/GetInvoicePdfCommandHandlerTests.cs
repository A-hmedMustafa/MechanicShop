using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Billing;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries.GetInvoicePDF;

public class GetInvoicePdfQueryHandlerTests : SubcutaneousTestBase
{
    public GetInvoicePdfQueryHandlerTests(WebAppFactory factory) : base(factory) { }
 private async Task<Invoice> SeedInvoiceAsync()
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

        var invoice = await _context.Invoices
            .Include(i => i.LineItems)
            .FirstAsync(i => i.Id == issueResult.Value.InvoiceId);
        TrackEntity(invoice);
        return invoice;
    }

    [Fact]
    public async Task Handle_WhenInvoiceExists_ShouldReturnPdf()
    {
        var invoice = await SeedInvoiceAsync();

        var result = await _mediator.Send(
            new GetInvoicePdfQuery(invoice.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.Content);
        Assert.Equal($"invoice_{invoice.Id}.pdf", result.Value.FileName);
    }

    [Fact]
    public async Task Handle_WhenInvoiceNotFound_ShouldFail()
    {
        var result = await _mediator.Send(
            new GetInvoicePdfQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "Invoice_Not_Found");
    }

    [Fact]
    public async Task Handle_WhenPdfGeneratorFails_ShouldFail()
    {
        var throwingGenerator = Substitute.For<IInvoicePdfGenerator>();
        throwingGenerator.Generate(Arg.Any<Invoice>()).Throws(new Exception("Pdf Crashed"));

        var handler = new GetInvoicePdfQueryHandler(
            Substitute.For<ILogger<GetInvoicePdfQueryHandler>>(),
            throwingGenerator,
            _context);

        var invoice = await SeedInvoiceAsync();

        var result = await handler.Handle(
            new GetInvoicePdfQuery(invoice.Id), CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorKind.Failure, result.TopError.Kind);
    }}