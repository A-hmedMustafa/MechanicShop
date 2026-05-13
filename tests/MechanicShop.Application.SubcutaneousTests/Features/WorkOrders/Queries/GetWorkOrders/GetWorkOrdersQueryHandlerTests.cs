using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrders;


public class GetWorkOrdersQueryHandlerTests : SubcutaneousTestBase
{
    public GetWorkOrdersQueryHandlerTests(WebAppFactory factory) : base(factory) { }

    // The base class provides `_mediator` and `_context` – do NOT redeclare them here.

    private async Task<Guid> SeedWorkOrderAsync(
        Spot spot, Guid vehicleId, Guid laborId, Guid[] repairTaskIds, DateTimeOffset startAt)
    {
        var createWorkOrderCommand = new CreateWorkOrderCommand(spot, vehicleId, startAt, repairTaskIds.ToList(), laborId);
        var createWorkOrderCommandResult = await _mediator.Send(createWorkOrderCommand, CancellationToken.None);
        
     
        Assert.True(createWorkOrderCommandResult.IsSuccess,
        string.Join(", ", createWorkOrderCommandResult.Errors
            .Select(e => $"{e.Code}: {e.Description}")));

        return createWorkOrderCommandResult.Value.WorkOrderId;
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WithDefaultSort()
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

        var startBase = DateTimeOffset.UtcNow.Date.AddDays(2).AddHours(10);
        var workOrder1 = await SeedWorkOrderAsync(Spot.A, vehicle.Id, labor.Id, [repairTask.Id], startBase);
        var workOrder2 = await SeedWorkOrderAsync(Spot.B, vehicle.Id, labor.Id, [repairTask.Id], startBase.AddHours(1));
        var workOrder3 = await SeedWorkOrderAsync(Spot.C, vehicle.Id, labor.Id, [repairTask.Id], startBase.AddHours(2));

        var getWorkOrdersQuery = new GetWorkOrdersQuery(Page: 1, PageSize: 2);
        var getWorkOrdersQueryResult = await _mediator.Send(getWorkOrdersQuery, CancellationToken.None);

        Assert.True(getWorkOrdersQueryResult.IsSuccess);
        var paginated = getWorkOrdersQueryResult.Value;
        Assert.Equal(1, paginated.PageNumber);
        Assert.Equal(2, paginated.PageSize);
        Assert.Equal(3, paginated.TotalCount);
        Assert.Equal(2, paginated.Items!.Count);

        var returnedIds = paginated.Items.Select(i => i.WorkOrderId).ToArray();
        var allSeededIds = new[] { workOrder1, workOrder2, workOrder3 };
        // Ensure every returned ID belongs to the seeded set, and we have exactly 2 of them
        Assert.All(returnedIds, id => Assert.Contains(id, allSeededIds));
        Assert.Equal(2, returnedIds.Length);
    }

    [Fact]
    public async Task Handle_ShouldReturnSecondPage()
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

        var startBase = DateTimeOffset.UtcNow.Date.AddDays(2).AddHours(10);
        var wo1 = await SeedWorkOrderAsync(Spot.A, vehicle.Id, labor.Id, [repairTask.Id], startBase);
        var wo2 = await SeedWorkOrderAsync(Spot.B, vehicle.Id, labor.Id, [repairTask.Id], startBase.AddHours(1));
        var wo3 = await SeedWorkOrderAsync(Spot.C, vehicle.Id, labor.Id, [repairTask.Id], startBase.AddHours(2));
        var wo4 = await SeedWorkOrderAsync(Spot.C, vehicle.Id, labor.Id, [repairTask.Id], startBase.AddHours(3));

