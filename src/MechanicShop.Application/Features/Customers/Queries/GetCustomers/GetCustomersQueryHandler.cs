using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomers;

public class GetCustomersQueryHandler(
    IAppDbContext context) : IRequestHandler<GetCustomersQuery, Result<List<CustomerDto>>>
{
    private readonly IAppDbContext _context = context;
   
    public async Task<Result<List<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _context.Customers
        .Include(customer => customer.Vehicles)
        .AsNoTracking()
        .ToListAsync(cancellationToken);

        return customers.ToDtos();
    }
}