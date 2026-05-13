using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandValidatorTests
{
    private readonly UpdateCustomerCommandValidator _validator;

    public UpdateCustomerCommandValidatorTests()
    {
        _validator = new UpdateCustomerCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var updateCustomerCommand = new UpdateCustomerCommand(
            CustomerId: Guid.NewGuid(),
            Name: "Jane Doe",
            PhoneNumber: "+1234567890",
            Email: "jane@example.com",
            Vehicles:
            [
                new UpdateVehicleCommand(Guid.NewGuid(), "Honda", "Civic", 2023, "XYZ987")
            ]);

        var  updateCustomerCommandResult  = _validator.Validate(updateCustomerCommand);

        Assert.True( updateCustomerCommandResult .IsValid);
    }

    [Fact]
    public void Validate_WithEmptyCustomerId_ShouldFail()
    {
        var updateCustomerCommand = new UpdateCustomerCommand(
            CustomerId: Guid.Empty,
            Name: "Jane",
            PhoneNumber: "+1234567890",
            Email: "jane@example.com",
            Vehicles:
            [
                new UpdateVehicleCommand(Guid.NewGuid(), "Honda", "Civic", 2023, "XYZ987")
            ]);

        var  updateCustomerCommandResult  = _validator.Validate(updateCustomerCommand);

        Assert.False( updateCustomerCommandResult .IsValid);
        Assert.Contains( updateCustomerCommandResult .Errors, e => e.PropertyName == "CustomerId");
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var updateCustomerCommand = new UpdateCustomerCommand(
            CustomerId: Guid.NewGuid(),
            Name: "",
            PhoneNumber: "+1234567890",
            Email: "jane@example.com",
            Vehicles:
            [
                new UpdateVehicleCommand(Guid.NewGuid(), "Honda", "Civic", 2023, "XYZ987")
            ]);

        var  updateCustomerCommandResult  = _validator.Validate(updateCustomerCommand);

        Assert.False( updateCustomerCommandResult .IsValid);
        Assert.Contains( updateCustomerCommandResult .Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldFail()
    {
        var updateCustomerCommand = new UpdateCustomerCommand(
            CustomerId: Guid.NewGuid(),
            Name: "Jane",
            PhoneNumber: "+1234567890",
            Email: "bad-email",
            Vehicles:
            [
                new UpdateVehicleCommand(Guid.NewGuid(), "Honda", "Civic", 2023, "XYZ987")
            ]);

        var  updateCustomerCommandResult  = _validator.Validate(updateCustomerCommand);

        Assert.False( updateCustomerCommandResult .IsValid);
        Assert.Contains( updateCustomerCommandResult .Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WithInvalidPhone_ShouldFail()
    {
        var updateCustomerCommand = new UpdateCustomerCommand(
            CustomerId: Guid.NewGuid(),
            Name: "Jane",
            PhoneNumber: "123",
            Email: "jane@example.com",
            Vehicles:
            [
                new UpdateVehicleCommand(Guid.NewGuid(), "Honda", "Civic", 2023, "XYZ987")
            ]);

        var  updateCustomerCommandResult  = _validator.Validate(updateCustomerCommand);

        Assert.False( updateCustomerCommandResult .IsValid);
        Assert.Contains( updateCustomerCommandResult .Errors, e => e.PropertyName == "PhoneNumber");
    }

    [Fact]
    public void Validate_WithEmptyVehiclesList_ShouldFail()
    {
        var updateCustomerCommand = new UpdateCustomerCommand(
            CustomerId: Guid.NewGuid(),
            Name: "Jane",
            PhoneNumber: "+1234567890",
            Email: "jane@example.com",
            Vehicles: []);

        var  updateCustomerCommandResult  = _validator.Validate(updateCustomerCommand);

        Assert.False( updateCustomerCommandResult .IsValid);
        Assert.Contains( updateCustomerCommandResult .Errors, e => e.PropertyName == "Vehicles");
    }

    [Fact]
    public void Validate_WithInvalidVehicle_ShouldFail()
    {
        var updateCustomerCommand = new UpdateCustomerCommand(
            CustomerId: Guid.NewGuid(),
            Name: "Jane",
            PhoneNumber: "+1234567890",
            Email: "jane@example.com",
            Vehicles:
            [
                new UpdateVehicleCommand(Guid.NewGuid(), "", "", 0, "") // all invalid
            ]);

        var  updateCustomerCommandResult  = _validator.Validate(updateCustomerCommand);

        Assert.False( updateCustomerCommandResult .IsValid);
       
        Assert.Contains( updateCustomerCommandResult .Errors, e => e.PropertyName.StartsWith("Vehicles"));
    }
}