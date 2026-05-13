using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;


public class UpdateWorkOrderStateCommandHandler(
    ILogger<UpdateWorkOrderStateCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    TimeProvider dateTime
    )
    : IRequestHandler<UpdateWorkOrderStateCommand, Result<Updated>>
{
    private readonly ILogger<UpdateWorkOrderStateCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly TimeProvider _dateTime = dateTime;

    public async Task<Result<Updated>> Handle(UpdateWorkOrderStateCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
        .FirstOrDefaultAsync(wOrder => wOrder.Id == request.WorkOrderId,
            cancellationToken);

         if (workOrder is null)
        {
            _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", request.WorkOrderId);

            return ApplicationErrors.WorkOrderNotFound;
        }

        if(workOrder.StartsAtUtc > _dateTime.GetUtcNow() && request.State == WorkOrderState.Cancelled)
        {
            _logger.LogError("State transition for WorkOrder Id '{WorkOrderId}` is not allowed before the work order�s scheduled start time.", request.WorkOrderId);

            return WorkOrderErrors.StateTransitionNotAllowed(workOrder.StartsAtUtc);
        }

        var updateStatusResult = workOrder.UpdateState(request.State);

        if (updateStatusResult.IsError)
        {
            _logger.LogError("Failed to update status: {Error}", updateStatusResult.TopError.Description);

            return updateStatusResult.Errors;
        }

        if(request.State == WorkOrderState.Completed)
            workOrder.AddDomainEvent(new WorkOrderCompleted {WorkOrderId = request.WorkOrderId});

        await _context.SaveChangesAsync(cancellationToken);

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _cache.RemoveByTagAsync("work-order", cancellationToken);

        return Result.Updated;
    }
}