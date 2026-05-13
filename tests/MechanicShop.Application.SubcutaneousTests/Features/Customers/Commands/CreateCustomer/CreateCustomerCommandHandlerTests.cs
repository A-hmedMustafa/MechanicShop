using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandlerTests: SubcutaneousTestBase
{
    public CreateCustomerCommandHandlerTests(WebAppFactory factory) : base(factory) { }
    
    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
        var createVehicleCommand = new CreateVehicleCommand(
            Make: "Honda",
            Model: "Civic",
            Year: 2023,
            LicensePlate: "XYZ987");

        var createCustomerCommand = new CreateCustomerCommand(
            Name: "Alice Smith",
            PhoneNumber: "+1234567890",
            Email: "alice@example.com",
            Vehicles: [createVehicleCommand]);

        var createCustomerCommandResult = await _mediator.Send(createCustomerCommand, CancellationToken.None);

        Assert.True(createCustomerCommandResult.IsSuccess);
        var customerDto = createCustomerCommandResult.Value;
        Assert.NotNull(customerDto);
        Assert.Equal("Alice Smith", customerDto.Name);
        Assert.NotEmpty(customerDto.Vehicles);
        Assert.Equal("Honda", customerDto.Vehicles[0].Make);

        var removeCommand = new RemoveCustomerCommand(customerDto.CustomerId);
        await _mediator.Send(removeCommand, CancellationToken.None);
    }

    
    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldFail()
    {
     
        var existingVehicle = VehicleFactory.CreateVehicle().Value;
        var existingCustomer = CustomerFactory.CreateCustomer(
            email: "duplicate@example.com",
            vehicles: [existingVehicle]).Value;

        await _context.Customers.AddAsync(existingCustomer, CancellationToken.None);
        TrackEntity(existingCustomer);
        await _context.SaveChangesAsync(CancellationToken.None);

        var createVehicleCommand = new CreateVehicleCommand("Toyota", "Corolla", 2020, "ABC123");
        var createCustomerCommand = new CreateCustomerCommand(
            Name: "Bob",
            PhoneNumber: "+1987654321",
            Email: "duplicate@example.com",
            Vehicles: [createVehicleCommand]);

        var createCustomerCommandResult = await _mediator.Send(createCustomerCommand, CancellationToken.None);

        Assert.False(createCustomerCommandResult.IsSuccess);
        Assert.Contains(createCustomerCommandResult.Errors, e => e.Code == "Customer_Email_Exists");
    }

   
    [Fact]
    public async Task Handle_WithInvalidVehicleDomainRule_ShouldFail()
    {
       
        var createVehicleCommand = new CreateVehicleCommand(
            Make: "Ford",
            Model: "Focus",
            Year: 0, 
            LicensePlate: "LMN456");

        var createCustomerCommand = new CreateCustomerCommand(
            Name: "Charlie",
            PhoneNumber: "+1122334455",
            Email: "charlie@example.com",
            Vehicles: [createVehicleCommand]);

        var createCustomerCommandResult = await _mediator.Send(createCustomerCommand, CancellationToken.None);

        Assert.False(createCustomerCommandResult.IsSuccess);
       
       
        Assert.Contains(createCustomerCommandResult.Errors, e => e.Code == "Vehicle_Year_Invalid");
    }
}