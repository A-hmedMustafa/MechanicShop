using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler(
      ILogger<CreateCustomerCommandHandler> logger,
      IAppDbContext context,
      HybridCache cache) : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    private readonly ILogger<CreateCustomerCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLower();

        var exists = await _context.Customers.AnyAsync(customer => customer.Email!.ToLower() == email, cancellationToken);
        
        if (exists)
        {
            _logger.LogWarning("Customer Creation Aborted. Email Already Exists.");
            return CustomerErrors.CustomerExists;
        }

        List<Vehicle> vehicles = [];

        foreach(var vehicle in request.Vehicles)
        {
            var vehicleCreationResult = Vehicle.Create(Guid.NewGuid(),vehicle.Make,vehicle.Model,vehicle.Year,vehicle.LicensePlate);
            
            if(vehicleCreationResult.IsError)
                return vehicleCreationResult.Errors;

            vehicles.Add(vehicleCreationResult.Value);    
        }

        var customerCreationResult = Customer.Create(Guid.NewGuid(),request.Name.Trim(),request.PhoneNumber.Trim(),request.Email.Trim(),vehicles);
        
        if(customerCreationResult.IsError)
            return customerCreationResult.Errors;

        _context.Customers.Add(customerCreationResult.Value);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        await _cache.RemoveByTagAsync("customer", cancellationToken);
        
        var customer = customerCreationResult.Value;
        
        _logger.LogInformation("Customer Create Successfully. Id: {CustomerId}",customerCreationResult.Value.Id);
        
        return customer.ToDto();
    }   
}