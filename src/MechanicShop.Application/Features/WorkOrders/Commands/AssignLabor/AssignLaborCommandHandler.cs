using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MechanicShop.Application.Common.Errors;

namespace MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;

public class AssignLaborCommandHandler(
    ILogger<AssignLaborCommandHandler> logger,
    HybridCache cache,
    IAppDbContext context,
    IWorkOrderPolicy workOrderValidator
) : IRequestHandler<AssignLaborCommand, Result<Updated>>
{
    private readonly ILogger<AssignLaborCommandHandler> _logger = logger;
    private readonly HybridCache _cache = cache;
    private readonly IAppDbContext _context = context;
    private readonly IWorkOrderPolicy _workOrderValidator = workOrderValidator;

    public async Task<Result<Updated>> Handle(AssignLaborCommand request, CancellationToken cancellationToken)
    {
         var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(a => a.Id == request.WorkOrderId, cancellationToken);

        if (workOrder is null)
        {
            _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", request.WorkOrderId);
           
            return ApplicationErrors.WorkOrderNotFound;
        }

        var labor = await _context.Employees.FindAsync([request.LaborId], cancellationToken);

        if (labor is null)
        {
            _logger.LogError("Invalid LaborId: {LaborId}", request.LaborId);
           
            return ApplicationErrors.LaborNotFound;
        }

        if(await _workOrderValidator.IsLaborOccupied(request.LaborId, request.WorkOrderId, workOrder.StartsAtUtc, workOrder.EndsAtUtc))
        {
            _logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", workOrder.LaborId);
          
            return ApplicationErrors.LaborOccupied;
        }

        var LaborUpdateResult = workOrder.UpdateLabor(request.LaborId);
        
        if (LaborUpdateResult.IsError)
        {
            foreach (var error in LaborUpdateResult.Errors)
            {
                _logger.LogError("[LaborUpdate] {ErrorCode}: {ErrorDescription}", error.Code, error.Description);
            }

            return LaborUpdateResult.Errors;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync("work-order", cancellationToken);

        return Result.Updated;
    }
}