using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MechanicShop.Infrastructure.Services;

public class WorkOrderPolicy(
    IOptions<AppSettings> appSettings,
    IAppDbContext context
) : IWorkOrderPolicy
{
    private readonly AppSettings _appSettings = appSettings.Value;
    private readonly IAppDbContext _context = context;

    public async Task<Result<Success>> CheckSpotAvailabilityAsync(Spot spot, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludeWorkOrderId = null, CancellationToken cancellationToken = default)
    {
        var isOccupied = await _context.WorkOrders
            .AnyAsync(wOrder => 
                wOrder.Spot == spot           && 
                wOrder.StartsAtUtc < endAt    && 
                wOrder.EndsAtUtc > startAt    &&
                (!excludeWorkOrderId.HasValue || wOrder.Id != excludeWorkOrderId.Value),
                cancellationToken
            );

        return isOccupied 
            ? Error.Conflict("MechanicShop_Spot_Full", "The selected time slot is unavailable for the requested services.")
            : Result.Success;
    }

    public async Task<bool> IsLaborOccupied(Guid laborId, Guid excludedWorkOrderId, DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        var isOccupied = await _context.WorkOrders
        .AnyAsync(wOrder => 
            wOrder.LaborId == laborId   &&
            wOrder.StartsAtUtc < endsAt &&
            wOrder.EndsAtUtc > startsAt &&
            wOrder.Id != excludedWorkOrderId
        );

       return isOccupied;
    }

    public bool IsOutsideOperatingHours(DateTimeOffset startsAt, TimeSpan duration)
    {
        var opensAt = startsAt.Date.Add(_appSettings.OpeningTime.ToTimeSpan());
        var closesAt = startsAt.Date.Add(_appSettings.ClosingTime.ToTimeSpan());

        var taskEndsAt = startsAt + duration;

        return startsAt < opensAt || taskEndsAt > closesAt;
    
    }

    public async Task<bool> IsVehicleAlreadyScheduled(Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludedWorkOrderId = null)
    {
        var isScheduled = await _context.WorkOrders.AnyAsync(wOrder =>
            wOrder.VehicleId == vehicleId &&
            wOrder.StartsAtUtc < endAt &&
            wOrder.EndsAtUtc > startAt &&
            (wOrder.Id != excludedWorkOrderId || excludedWorkOrderId == null)
        );
        return isScheduled;
    }

    public Result<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if((endAt - startAt) < TimeSpan.FromMinutes(_appSettings.MinAppointmentDurationInMinutes))
        {
            return Error.Conflict(
                "WorkOrder_TooShort",
                $"WorkOrder duration must be at least {_appSettings.CancellationDeadlineInMinutes} minutes."
            );
        }
        return Result.Success;
    }


}