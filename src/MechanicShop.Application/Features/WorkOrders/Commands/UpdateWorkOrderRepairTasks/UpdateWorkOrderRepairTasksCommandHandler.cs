using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public class UpdateWorkOrderRepairTasksCommandHandler(
    ILogger<UpdateWorkOrderRepairTasksCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    IWorkOrderPolicy workOrderValidator
) : IRequestHandler<UpdateWorkOrderRepairTasksCommand, Result<Updated>>
{
    private readonly ILogger<UpdateWorkOrderRepairTasksCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly IWorkOrderPolicy _workOrderValidator = workOrderValidator;

    public async Task<Result<Updated>> Handle(UpdateWorkOrderRepairTasksCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .Include(wOrder => wOrder.RepairTasks)
            .FirstOrDefaultAsync(wOrder => wOrder.Id == request.WorkOrderId,
                cancellationToken);

        if (workOrder is null)
        {
            _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", request.WorkOrderId);
            return ApplicationErrors.WorkOrderNotFound;
        }

        if (request.RepairTaskIds.Length == 0)
        {
            _logger.LogError("Empty RepairTaskIds list submitted.");
            return RepairTaskErrors.AtLeastOneRepairTaskIsRequired;
        }

        // duplicate check
        if (request.RepairTaskIds.Distinct().Count() != request.RepairTaskIds.Length)
        {
            _logger.LogError("Duplicate RepairTaskIds submitted.");
            return Error.Conflict(
                "WorkOrderErrors.RepairTaskAlreadyAdded",
                "Duplicate repair task IDs are not allowed.");
        }

        var repairTaskInRequest = await _context.RepairTasks
            .Where(repairTask => request.RepairTaskIds.Contains(repairTask.Id))
            .ToListAsync(cancellationToken);

        if (repairTaskInRequest.Count != request.RepairTaskIds.Count())
        {
            var missingIdsFromRequest = request.RepairTaskIds
                .Except(repairTaskInRequest.Select(rTask => rTask.Id))
                .ToArray();

            _logger.LogError("One Or More Repair Tasks Not Found. {Ids}", string.Join(",", missingIdsFromRequest));
            return ApplicationErrors.RepairTaskNotFound;
        }

        var clearRepairTasksResult = workOrder.ClearRepairTasks();

        if (clearRepairTasksResult.IsError)
        {
            return clearRepairTasksResult.Errors;
        }

        foreach (var task in repairTaskInRequest)
        {
            var addRepairTaskResult = workOrder.AddRepairTask(task);

            if (addRepairTaskResult.IsError)
            {
                return addRepairTaskResult.Errors;
            }
        }

        var totalDuration = TimeSpan.FromMinutes(
            repairTaskInRequest.Sum(rTask => (int)rTask.EstimatedDurationInMins));

        var newEndAt = workOrder.StartsAtUtc + totalDuration;

        if (_workOrderValidator.IsOutsideOperatingHours(workOrder.StartsAtUtc, totalDuration))
        {
            return Error.Conflict(
                "WorkOrder_Outside_OperatingHours",
                "WorkOrder timing exceeds business hours.");
        }

        var spotAvailabilityCheck = await _workOrderValidator.CheckSpotAvailabilityAsync(
            workOrder.Spot,
            workOrder.StartsAtUtc,
            newEndAt,
            workOrder.Id,
            cancellationToken);

        if (spotAvailabilityCheck.IsError)
        {
            return spotAvailabilityCheck.Errors;
        }

        if (await _workOrderValidator.IsLaborOccupied(
            workOrder.LaborId, workOrder.Id, workOrder.StartsAtUtc, newEndAt))
        {
            return ApplicationErrors.LaborOccupied;
        }

        var updateTimingResult = workOrder.UpdateTiming(workOrder.StartsAtUtc, newEndAt);

        if (updateTimingResult.IsError)
        {
            _logger.LogError("Failed to update timing: {Error}", updateTimingResult.TopError.Description);
            return updateTimingResult.Errors;
        }

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync("work-order", cancellationToken);

        return Result.Updated;
    }
}