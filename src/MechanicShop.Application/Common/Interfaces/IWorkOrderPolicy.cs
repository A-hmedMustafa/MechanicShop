using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Common.Interfaces;

public interface IWorkOrderPolicy
{
    bool IsOutsideOperatingHours(DateTimeOffset startsAt, TimeSpan duration);
    
    Task<bool> IsLaborOccupied(Guid laborId, Guid excludedWorkOrderId, DateTimeOffset startsAt, DateTimeOffset endsAt);
    
    Task<bool> IsVehicleAlreadyScheduled(Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludedWorkOrderId = null);

    Task<Result<Success>> CheckSpotAvailabilityAsync(Spot spot, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludeWorkOrderId = null, CancellationToken cancellationToken = default);

    Result<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt);
}