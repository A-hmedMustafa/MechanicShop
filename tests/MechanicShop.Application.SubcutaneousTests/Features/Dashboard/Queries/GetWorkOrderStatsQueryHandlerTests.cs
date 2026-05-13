using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Dashboard.Queries;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetWorkOrderStatsQueryHandlerTests: SubcutaneousTestBase
{
    public GetWorkOrderStatsQueryHandlerTests(WebAppFactory factory) : base(factory) { }

       private async Task<Guid> SeedWorkOrderAsync(Spot spot, Guid vehicleId, Guid laborId, Guid[] repairTaskIds, DateTimeOffset startAt)
    {
        var createWorkOrderCommand = new CreateWorkOrderCommand(spot, vehicleId, startAt, repairTaskIds.ToList(), laborId);
        var createWorkOrderCommandResult = await _mediator.Send(createWorkOrderCommand, CancellationToken.None);
        return createWorkOrderCommandResult.Value.WorkOrderId;
    }

    private async Task<Invoice> CompleteAndInvoiceWorkOrderAsync(Guid workOrderId)
    {
        await _mediator.Send(new UpdateWorkOrderStateCommand(workOrderId, WorkOrderState.InProgress), CancellationToken.None);
        await _mediator.Send(new UpdateWorkOrderStateCommand(workOrderId, WorkOrderState.Completed), CancellationToken.None);

        var invoiceCommand = new IssueInvoiceCommand(workOrderId);
        var invoiceResult = await _mediator.Send(invoiceCommand, CancellationToken.None);

        var invoice = await _context.Invoices.FindAsync(invoiceResult.Value.InvoiceId);
        TrackEntity(invoice!);
        return invoice!;
    }

    [Fact]
    public async Task Handle_WhenNoWorkOrdersOnDate_ShouldReturnZeroStats()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)); 

        var getWorkOrderStatsQuery = new GetWorkOrderStatsQuery(date);

        var getWorkOrderStatsQueryResult = await _mediator.Send(getWorkOrderStatsQuery, CancellationToken.None);

        Assert.True(getWorkOrderStatsQueryResult.IsSuccess);
        var dto = getWorkOrderStatsQueryResult.Value;
        Assert.Equal(date, dto.Date);
        Assert.Equal(0, dto.Total);
        Assert.Equal(0, dto.Scheduled);
        Assert.Equal(0, dto.Completed);
        Assert.Equal(0, dto.TotalRevenue);
        Assert.Equal(0, dto.ProfitMargin);
    }

    [Fact]
    public async Task Handle_WithWorkOrders_ShouldReturnCorrectStats()
    {
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var labor = EmployeeFactory.CreateEmployee().Value;
        var baseDate = DateTimeOffset.UtcNow.Date.AddDays(2); 
        var startMorning = baseDate.AddHours(9);   
        var startAfternoon = baseDate.AddHours(14);

        await _context.Vehicles.AddAsync(vehicle1, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle2, CancellationToken.None);
        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(vehicle1);
        TrackEntity(vehicle2);
        TrackEntity(customer);
        TrackEntity(repairTask);
        TrackEntity(labor);


        var workOrderId1 = await SeedWorkOrderAsync(Spot.A, vehicle1.Id, labor.Id, [repairTask.Id], startAfternoon);

        
        var workOrderId2 = await SeedWorkOrderAsync(Spot.B, vehicle2.Id, labor.Id, [repairTask.Id], startMorning);
        var invoice = await CompleteAndInvoiceWorkOrderAsync(workOrderId2); 

        var date = DateOnly.FromDateTime(baseDate.Date);

        var getWorkOrderStatsQuery = new GetWorkOrderStatsQuery(date);

        var getWorkOrderStatsQueryResult = await _mediator.Send(getWorkOrderStatsQuery, CancellationToken.None);

        Assert.True(getWorkOrderStatsQueryResult.IsSuccess);
        var dto = getWorkOrderStatsQueryResult.Value;
        Assert.Equal(date, dto.Date);
        Assert.Equal(2, dto.Total);
        Assert.Equal(1, dto.Scheduled);    
        Assert.Equal(1, dto.Completed);     
        Assert.Equal(0, dto.InProgress);
        Assert.Equal(0, dto.Cancelled);

      
        Assert.True(dto.TotalRevenue > 0);
        Assert.True(dto.TotalPartsCost > 0);
        Assert.True(dto.TotalLaborCost > 0);

   
        Assert.Equal(2, dto.UniqueVehicles);  
        Assert.Equal(1, dto.UniqueCustomers); 

        
        Assert.Equal(50.0m, dto.CompletionRate); 
        Assert.True(dto.ProfitMargin > 0 && dto.ProfitMargin <= 100); 
        Assert.True(dto.AverageRevenuePerOrder > 0);
        Assert.True(dto.OrdersPerVehicle == 1.0m); 
    }
}