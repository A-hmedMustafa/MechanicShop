using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.Customers;
using Xunit;

namespace MechanicShop.Domain.UnitTests.Customers;

public class CustomerTests
{
    [Fact]
    public void CreateCustomer_ShouldSucceed_WithValidData()
    {
        Guid id = Guid.NewGuid();
        string name = "ahmed";
        string phoneNumber = "0159124878";
        string email = "adfj@gmail.Com";
        List<Vehicle> vehicles = [VehicleFactory.CreateVehicle().Value] ;

        var customerCreationResult = CustomerFactory.CreateCustomer(
            id: id,
            name: name,
            phoneNumber: phoneNumber,
            email: email,
            vehicles: vehicles
        );

        var newCustomer = customerCreationResult.Value;

        Assert.True(customerCreationResult.IsSuccess);
        Assert.IsType<Customer>(newCustomer);
        Assert.NotNull(newCustomer);
        Assert.Equal(id, newCustomer.Id);
        Assert.Equal(name, newCustomer.Name);
        Assert.Equal(phoneNumber, newCustomer.PhoneNumber);
        Assert.Equal(email, newCustomer.Email);
        Assert.Single(newCustomer.Vehicles);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCustomer_ShouldFail_WhenNameInvalid(string? invalidName)
    {
        var customerCreationResult = CustomerFactory.CreateCustomer(name: invalidName);
        Assert.True(customerCreationResult.IsError);
    }

    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")] // less than 7
    [InlineData("12345678910111213")] // greater than 15
    public void CreateCustomer_ShouldFail_WhenPhoneInvalid(string? invalidPhone)
    {
        var customerCreationResult = CustomerFactory.CreateCustomer(phoneNumber: invalidPhone);
        Assert.True(customerCreationResult.IsError);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCustomer_ShouldFail_WhenEmailEmptyOrNull(string? emptyEmail)
    {
        var customerCreationResult = CustomerFactory.CreateCustomer(email: emptyEmail);
        Assert.True(customerCreationResult.IsError);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("abc1.@")]
    public void CreateCustomer_ShouldFail_WhenEmailInvalid(string? invalidEmail)
    {
        var customerCreationResult = CustomerFactory.CreateCustomer(email: invalidEmail);
        Assert.True(customerCreationResult.IsError);
    }

    
    [Fact]
    public void UpdateCustomer_ShouldSucceed_WithValidData()
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var customerUpdateResult = customer.Update("Updated Name", "updated@email.com", "1234567890");

        Assert.True(customerUpdateResult.IsSuccess);
        Assert.Equal(Result.Updated, customerUpdateResult.Value);
    }

    [Fact]
    public void UpdateCustomer_ShouldFail_WhenInvalidName()
    {
        var customerUpdateResult = CustomerFactory.CreateCustomer().Value;

        var result = customerUpdateResult.Update(string.Empty, "newEmail@localhost", "123-1232");

        Assert.True(result.IsError);
    }

    [Fact]
    public void UpdateCustomer_ShouldFail_WhenInvalidPhoneNumber()
    {
        var customerUpdateResult = CustomerFactory.CreateCustomer().Value;

        var result = customerUpdateResult.Update("New name", "newEmail@localhost", string.Empty);

        Assert.True(result.IsError);
    }

    [Fact]
    public void UpdateCustomer_ShouldFail_WhenInvalidEmail()
    {
        var customerUpdateResult = CustomerFactory.CreateCustomer().Value;

        var result = customerUpdateResult.Update("New name", string.Empty, "123-1232");

        Assert.True(result.IsError);
    }

    [Fact]
    public void UpsertParts_ShouldAddNewVehiclesAndUpdateExisting()
    {
        var vehicle1 = VehicleFactory.CreateVehicle(make: "Ford").Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1]).Value;

        var vehicle1Updated = VehicleFactory.CreateVehicle(make: "BMW").Value;
        var vehicle2 = VehicleFactory.CreateVehicle(make: "Suzuki").Value;

        var upsertResult = customer.UpsertParts([vehicle1Updated, vehicle2]);

        Assert.True(upsertResult.IsSuccess);
        Assert.Equal(2, customer.Vehicles.Count());
        Assert.Equal(Result.Updated, upsertResult.Value);
        Assert.Contains(customer.Vehicles, vehicle => vehicle.Id == vehicle1Updated.Id && vehicle.Make == "BMW");
        Assert.Contains(customer.Vehicles, vehicle => vehicle.Id == vehicle2.Id && vehicle.Make == "Suzuki");

    }

    [Fact]
    public void UpsertParts_ShouldRemoveVehiclesNotInIncomingList()
    {
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;

        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;

        var vehicle2Updated = VehicleFactory.CreateVehicle(id: vehicle2.Id).Value;

        var upsertResult = customer.UpsertParts([vehicle2Updated]);

        Assert.True(upsertResult.IsSuccess);
        Assert.Equal(Result.Updated, upsertResult.Value); 
        Assert.Single(customer.Vehicles);
        Assert.Equal(vehicle2.Id, customer.Vehicles.Single().Id);
    }
}