using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler (
    ILogger<UpdateCustomerCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache
): IRequestHandler<UpdateCustomerCommand, Result<Updated>>
{
    private readonly ILogger<UpdateCustomerCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    public async Task<Result<Updated>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
        .Include(customer => customer.Vehicles)
        .FirstOrDefaultAsync(customer => customer.Id == request.CustomerId, 
        cancellationToken);

        if(customer is null)
        {
            _logger.LogInformation("Customer {CustomerId} Not Found For Update." ,request.CustomerId);
            return ApplicationErrors.CustomerNotFound;
        }

        var validatedVehicles = new List<Vehicle>();
        
        foreach(var vehicle in request.Vehicles)
        {
            var vehicleId = vehicle.VehicleId ?? Guid.NewGuid();
            
            var vehicleCreationResult = Vehicle.Create(vehicleId, vehicle.Make, vehicle.Model, vehicle.Year, vehicle.LicensePlate);
            
            if(vehicleCreationResult.IsError)
                return vehicleCreationResult.Errors;

            validatedVehicles.Add(vehicleCreationResult.Value);
        }

        var customerUpdateResult = customer.Update(request.Name, request.Email, request.PhoneNumber);
       
        if(customerUpdateResult.IsError)
            return customerUpdateResult.Errors;
    
        var upsertPartsResult = customer.UpsertParts(validatedVehicles);
        
        if(upsertPartsResult.IsError)
            return upsertPartsResult.Errors;

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync("customer",cancellationToken);

        return Result.Updated;
    }
}