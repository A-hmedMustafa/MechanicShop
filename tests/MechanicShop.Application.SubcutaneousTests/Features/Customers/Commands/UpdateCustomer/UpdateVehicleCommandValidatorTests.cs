using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.UpdateCustomer;

public class UpdateVehicleCommandValidatorTests
{
    private readonly UpdateVehicleCommandValidator _validator;

    public UpdateVehicleCommandValidatorTests()
    {
        _validator = new UpdateVehicleCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var updateVehicleCommand = new UpdateVehicleCommand(
            VehicleId: Guid.NewGuid(),
            Make: "Toyota",
            Model: "Camry",
            Year: 2022,
            LicensePlate: "ABC123");

        var updateVehicleCommandResult = _validator.Validate(updateVehicleCommand);

        Assert.True(updateVehicleCommandResult.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyMake_ShouldFail()
    {
        var updateVehicleCommand = new UpdateVehicleCommand(
            VehicleId: Guid.NewGuid(),
            Make: "",
            Model: "Camry",
            Year: 2022,
            LicensePlate: "ABC123");

        var updateVehicleCommandResult = _validator.Validate(updateVehicleCommand);

        Assert.False(updateVehicleCommandResult.IsValid);
        Assert.Contains(updateVehicleCommandResult.Errors, e => e.PropertyName == "Make");
    }

    [Fact]
    public void Validate_WithEmptyModel_ShouldFail()
    {
        var updateVehicleCommand = new UpdateVehicleCommand(
            VehicleId: Guid.NewGuid(),
            Make: "Toyota",
            Model: "",
            Year: 2022,
            LicensePlate: "ABC123");

        var updateVehicleCommandResult = _validator.Validate(updateVehicleCommand);

        Assert.False(updateVehicleCommandResult.IsValid);
        Assert.Contains(updateVehicleCommandResult.Errors, e => e.PropertyName == "Model");
    }

    [Fact]
    public void Validate_WithEmptyLicensePlate_ShouldFail()
    {
        var updateVehicleCommand = new UpdateVehicleCommand(
            VehicleId: Guid.NewGuid(),
            Make: "Toyota",
            Model: "Camry",
            Year: 2022,
            LicensePlate: "");

        var updateVehicleCommandResult = _validator.Validate(updateVehicleCommand);

        Assert.False(updateVehicleCommandResult.IsValid);
        Assert.Contains(updateVehicleCommandResult.Errors, e => e.PropertyName == "LicensePlate");
    }
}