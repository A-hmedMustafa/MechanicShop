using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator;

    public CreateCustomerCommandValidatorTests()
    {
        _validator = new CreateCustomerCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var createCustomerCommand = new CreateCustomerCommand(
            Name: "John Doe",
            PhoneNumber: "+1234567890",
            Email: "john@example.com",
            Vehicles: [new CreateVehicleCommand("Toyota", "Camry", 2020, "ABC123")]);

        var createCustomerCommandResult = _validator.Validate(createCustomerCommand);

        Assert.True(createCustomerCommandResult.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var createCustomerCommand = new CreateCustomerCommand(
            Name: "",
            PhoneNumber: "+1234567890",
            Email: "john@example.com",
            Vehicles: [new CreateVehicleCommand("Toyota", "Camry", 2020, "ABC123")]);

        var createCustomerCommandResult = _validator.Validate(createCustomerCommand);

        Assert.False(createCustomerCommandResult.IsValid);
        Assert.Contains(createCustomerCommandResult.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldFail()
    {
        var createCustomerCommand = new CreateCustomerCommand(
            Name: "John",
            PhoneNumber: "+1234567890",
            Email: "notanemail",
            Vehicles: [new CreateVehicleCommand("Toyota", "Camry", 2020, "ABC123")]);

        var createCustomerCommandResult = _validator.Validate(createCustomerCommand);

        Assert.False(createCustomerCommandResult.IsValid);
        Assert.Contains(createCustomerCommandResult.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WithInvalidPhone_ShouldFail()
    {
        var createCustomerCommand = new CreateCustomerCommand(
            Name: "John",
            PhoneNumber: "123", 
            Email: "john@example.com",
            Vehicles: [new CreateVehicleCommand("Toyota", "Camry", 2020, "ABC123")]);

        var createCustomerCommandResult = _validator.Validate(createCustomerCommand);

        Assert.False(createCustomerCommandResult.IsValid);
        Assert.Contains(createCustomerCommandResult.Errors, e => e.PropertyName == "PhoneNumber");
    }

    [Fact]
    public void Validate_WithNoVehicles_ShouldFail()
    {
        var createCustomerCommand = new CreateCustomerCommand(
            Name: "John",
            PhoneNumber: "+1234567890",
            Email: "john@example.com",
            Vehicles: []);

        var createCustomerCommandResult = _validator.Validate(createCustomerCommand);

        Assert.False(createCustomerCommandResult.IsValid);
        Assert.Contains(createCustomerCommandResult.Errors, e => e.PropertyName == "Vehicles");
    }

    [Fact]
    public void Validate_WithInvalidVehicle_ShouldFail()
    {
        var createCustomerCommand = new CreateCustomerCommand(
            Name: "John",
            PhoneNumber: "+1234567890",
            Email: "john@example.com",
            Vehicles: [new CreateVehicleCommand("", "", 0, "")]); 

        var createCustomerCommandResult = _validator.Validate(createCustomerCommand);

        Assert.False(createCustomerCommandResult.IsValid);
        Assert.Contains(createCustomerCommandResult.Errors, e => e.ErrorMessage.Contains("Make") || e.ErrorMessage.Contains("Model") || e.ErrorMessage.Contains("LicensePlate"));
    }
}