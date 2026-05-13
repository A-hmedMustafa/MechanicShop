using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOder;

public class RelocateWorkOrderCommandHandler(
    ILogger<RelocateWorkOrderCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    IWorkOrderPolicy WorkOrderValidator
    )
    : IRequestHandler<RelocateWorkOrderCommand, Result<Updated>>
{
    private readonly ILogger<RelocateWorkOrderCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly IWorkOrderPolicy _appointmentValidator = WorkOrderValidator;

    public async Task<Result<Updated>> Handle(RelocateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .Include(wOrder => wOrder.RepairTasks)
            .Include(wOrder => wOrder.Labor)
            .Include(wOrder => wOrder.Vehicle)
            .FirstOrDefaultAsync(wOrder => wOrder.Id == request.WorkOrderId,
                cancellationToken);

        if (workOrder is null)
        {
            _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", request.WorkOrderId);
            return ApplicationErrors.WorkOrderNotFound;
        }

        var duration = workOrder.EndsAtUtc.Subtract(workOrder.StartsAtUtc).Duration();
        var endsAt = request.NewStartAt.Add(duration);

        if (_appointmentValidator.IsOutsideOperatingHours(request.NewStartAt, duration))
        {
            _logger.LogError("Relocated WorkOrder time ({StartAt} → {EndAt}) is outside of store operating hours.",
                request.NewStartAt, endsAt);
            return ApplicationErrors.WorkOrderOutsideOperatingHour(request.NewStartAt, endsAt);
        }

        var spotAvailabilityCheckResult = await _appointmentValidator
            .CheckSpotAvailabilityAsync(
                request.NewSpot,
                request.NewStartAt,
                endsAt,
                request.WorkOrderId,
                cancellationToken);

        if (spotAvailabilityCheckResult.IsError)
        {
            _logger.LogError("Spot: {Spot} is not available.", request.NewSpot.ToString());
            return spotAvailabilityCheckResult.Errors;
        }

        if (await _appointmentValidator.IsLaborOccupied(workOrder.LaborId, workOrder.Id, request.NewStartAt, endsAt))
        {
            _logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", workOrder.LaborId);
            return ApplicationErrors.LaborOccupied;
        }

        if (await _appointmentValidator.IsVehicleAlreadyScheduled(workOrder.VehicleId, request.NewStartAt, endsAt, workOrder.Id))
        {
            _logger.LogError("Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", workOrder.VehicleId);
            return ApplicationErrors.VehicleSchedulingConflict;
        }

        var updateTimingResult = workOrder.UpdateTiming(request.NewStartAt, endsAt);

        if (updateTimingResult.IsError)
        {
            _logger.LogError("Failed to update timing: {Error}", updateTimingResult.TopError.Description);
            return updateTimingResult.Errors;
        }

        var updateSpotResult = workOrder.UpdateSpot(request.NewSpot);

        if (updateSpotResult.IsError)
        {
            _logger.LogError("Failed to update Spot: {Error}", updateSpotResult.TopError.Description);
            return updateSpotResult.Errors;
        }

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync("work-order", cancellationToken);

        return Result.Updated;
    }
}