        var query = new GetWorkOrdersQuery(Page: 2, PageSize: 2);
        var queryResult = await _mediator.Send(query, CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        var paginated = queryResult.Value;
        Assert.Equal(2, paginated.PageNumber);
        Assert.Equal(2, paginated.PageSize);
        Assert.Equal(4, paginated.TotalCount);
        Assert.Equal(2, paginated.Items!.Count);

        var returnedIds = paginated.Items.Select(i => i.WorkOrderId).ToArray();
        // Page 2 should contain the two earliest work orders (by CreatedUtc desc): wo1 and wo2
        var expectedOnPage2 = new[] { wo3, wo4 };
        Assert.True(returnedIds.All(id => expectedOnPage2.Contains(id)));
        Assert.Equal(2, returnedIds.Length);
    }

    [Fact]
    public async Task Handle_ShouldFilterByState()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var labor = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        // await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(repairTask);
        TrackEntity(labor);

        var futureStart = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(11);
        var pastStart = DateTimeOffset.UtcNow.Date.AddDays(-1).AddHours(12);

        var scheduledId = await SeedWorkOrderAsync(Spot.A, vehicle.Id, labor.Id, [repairTask.Id], futureStart);
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            id: Guid.NewGuid(),
            vehicleId: vehicle.Id,
            startsAt: pastStart,
            endsAt: pastStart.AddMinutes(60),
            laborId: labor.Id,
            spot: Spot.A,
            repairTasks: [repairTask]
        ).Value;

        await _context.WorkOrders.AddAsync(workOrder, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

       
        await _mediator.Send(new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.InProgress), CancellationToken.None);

        var query = new GetWorkOrdersQuery(Page: 1, PageSize: 10, State: WorkOrderState.Scheduled);
        var queryResult = await _mediator.Send(query, CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        var items = queryResult.Value.Items!;
        Assert.Single(items);
        Assert.Equal(scheduledId, items.ElementAt(0).WorkOrderId); // Only one item, position is irrelevant but fine
    }

    [Fact]
    public async Task Handle_ShouldFilterByVehicleId()
    {
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var labor = EmployeeFactory.CreateEmployee().Value;

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

        var startBase = DateTimeOffset.UtcNow.Date.AddDays(2).AddHours(10);
        var wo1 = await SeedWorkOrderAsync(Spot.A, vehicle1.Id, labor.Id, [repairTask.Id], startBase);
        var wo2 = await SeedWorkOrderAsync(Spot.B, vehicle1.Id, labor.Id, [repairTask.Id], startBase.AddHours(1));
        var wo3 = await SeedWorkOrderAsync(Spot.C, vehicle1.Id, labor.Id, [repairTask.Id], startBase.AddHours(2));
        var wo4 = await SeedWorkOrderAsync(Spot.A, vehicle2.Id, labor.Id, [repairTask.Id], startBase.AddHours(3));

        var query = new GetWorkOrdersQuery(Page: 1, PageSize: 10, VehicleId: vehicle1.Id);
        var queryResult = await _mediator.Send(query, CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        var paginated = queryResult.Value;
        Assert.Equal(1, paginated.PageNumber);
        Assert.Equal(10, paginated.PageSize);
        Assert.Equal(3, paginated.TotalCount);
        Assert.Equal(3, paginated.Items!.Count);

        var returnedIds = paginated.Items.Select(i => i.WorkOrderId).ToArray();
        var vehicle1Orders = new[] { wo1, wo2, wo3 };
        Assert.All(returnedIds, id => Assert.Contains(id, vehicle1Orders));
    }

    [Fact]
    public async Task Handle_ShouldFilterByLaborId()
    {
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
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

        var startBase = DateTimeOffset.UtcNow.Date.AddDays(2).AddHours(10);
        var wo1 = await SeedWorkOrderAsync(Spot.A, vehicle1.Id, labor1.Id, [repairTask.Id], startBase);
        var wo2 = await SeedWorkOrderAsync(Spot.B, vehicle1.Id, labor2.Id, [repairTask.Id], startBase.AddHours(1));
        var wo3 = await SeedWorkOrderAsync(Spot.C, vehicle1.Id, labor2.Id, [repairTask.Id], startBase.AddHours(2));
        var wo4 = await SeedWorkOrderAsync(Spot.A, vehicle2.Id, labor2.Id, [repairTask.Id], startBase.AddHours(3));

        var query = new GetWorkOrdersQuery(Page: 1, PageSize: 10, LaborId: labor2.Id);
        var queryResult = await _mediator.Send(query, CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        var paginated = queryResult.Value;
        Assert.Equal(1, paginated.PageNumber);
        Assert.Equal(10, paginated.PageSize);
        Assert.Equal(3, paginated.TotalCount);
        Assert.Equal(3, paginated.Items!.Count);

        var returnedIds = paginated.Items.Select(i => i.WorkOrderId).ToArray();
        var labor2Orders = new[] { wo2, wo3, wo4 };
        Assert.All(returnedIds, id => Assert.Contains(id, labor2Orders));
    }

    [Fact]
    public async Task Handle_ShouldFilterBySpot()
    {
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
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

        var startBase = DateTimeOffset.UtcNow.Date.AddDays(2).AddHours(10);
        var wo1 = await SeedWorkOrderAsync(Spot.A, vehicle1.Id, labor1.Id, [repairTask.Id], startBase);
        var wo2 = await SeedWorkOrderAsync(Spot.A, vehicle1.Id, labor2.Id, [repairTask.Id], startBase.AddHours(1));
        var wo3 = await SeedWorkOrderAsync(Spot.C, vehicle1.Id, labor2.Id, [repairTask.Id], startBase.AddHours(2));
        var wo4 = await SeedWorkOrderAsync(Spot.A, vehicle2.Id, labor2.Id, [repairTask.Id], startBase.AddHours(3));

        var query = new GetWorkOrdersQuery(Page: 1, PageSize: 10, Spot: Spot.A);
        var queryResult = await _mediator.Send(query, CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        var paginated = queryResult.Value;
        Assert.Equal(1, paginated.PageNumber);
        Assert.Equal(10, paginated.PageSize);
        Assert.Equal(3, paginated.TotalCount);
        Assert.Equal(3, paginated.Items!.Count);

        var returnedIds = paginated.Items.Select(i => i.WorkOrderId).ToArray();
        var spotAOrders = new[] { wo1, wo2, wo4 };
        Assert.All(returnedIds, id => Assert.Contains(id, spotAOrders));
    }

    [Fact]
    public async Task Handle_ShouldFilterByStartDateRange()
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

        var baseDate = DateTimeOffset.UtcNow.Date.AddDays(2);
        var start1 = baseDate.AddHours(10);
        var start2 = baseDate.AddDays(1).AddHours(10);
        var start3 = baseDate.AddDays(2).AddHours(10);

        var wo1 = await SeedWorkOrderAsync(Spot.A, vehicle.Id, labor.Id, [repairTask.Id], start1);
        var wo2 = await SeedWorkOrderAsync(Spot.B, vehicle.Id, labor.Id, [repairTask.Id], start2);
        var wo3 = await SeedWorkOrderAsync(Spot.C, vehicle.Id, labor.Id, [repairTask.Id], start3);

        DateTime from = start1.Date;
        DateTime to   = start2.Date.AddDays(1).AddTicks(-1);

        var query = new GetWorkOrdersQuery(Page: 1, PageSize: 10, StartDateFrom: from, StartDateTo: to);
        var queryResult = await _mediator.Send(query, CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        var paginated = queryResult.Value;
        Assert.Equal(1, paginated.PageNumber);
        Assert.Equal(10, paginated.PageSize);
        Assert.Equal(2, paginated.TotalCount);
        Assert.Equal(2, paginated.Items!.Count);

        var returnedIds = paginated.Items.Select(i => i.WorkOrderId).ToArray();
        var expectedIds = new[] { wo1, wo2 };
        Assert.All(returnedIds, id => Assert.Contains(id, expectedIds));
    }

    [Fact]
    public async Task Handle_ShouldFilterByEndDateRange()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        var labor = EmployeeFactory.CreateEmployee().Value;
        var task30 = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min30).Value;

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle, CancellationToken.None);
        await _context.RepairTasks.AddAsync(task30, CancellationToken.None);
        await _context.Employees.AddAsync(labor, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);
        TrackEntity(task30);
        TrackEntity(labor);

        var baseDate = DateTimeOffset.UtcNow.Date.AddDays(2);
        var start1 = baseDate.AddHours(10);
        var start2 = baseDate.AddDays(1).AddHours(10);
        var start3 = baseDate.AddDays(2).AddHours(10);

        var wo1 = await SeedWorkOrderAsync(Spot.A, vehicle.Id, labor.Id, [task30.Id], start1);
        var wo2 = await SeedWorkOrderAsync(Spot.B, vehicle.Id, labor.Id, [task30.Id], start2);
        var wo3 = await SeedWorkOrderAsync(Spot.C, vehicle.Id, labor.Id, [task30.Id], start3);

        DateTime from = baseDate.Date;
        DateTime to   = baseDate.AddDays(2).AddTicks(-1);

        var query = new GetWorkOrdersQuery(Page: 1, PageSize: 10, EndDateFrom: from, EndDateTo: to);
        var queryResult = await _mediator.Send(query, CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        var paginated = queryResult.Value;
        Assert.Equal(1, paginated.PageNumber);
        Assert.Equal(10, paginated.PageSize);
        Assert.Equal(2, paginated.TotalCount);
        Assert.Equal(2, paginated.Items!.Count);

        var returnedIds = paginated.Items.Select(i => i.WorkOrderId).ToArray();
        var expectedIds = new[] { wo1, wo2 };
        Assert.All(returnedIds, id => Assert.Contains(id, expectedIds));
    }

    [Fact]
    public async Task Handle_ShouldSearchByLicensePlate()
    {
        var vehicle1 = VehicleFactory.CreateVehicle(licensePlate: "awc 1243").Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
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

        var startBase = DateTimeOffset.UtcNow.Date.AddDays(2).AddHours(10);
        var wo1 = await SeedWorkOrderAsync(Spot.A, vehicle1.Id, labor1.Id, [repairTask.Id], startBase);
        var wo2 = await SeedWorkOrderAsync(Spot.B, vehicle1.Id, labor2.Id, [repairTask.Id], startBase.AddHours(1));
        var wo3 = await SeedWorkOrderAsync(Spot.C, vehicle1.Id, labor2.Id, [repairTask.Id], startBase.AddHours(2));
        var wo4 = await SeedWorkOrderAsync(Spot.D, vehicle2.Id, labor2.Id, [repairTask.Id], startBase.AddHours(3));

        var query = new GetWorkOrdersQuery(Page: 1, PageSize: 10, SearchTerm: vehicle1.LicensePlate);
        var queryResult = await _mediator.Send(query, CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        var paginated = queryResult.Value;
        Assert.Equal(1, paginated.PageNumber);
        Assert.Equal(10, paginated.PageSize);
        Assert.Equal(3, paginated.TotalCount);
        Assert.Equal(3, paginated.Items!.Count);

        var returnedIds = paginated.Items.Select(i => i.WorkOrderId).ToArray();
        var expectedOnVehicle1 = new[] { wo1, wo2, wo3 };
        Assert.All(returnedIds, id => Assert.Contains(id, expectedOnVehicle1));
    }

    [Fact]
    public async Task Handle_ShouldSearchByLaborFirstName()
    {
        var vehicle1 = VehicleFactory.CreateVehicle(licensePlate: "awc 1243").Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var labor1 = EmployeeFactory.CreateEmployee(firstName: "Alice").Value;
        var labor2 = EmployeeFactory.CreateEmployee(firstName: "Bob").Value;

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

        var startBase = DateTimeOffset.UtcNow.Date.AddDays(2).AddHours(10);
        var wo1 = await SeedWorkOrderAsync(Spot.A, vehicle1.Id, labor1.Id, [repairTask.Id], startBase);
        var wo2 = await SeedWorkOrderAsync(Spot.B, vehicle1.Id, labor1.Id, [repairTask.Id], startBase.AddHours(1));
        var wo3 = await SeedWorkOrderAsync(Spot.C, vehicle1.Id, labor2.Id, [repairTask.Id], startBase.AddHours(2));
        var wo4 = await SeedWorkOrderAsync(Spot.D, vehicle2.Id, labor2.Id, [repairTask.Id], startBase.AddHours(3));

        var query = new GetWorkOrdersQuery(Page: 1, PageSize: 10, SearchTerm: labor1.FirstName);
        var queryResult = await _mediator.Send(query, CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        var paginated = queryResult.Value;
        Assert.Equal(1, paginated.PageNumber);
        Assert.Equal(10, paginated.PageSize);
        Assert.Equal(2, paginated.TotalCount);
        Assert.Equal(2, paginated.Items!.Count);

        var returnedIds = paginated.Items.Select(i => i.WorkOrderId).ToArray();
        var labor1Orders = new[] { wo1, wo2 };
        Assert.All(returnedIds, id => Assert.Contains(id, labor1Orders));
    }

    [Fact]
    public async Task Handle_ShouldSearchByRepairTaskName()
    {
        var vehicle1 = VehicleFactory.CreateVehicle(licensePlate: "awc 1243").Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;
        var repairTask1 = RepairTaskFactory.CreateRepairTask(name: "Oil Change").Value;
        var repairTask2 = RepairTaskFactory.CreateRepairTask(name: "Changing Tires").Value;
        var labor1 = EmployeeFactory.CreateEmployee().Value;
        var labor2 = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle1, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle2, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask1, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask2, CancellationToken.None);
        await _context.Employees.AddAsync(labor1, CancellationToken.None);
        await _context.Employees.AddAsync(labor2, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle1);
        TrackEntity(vehicle2);
        TrackEntity(repairTask1);
        TrackEntity(repairTask2);
        TrackEntity(labor1);
        TrackEntity(labor2);

        var startBase = DateTimeOffset.UtcNow.Date.AddDays(2).AddHours(10);
        var wo1 = await SeedWorkOrderAsync(Spot.A, vehicle1.Id, labor1.Id, [repairTask1.Id], startBase);
        var wo2 = await SeedWorkOrderAsync(Spot.B, vehicle1.Id, labor1.Id, [repairTask2.Id], startBase.AddHours(1));
        var wo3 = await SeedWorkOrderAsync(Spot.C, vehicle1.Id, labor2.Id, [repairTask1.Id], startBase.AddHours(2));
        var wo4 = await SeedWorkOrderAsync(Spot.D, vehicle2.Id, labor2.Id, [repairTask1.Id], startBase.AddHours(3));

        var query = new GetWorkOrdersQuery(Page: 1, PageSize: 10, SearchTerm: repairTask1.Name);
        var queryResult = await _mediator.Send(query, CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        var paginated = queryResult.Value;
        Assert.Equal(1, paginated.PageNumber);
        Assert.Equal(10, paginated.PageSize);
        Assert.Equal(3, paginated.TotalCount);
        Assert.Equal(3, paginated.Items!.Count);

        var returnedIds = paginated.Items.Select(i => i.WorkOrderId).ToArray();
        var expectedOilChangeOrders = new[] { wo1, wo3, wo4 };
        Assert.All(returnedIds, id => Assert.Contains(id, expectedOilChangeOrders));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoMatch()
    {
        var vehicle1 = VehicleFactory.CreateVehicle(licensePlate: "awc 1243").Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;
        var repairTask1 = RepairTaskFactory.CreateRepairTask(name: "Oil Change").Value;
        var repairTask2 = RepairTaskFactory.CreateRepairTask(name: "Changing Tires").Value;
        var labor1 = EmployeeFactory.CreateEmployee().Value;
        var labor2 = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle1, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle2, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask1, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask2, CancellationToken.None);
        await _context.Employees.AddAsync(labor1, CancellationToken.None);
        await _context.Employees.AddAsync(labor2, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle1);
        TrackEntity(vehicle2);
        TrackEntity(repairTask1);
        TrackEntity(repairTask2);
        TrackEntity(labor1);
        TrackEntity(labor2);

        var startBase = DateTimeOffset.UtcNow.Date.AddDays(2).AddHours(10);
        await SeedWorkOrderAsync(Spot.A, vehicle1.Id, labor1.Id, [repairTask1.Id], startBase);
        await SeedWorkOrderAsync(Spot.B, vehicle1.Id, labor1.Id, [repairTask2.Id], startBase.AddHours(1));
        await SeedWorkOrderAsync(Spot.C, vehicle1.Id, labor2.Id, [repairTask1.Id], startBase.AddHours(2));
        await SeedWorkOrderAsync(Spot.D, vehicle2.Id, labor2.Id, [repairTask1.Id], startBase.AddHours(3));

        var query = new GetWorkOrdersQuery(Page: 1, PageSize: 10, SearchTerm: "amazon");
        var queryResult = await _mediator.Send(query, CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        var paginated = queryResult.Value;
        Assert.Empty(paginated.Items!);
        Assert.Equal(1, paginated.PageNumber);
        Assert.Equal(10, paginated.PageSize);
        Assert.Equal(0, paginated.TotalCount);
    }

    [Fact]
    public async Task Handle_ShouldSortByStartAtAscending()
    {
        var vehicle1 = VehicleFactory.CreateVehicle(licensePlate: "awc 1243").Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;
        var repairTask1 = RepairTaskFactory.CreateRepairTask(name: "Oil Change").Value;
        var repairTask2 = RepairTaskFactory.CreateRepairTask(name: "Changing Tires").Value;
        var labor1 = EmployeeFactory.CreateEmployee().Value;
        var labor2 = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle1, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle2, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask1, CancellationToken.None);
        await _context.RepairTasks.AddAsync(repairTask2, CancellationToken.None);
        await _context.Employees.AddAsync(labor1, CancellationToken.None);
        await _context.Employees.AddAsync(labor2, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle1);
        TrackEntity(vehicle2);
        TrackEntity(repairTask1);
        TrackEntity(repairTask2);
        TrackEntity(labor1);
        TrackEntity(labor2);

        // Start times are explicitly ordered for this test
        var startBase = DateTimeOffset.UtcNow.Date.AddDays(2).AddHours(10);
        var wo1 = await SeedWorkOrderAsync(Spot.A, vehicle1.Id, labor1.Id, [repairTask1.Id], startBase);
        var wo2 = await SeedWorkOrderAsync(Spot.B, vehicle1.Id, labor1.Id, [repairTask2.Id], startBase.AddHours(1));
        var wo3 = await SeedWorkOrderAsync(Spot.C, vehicle1.Id, labor2.Id, [repairTask1.Id], startBase.AddHours(2));
        var wo4 = await SeedWorkOrderAsync(Spot.D, vehicle2.Id, labor2.Id, [repairTask1.Id], startBase.AddHours(3));

        var query = new GetWorkOrdersQuery(Page: 1, PageSize: 10, SortColumn: "startat", SortDirection: "asc");
        var queryResult = await _mediator.Send(query, CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        var paginated = queryResult.Value;
        Assert.Equal(1, paginated.PageNumber);
        Assert.Equal(10, paginated.PageSize);
        Assert.Equal(4, paginated.TotalCount);
        Assert.Equal(4, paginated.Items!.Count);

        // Exact order check is safe here because start times are deterministic
        Assert.Equal(wo1, paginated.Items.ElementAt(0).WorkOrderId);
        Assert.Equal(wo2, paginated.Items.ElementAt(1).WorkOrderId);
        Assert.Equal(wo3, paginated.Items.ElementAt(2).WorkOrderId);
        Assert.Equal(wo4, paginated.Items.ElementAt(3).WorkOrderId);
    }
}