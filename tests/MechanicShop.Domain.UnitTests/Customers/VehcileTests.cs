using MechanicShop.Domain.Common.Results;
using MechanicShop.Tests.Common.Customers;
using Xunit;

namespace MechanicShop.Domain.UnitTests.Customers;

public class VehicleTests
{
    [Fact]
    public void CreateVehicle_ShouldSucceed_WithValidDate()
    {
        var id = Guid.NewGuid();
        const string make = "Honda";
        const string model = "Accord";
        const int year = 2024;
        const string licensePlate = "ABC 123";

        var vehicleCreationResult = VehicleFactory.CreateVehicle(
            id:id, make: make, model: model, year: year,licensePlate: licensePlate);

        var newVehicle = vehicleCreationResult.Value;

        Assert.True(vehicleCreationResult.IsSuccess);
        Assert.Equal(make, newVehicle.Make); 
        Assert.Equal(model, newVehicle.Model); 
        Assert.Equal(year, newVehicle.Year); 
        Assert.Equal(licensePlate, newVehicle.LicensePlate); 
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateVehicle_ShouldFail_WhenMakeInvalid(string make)
    {
        var vehicleCreationResult = VehicleFactory.CreateVehicle(make: make);
        Assert.True(vehicleCreationResult.IsError);
    }

    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateVehicle_ShouldFail_WhenModelInvalid(string model)
    {
        var vehicleCreationResult = VehicleFactory.CreateVehicle(model: model);
        Assert.True(vehicleCreationResult.IsError);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateVehicle_ShouldFail_WhenLicensePlateInvalid(string plate)
    {
        var vehicleCreationResult = VehicleFactory.CreateVehicle(licensePlate: plate);
        Assert.True(vehicleCreationResult.IsError);
    }

    [Theory]
    [InlineData(1700)]
    [InlineData(3000)]
    public void CreateVehicle_ShouldFail_WhenYearInvalid(int year)
    {
        var vehicleCreationResult = VehicleFactory.CreateVehicle(year: year);
        Assert.True(vehicleCreationResult.IsError);
    }

    [Fact]
    public void UpdateVehicle_ShouldSucceed_WithValidData()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var vehicleUpdateResult = vehicle.Update("Toyota", "Camry", 2023, "ASD 124");

        Assert.True(vehicleUpdateResult.IsSuccess);
        Assert.Equal("Toyota", vehicle.Make);
        Assert.Equal("Camry", vehicle.Model);
        Assert.Equal(2023, vehicle.Year);
        Assert.Equal("ASD 124", vehicle.LicensePlate);
    }

    [Fact]
    public void UpdateVehicle_ShouldFail_WhenMakeIsInvalid()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var vehicleUpdateResult = vehicle.Update(string.Empty, "Model", 2022, "XYZ123");

        Assert.True(vehicleUpdateResult.IsError);
    }
    [Fact]
    public void UpdateVehicle_ShouldFail_WhenModelIsInvalid()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var vehicleUpdateResult = vehicle.Update("Make", string.Empty, 2022, "XYZ123");

        Assert.True(vehicleUpdateResult.IsError);
    }

    [Fact]
    public void UpdateVehicle_ShouldFail_WhenLicensePlateIsInvalid()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var vehicleUpdateResult = vehicle.Update("Make", "Model", 2022, string.Empty);

        Assert.True(vehicleUpdateResult.IsError);
    }
    
    [Theory]
    [InlineData(1800)]
    [InlineData(5000)]
    public void UpdateVehicle_ShouldFail_WhenYearInvalid(int year)
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var vehicleUpdateResult = vehicle.Update("Make", "Model", year, "XYZ123");

        Assert.True(vehicleUpdateResult.IsError);
    }

    [Fact]
    public void VehicleInfo_ShouldReturnFormattedString()
    {
        var vehicle = VehicleFactory.CreateVehicle(make: "Ford", model: "Mustang", year:2021).Value;

        Assert.Equal("Ford | Mustang | 2021", vehicle.VehicleInfo);
    }
}