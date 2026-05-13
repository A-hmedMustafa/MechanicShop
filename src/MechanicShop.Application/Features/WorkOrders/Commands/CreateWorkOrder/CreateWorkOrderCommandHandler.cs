using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public class CreateWorkOrderCommandHandler(
    ILogger<CreateWorkOrderCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    IWorkOrderPolicy workOrderValidator
    )
    : IRequestHandler<CreateWorkOrderCommand, Result<WorkOrderDto>>
{
    private readonly ILogger<CreateWorkOrderCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly IWorkOrderPolicy _workOrderPolicy = workOrderValidator;

    public async Task<Result<WorkOrderDto>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var repairTasksInRequest = await _context.RepairTasks
        .Where(repairTask => request.RepairTaskIds.Contains(repairTask.Id))
        .ToListAsync(cancellationToken);

        if(repairTasksInRequest.Count != request.RepairTaskIds.Count)
        {
            var missingIdsFromRequest = request.RepairTaskIds
                .Except(repairTasksInRequest
                .Select(rTask => rTask.Id))
                .ToArray();

            _logger.LogError("Some RepairTaskIds Not Found : {MissingIds}", 
                string.Join(",", missingIdsFromRequest));

            return ApplicationErrors.RepairTaskNotFound;
        }

        var totalEstimatedDuration = TimeSpan.FromMinutes(
            repairTasksInRequest.Sum(repairTask => (int)repairTask.EstimatedDurationInMins));

        var endsAt = request.StartsAt.Add(totalEstimatedDuration);

        if(_workOrderPolicy.IsOutsideOperatingHours(request.StartsAt, totalEstimatedDuration))
        {
            _logger.LogError("WorkOrder time ({StartAt} ? {EndAt}) is outside of store openings hours.", request.StartsAt, endsAt);

            return ApplicationErrors.WorkOrderOutsideOperatingHour(request.StartsAt, endsAt);
        }

        var checkMinRequirementResult = _workOrderPolicy.ValidateMinimumRequirement(request.StartsAt, endsAt);

        if (checkMinRequirementResult.IsError)
        {
            _logger.LogError("WorkOrder duration is shorter than the configured minimum.");

            return checkMinRequirementResult.Errors;
        }

        var spotAvailabilityCheckResult = await _workOrderPolicy.CheckSpotAvailabilityAsync(
            request.Spot,
            request.StartsAt,
            endsAt,
            null,
            cancellationToken
        );
        if (spotAvailabilityCheckResult.IsError)
        {
            _logger.LogError("Spot: {Spot} is not available", request.Spot.ToString());

            return spotAvailabilityCheckResult.Errors;
        }

        var vehicle = await _context.Vehicles
            .Include(v => v.Customer)
            .FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken);


        if (vehicle is null)
        {
            _logger.LogError("Vehicle with Id '{VehicleId}' does not exist.", request.VehicleId);

            return ApplicationErrors.VehicleNotFound;
        }

        var labor = await _context.Employees.FindAsync([request.LaborId], cancellationToken);

        if(labor is null)
        {
            _logger.LogError("Invalid LaborId: {LaborId}", request.LaborId.ToString());

            return ApplicationErrors.LaborNotFound;
        }

        var hasVehicleConflict = await _context.WorkOrders
            .AnyAsync( workOrder =>
                       workOrder.VehicleId == request.VehicleId &&
                       workOrder.StartsAtUtc.Date == request.StartsAt.Date &&
                       workOrder.StartsAtUtc < endsAt &&
                       workOrder.EndsAtUtc > request.StartsAt,
                       cancellationToken);

        if (hasVehicleConflict)
        {
            _logger.LogError("Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", request.VehicleId);
            return Error.Conflict(
                code: "Vehicle_Overlapping_WorkOrders",
                description: "The vehicle already has an overlapping WorkOrder.");
        }

        var isLaborOccupied = await _context.WorkOrders
            .AnyAsync( workOrder =>
                       workOrder.LaborId == request.LaborId &&
                       workOrder.StartsAtUtc < endsAt &&
                       workOrder.EndsAtUtc > request.StartsAt,
                       cancellationToken);

        if (isLaborOccupied)
        {
            _logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", request.LaborId);
            return Error.Conflict(
                code: "Labor_Occupied",
                description: "Labor is already occupied during the requested time.");
        }              

        var createWorkOrderResult = WorkOrder.Create(
            Guid.NewGuid(),
            request.VehicleId,
            request.StartsAt,
            endsAt,
            request.LaborId!.Value,
            request.Spot,
            repairTasksInRequest
        );

        if (createWorkOrderResult.IsError)
        {
            _logger.LogError("Failed to create WorkOrder: {Error}", createWorkOrderResult.TopError.Description);

            return createWorkOrderResult.Errors;
        } 

        var workOrder = createWorkOrderResult.Value;

        _context.WorkOrders.Add(workOrder);

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _context.SaveChangesAsync(cancellationToken);

        workOrder.Vehicle = vehicle;

        workOrder.Labor = labor;

        _logger.LogInformation("WorkOrder with Id '{WorkOrderId}' created successfully.", workOrder.Id);

        await _cache.RemoveByTagAsync("work-order", cancellationToken);

        return workOrder.ToDto();
    }
}