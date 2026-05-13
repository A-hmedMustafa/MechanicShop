using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labor.Dtos;
using MechanicShop.Application.Features.Labors.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Labors.Queries.GetLabors;

public class GetLaborsQueryHandler(IAppDbContext context) : IRequestHandler<GetLaborsQuery, Result<List<LaborDto>>>
{
    private readonly IAppDbContext _context = context;
    public async Task<Result<List<LaborDto>>> Handle(GetLaborsQuery request, CancellationToken cancellationToken)
    {
        var labors = await _context.Employees
        .AsNoTracking()
        .Where(employee => employee.Role == Domain.Identity.Role.Labor)
        .ToListAsync(cancellationToken);

        return labors.ToDtos();
    }
}