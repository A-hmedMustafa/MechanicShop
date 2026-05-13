using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;

namespace MechanicShop.Application.Features.Customers.Mappers;

public static class CustomerMapper
{
    public static CustomerDto ToDto(this Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);
        return new CustomerDto
        {
            CustomerId = customer.Id,
            Name = customer.Name!,
            Email = customer.Email!,
            PhoneNumber = customer.PhoneNumber!,
            Vehicles = customer.Vehicles?.Select(v=>v.ToDto()).ToList() ?? []
        };
    }
    public static List<CustomerDto> ToDtos(this IEnumerable<Customer> customers)
    {
        return [.. customers.Select(c => c.ToDto())];
    }

    public static VehicleDto ToDto(this Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        return new VehicleDto(
            vehicle.Id,
            vehicle.Make!, 
            vehicle.Model!,
            vehicle.Year,
            vehicle.LicensePlate!
        );
    }

    public static List<VehicleDto> ToDtos(this IEnumerable<Vehicle> vehicles)
    {
        return [..vehicles.Select(v => v.ToDto())];
    }
}