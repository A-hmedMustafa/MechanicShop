using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler(
    ILogger<GetCustomerByIdQueryHandler> logger,
    IAppDbContext context
    ) : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly ILogger<GetCustomerByIdQueryHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.AsNoTracking()
        .Include(customer => customer.Vehicles)
        .FirstOrDefaultAsync(customer => customer.Id == request.CustomerId, cancellationToken);

        if(customer is null)
        {
            _logger.LogWarning("Customer with id {CustomerId} was not found", request.CustomerId);
            
            return Error.NotFound(
                code:"Customer_NotFound",
                description:$"Customer With Id '{request.CustomerId}' Not Found"
                );
        }
        
        return customer.ToDto();
    }
}