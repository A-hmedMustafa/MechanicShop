using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.Customers;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class CustomerMapperTests
{
    [Fact]
    public void VehicleToDto_ShouldMapCorrectly()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;
        
        var vehicleDto = vehicle.ToDto();

        Assert.NotNull(vehicleDto);
        Assert.Equal(vehicle.Id, vehicleDto.VehicleId);
        Assert.Equal(vehicle.Make, vehicleDto.Make);
        Assert.Equal(vehicle.Model, vehicleDto.Model);
        Assert.Equal(vehicle.Year, vehicleDto.Year);
        Assert.Equal(vehicle.LicensePlate, vehicleDto.LicensePlate);
        
    }
    [Fact]
    public void VehicleToDtos_ShouldMapCorrectly()
    {   
        var vehicle = VehicleFactory.CreateVehicle().Value;
        List<Vehicle> vehicles = [vehicle];
        
        
        var vehiclesDtos = vehicles.ToDtos();
        Assert.Single(vehiclesDtos);

        var vehicleDto = vehiclesDtos[0];
        Assert.NotNull(vehicleDto);
        Assert.Equal(vehicle.Id, vehicleDto.VehicleId);
        Assert.Equal(vehicle.Make, vehicleDto.Make);
        Assert.Equal(vehicle.Model, vehicleDto.Model);
        Assert.Equal(vehicle.Year, vehicleDto.Year);
        Assert.Equal(vehicle.LicensePlate, vehicleDto.LicensePlate);
        
    }
    [Fact]
    public void CustomerToDto_ShouldMapCorrectly()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value; 
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;

        var customerDto = customer.ToDto();

        Assert.Equal(customerDto.CustomerId, customer.Id);
        Assert.Equal(customerDto.Name, customer.Name);
        Assert.Equal(customerDto.Email, customer.Email);
        Assert.Equal(customerDto.PhoneNumber, customer.PhoneNumber);


        Assert.NotNull(customerDto.Vehicles);
        Assert.Single(customerDto.Vehicles);
        Assert.Equal(vehicle.Id, customerDto.Vehicles[0].VehicleId);
        Assert.Equal(vehicle.Make, customerDto.Vehicles[0].Make);
        Assert.Equal(vehicle.Model, customerDto.Vehicles[0].Model);
        Assert.Equal(vehicle.Year, customerDto.Vehicles[0].Year);
        Assert.Equal(vehicle.LicensePlate, customerDto.Vehicles[0].LicensePlate);
    }
    

    [Fact]
    public void CustomerToDtos_ShouldMapCorrectly()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value; 
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        var customers = new List<Customer> {customer};
        var customersDtos = customers.ToDtos();

        Assert.Single(customersDtos);
        var customerDto = customersDtos[0];

        Assert.NotNull(customerDto);
        Assert.Equal(customerDto.CustomerId, customer.Id);
        Assert.Equal(customerDto.Name, customer.Name);
        Assert.Equal(customerDto.Email, customer.Email);
        Assert.Equal(customerDto.PhoneNumber, customer.PhoneNumber);

        Assert.NotNull(customerDto.Vehicles);
        Assert.Single(customerDto.Vehicles);
        Assert.Equal(vehicle.Id, customerDto.Vehicles[0].VehicleId);
        Assert.Equal(vehicle.Make, customerDto.Vehicles[0].Make);
        Assert.Equal(vehicle.Model, customerDto.Vehicles[0].Model);
        Assert.Equal(vehicle.Year, customerDto.Vehicles[0].Year);
        Assert.Equal(vehicle.LicensePlate, customerDto.Vehicles[0].LicensePlate);
    }
}