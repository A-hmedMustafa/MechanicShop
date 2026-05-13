using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.CreateCustomer;

public class CreateVehicleCommandValidatorTests
{
    private readonly CreateVehicleCommandValidator _validator;

    public CreateVehicleCommandValidatorTests()
    {
        _validator = new CreateVehicleCommandValidator();
    }
    
    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var command = new CreateVehicleCommand(
            Make: "Toyota",
            Model: "Camry",
            Year: 2020,
            LicensePlate: "ABC123");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyMake_ShouldFail()
    {
        var command = new CreateVehicleCommand(
            Make: "",
            Model: "Camry",
            Year: 2020,
            LicensePlate: "ABC123");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Make");
    }

    [Fact]
    public void Validate_WithEmptyModel_ShouldFail()
    {
        var command = new CreateVehicleCommand(
            Make: "Toyota",
            Model: "",
            Year: 2020,
            LicensePlate: "ABC123");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Model");
    }

    [Fact]
    public void Validate_WithEmptyLicensePlate_ShouldFail()
    {
        var command = new CreateVehicleCommand(
            Make: "Toyota",
            Model: "Camry",
            Year: 2020,
            LicensePlate: "");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LicensePlate");
    }

    [Fact]
    public void Validate_WithTooLongFields_ShouldFail()
    {
        var command = new CreateVehicleCommand(
            Make: new string('X', 51),
            Model: new string('Y', 51),
            Year: 2020,
            LicensePlate: new string('Z', 11));

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Make");
        Assert.Contains(result.Errors, e => e.PropertyName == "Model");
        Assert.Contains(result.Errors, e => e.PropertyName == "LicensePlate");
    }
}