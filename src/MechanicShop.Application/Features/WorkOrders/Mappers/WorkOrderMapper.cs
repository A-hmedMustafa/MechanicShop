using System.Net;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Application.Features.Labor.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.Features.WorkOrders.Mappers;

public static class WorkOrderMapper
{
    public static WorkOrderDto ToDto(this WorkOrder workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        return new WorkOrderDto
        {
            WorkOrderId = workOrder.Id,
            Spot = workOrder.Spot,
            StartsAtUtc = workOrder.StartsAtUtc,
            EndsAtUtc = workOrder.EndsAtUtc,
            Labor = workOrder.Labor is null ? null : new LaborDto
            {
                LaborId = workOrder.LaborId,
                Name = $"{workOrder.Labor.FirstName} {workOrder.Labor.LastName}"
            },
            RepairTasks = workOrder.RepairTasks.ToDtos(),
            Vehicle = workOrder.Vehicle is null ? null : workOrder.Vehicle.ToDto(),
            State = workOrder.State,
            TotalPartCost = workOrder.RepairTasks.SelectMany(repairTask => repairTask.Parts).Sum(part => part.Cost * part.Quantity),
            TotalLaborCost = workOrder.RepairTasks.Sum(repairTask => repairTask.LaborCost),
            TotalCost = workOrder.RepairTasks.Sum(repairTask => repairTask.TotalCost),
            TotalDurationInMins = workOrder.RepairTasks.Sum(repairTask => (int)repairTask.EstimatedDurationInMins),
            InvoiceId = workOrder.Invoice?.Id,
            CreatedAt = workOrder.CreatedAtUtc
        };
    }

    public static List<WorkOrderDto> ToDtos(this IEnumerable<WorkOrder> workOrders)
    {
        return [.. workOrders.Select(workOrder => workOrder.ToDto())];
    }

    
    public static WorkOrderListItemDto ToListItemDto(this WorkOrder workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        return new WorkOrderListItemDto
        {
            WorkOrderId = workOrder.Id,
            Spot = workOrder.Spot,
            StartsAtUtc = workOrder.StartsAtUtc,
            EndsAtUtc = workOrder.EndsAtUtc,
            Vehicle = workOrder.Vehicle!.ToDto(),
            Labor = workOrder.Labor is null ? null :
                $"{workOrder.Labor.FirstName} {workOrder.Labor.LastName}",
            State = workOrder.State,
            RepairTasks = workOrder.RepairTasks.Select(rt => rt.Name).ToList()
        };
    }
}