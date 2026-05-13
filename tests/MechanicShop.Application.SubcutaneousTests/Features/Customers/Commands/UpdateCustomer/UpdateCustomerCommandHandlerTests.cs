using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.Customers;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.UpdateCustomer;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateCustomerCommandHandlerTests: SubcutaneousTestBase
{
    public UpdateCustomerCommandHandlerTests(WebAppFactory factory) : base(factory) { }
      private async Task<(Customer customer, Vehicle vehicle)> SeedCustomerWithEntitiesAsync(
        string name = "Original",
        string email = "original@example.com",
        string phone = "+1111111111",
        string make = "Ford", string model = "Focus", int year = 2020, string plate = "OLD123")
    {
        var vehicle = VehicleFactory.CreateVehicle(make: make, model: model, year: year, licensePlate: plate).Value;
        var customer = CustomerFactory.CreateCustomer(name: name, email: email, phoneNumber: phone, vehicles: [vehicle]).Value;

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(customer);
        TrackEntity(vehicle);  
        return (customer, vehicle);
    }

    
    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
        var (customer, existingVehicle) = await SeedCustomerWithEntitiesAsync();
       

        var updateCommand = new UpdateCustomerCommand(
            CustomerId: customer.Id,
            Name: "Updated Name",
            PhoneNumber: "+2222222222",
            Email: "updated@example.com",
            Vehicles:
            [
                
                new UpdateVehicleCommand(
                    VehicleId: existingVehicle.Id,
                    Make: "UpdatedMake",
                    Model: "UpdatedModel",
                    Year: 2023,
                    LicensePlate: "UPD123"),
                
                new UpdateVehicleCommand(
                    VehicleId: null,
                    Make: "NewCar",
                    Model: "NewModel",
                    Year: 2024,
                    LicensePlate: "NEW456")
            ]);

        var updateCustomerCommandResult = await _mediator.Send(updateCommand, CancellationToken.None);

        Assert.True(updateCustomerCommandResult.IsSuccess);
        await _mediator.Send(new RemoveCustomerCommand(customer.Id), CancellationToken.None);
    }

   
    [Fact]
    public async Task Handle_WhenCustomerNotFound_ShouldFail()
    {
        var updateCommand = new UpdateCustomerCommand(
            CustomerId: Guid.NewGuid(),
            Name: "Ghost",
            PhoneNumber: "+1234567890",
            Email: "ghost@example.com",
            Vehicles:
            [
                new UpdateVehicleCommand(null, "Any", "Car", 2020, "ABC123")
            ]);

        var updateCustomerCommandResult = await _mediator.Send(updateCommand, CancellationToken.None);

        Assert.False(updateCustomerCommandResult.IsSuccess);
        Assert.Contains(updateCustomerCommandResult.Errors, e => e.Code == "ApplicationErrors.Customer.NotFound");
    }

    
    [Fact]
    public async Task Handle_WhenVehicleDomainValidationFails_ShouldFail()
    {
        var (customer, vehicle) = await SeedCustomerWithEntitiesAsync();
       

        var updateCommand = new UpdateCustomerCommand(
            CustomerId: customer.Id,
            Name: "Test",
            PhoneNumber: "+1234567890",
            Email: "test@example.com",
            Vehicles:
            [
                new UpdateVehicleCommand(
                    VehicleId: null,
                    Make: "Bad",
                    Model: "Car",
                    Year: 0,         
                    LicensePlate: "BAD123")
            ]);

        var updateCustomerCommandResult = await _mediator.Send(updateCommand, CancellationToken.None);

        Assert.False(updateCustomerCommandResult.IsSuccess);
       
        Assert.Contains(updateCustomerCommandResult.Errors, e => e.Code == "Vehicle_Year_Invalid");
    }

    
}