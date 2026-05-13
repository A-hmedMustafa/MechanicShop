using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderByIdQuery;

public class GetWorkOrderByIdQueryHandler(
    ILogger<GetWorkOrderByIdQueryHandler> logger,
    IAppDbContext context
) : IRequestHandler<GetWorkOrderByIdQuery, Result<WorkOrderDto>>
{
    private readonly ILogger<GetWorkOrderByIdQueryHandler> _logger = logger;
    private readonly IAppDbContext _context = context;

    public async Task<Result<WorkOrderDto>> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .AsNoTracking()
            .Include(wOrder => wOrder.RepairTasks).ThenInclude(rTask => rTask.Parts)
            .Include(wOrder => wOrder.Labor)
            .Include(wOrder => wOrder.Vehicle!).ThenInclude(vehicle => vehicle.Customer)
            .Include(wOrder => wOrder.Invoice)
            .FirstOrDefaultAsync(wOrder => wOrder.Id == request.WorkOrderId,
                cancellationToken);
            

       if (workOrder is null)
        {
            _logger.LogWarning("WorkOrder with id {WorkOrderId} was not found", request.WorkOrderId);

            return ApplicationErrors.WorkOrderNotFound;
        }

        return workOrder.ToDto();     
    }
}