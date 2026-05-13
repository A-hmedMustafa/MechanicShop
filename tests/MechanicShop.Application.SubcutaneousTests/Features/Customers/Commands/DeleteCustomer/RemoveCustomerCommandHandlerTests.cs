using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.DeleteCustomer;


public class RemoveCustomerCommandHandlerTests: SubcutaneousTestBase
{
    public RemoveCustomerCommandHandlerTests(WebAppFactory factory) : base(factory) { }
    

  

    [Fact]
    public async Task Handle_WithCustomerWithoutWorkOrders_ShouldSucceed()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        var removeCustomerCommand = new RemoveCustomerCommand(customer.Id);

        var removeCustomerCommandResult = await _mediator.Send(removeCustomerCommand, CancellationToken.None);

        Assert.True(removeCustomerCommandResult.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ShouldFail()
    {
        var removeCustomerCommand = new RemoveCustomerCommand(Guid.NewGuid());

        var removeCustomerCommandResult = await _mediator.Send(removeCustomerCommand, CancellationToken.None);

        Assert.False(removeCustomerCommandResult.IsSuccess);
        Assert.Contains(removeCustomerCommandResult.Errors, e => e.Code == "ApplicationErrors.Customer.NotFound");
    }

    
    [Fact]
    public async Task Handle_WhenCustomerHasWorkOrders_ShouldFail()
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

        TrackEntity(vehicle);
        TrackEntity(customer);
        TrackEntity(repairTask);
        TrackEntity(labor);
        
        var createWorkOrderCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle.Id,
            StartsAt: DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10),
            RepairTaskIds: [repairTask.Id],
            LaborId: labor.Id);
        await _mediator.Send(createWorkOrderCommand, CancellationToken.None);

        var removeCustomerCommand = new RemoveCustomerCommand(customer.Id);

        var removeCustomerCommandResult = await _mediator.Send(removeCustomerCommand, CancellationToken.None);

        Assert.False(removeCustomerCommandResult.IsSuccess);
        Assert.Contains(removeCustomerCommandResult.Errors, e => e.Code == "Customer.CannotDelete");
    }
}