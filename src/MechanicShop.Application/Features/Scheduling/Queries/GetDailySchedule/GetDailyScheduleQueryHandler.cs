using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labors.Mappers;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Scheduling.Queries.GetDailySchedule;


public class GetDailyScheduleQueryHandler(
    IAppDbContext context,
    TimeProvider datetime
) : IRequestHandler<GetDailyScheduleQuery, Result<ScheduleDto>>
{
    private readonly IAppDbContext _context = context;
    private readonly TimeProvider _datetime = datetime;

    public async Task<Result<ScheduleDto>> Handle(GetDailyScheduleQuery request, CancellationToken cancellationToken)
    {
        var localStart = request.ScheduleDate.ToDateTime(TimeOnly.MinValue);

        var localEnd = localStart.AddDays(1);

        var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, request.TimeZone);

        var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, request.TimeZone);

        var allWorkOrdersFromStartToEnd = await _context.WorkOrders 
            .Where(workOrder => workOrder.StartsAtUtc < utcEnd &&
                   workOrder.EndsAtUtc > utcStart &&
                   (request.LaborId == null || workOrder.LaborId == request.LaborId))
            .Include(workOrder => workOrder.RepairTasks)       
            .Include(workOrder => workOrder.Vehicle)       
            .Include(workOrder => workOrder.Labor)  
            .ToListAsync(cancellationToken);

        var now = TimeZoneInfo.ConvertTime(_datetime.GetUtcNow(), request.TimeZone);

        var result = new ScheduleDto
        {
            OnDate = request.ScheduleDate,
            EndOfDay = localEnd < now,
            Spots = []
        };

        foreach(var spot in Enum.GetValues<Spot>())
        {
            var current = localStart;
            var slots = new List<AvailabilitySlotDto>();

            var workOrderBySpot = allWorkOrdersFromStartToEnd
                .Where(workOrder => workOrder.Spot == spot)
                .OrderBy(workOrder => workOrder.StartsAtUtc)
                .ToList();

            while(current < localEnd)
            {
                var next = current.AddMinutes(15);

                var slotStartAtUtc = TimeZoneInfo.ConvertTimeToUtc(current, request.TimeZone);
                var slotEndAtUtc = TimeZoneInfo.ConvertTimeToUtc(next, request.TimeZone);

                var workOrderInCurrentSlot = workOrderBySpot
                    .FirstOrDefault(workOrder => workOrder.StartsAtUtc < slotEndAtUtc &&
                                    workOrder.EndsAtUtc > slotStartAtUtc);

                if(workOrderInCurrentSlot != null)
                {
                    if(!slots.Any(slot => slot.WorkOrderId == workOrderInCurrentSlot.Id))
                    {
                        slots.Add(new AvailabilitySlotDto
                        {
                            WorkOrderId = workOrderInCurrentSlot.Id,
                            Spot = spot,
                            StartsAt = workOrderInCurrentSlot.StartsAtUtc,
                            EndsAt = workOrderInCurrentSlot.EndsAtUtc,
                            Vehicle = FormatVehicleInfo(workOrderInCurrentSlot.Vehicle!),
                            Labor = workOrderInCurrentSlot.Labor!.ToDto(),
                            IsOccupied = true,
                            IsAvailable = false,
                            RepairTasks = [.. workOrderInCurrentSlot.RepairTasks.ToList().ConvertAll(repairTask => repairTask.ToDto())],
                            WorkOrderLocked = !workOrderInCurrentSlot.IsEditable,
                            State = workOrderInCurrentSlot.State
                        });
                    }
                }
                else
                {
                    slots.Add(new AvailabilitySlotDto
                    {
                        Spot = spot,
                        StartsAt = slotStartAtUtc,
                        EndsAt = slotEndAtUtc,
                        WorkOrderLocked = false,
                        IsAvailable = current >= now
                    });

                }                                    
                current = next;
            }    
            result.Spots.Add(new SpotDto
            {
                Spot = spot,
                Slots = slots
            });
        }
        return result;
    }   

    private static string? FormatVehicleInfo(Vehicle vehicle) =>
        vehicle is not null ? $"{vehicle.Make} | {vehicle.LicensePlate}" : null;
}