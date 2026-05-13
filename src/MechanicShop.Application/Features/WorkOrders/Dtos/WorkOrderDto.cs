using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Labor.Dtos;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.WorkOrders.Dtos;

public class WorkOrderDto
{
    public Guid WorkOrderId {get; set; }
    public Guid? InvoiceId {get; set; }
    public Spot Spot {get; set; }
    public VehicleDto? Vehicle {get; set; }
    public LaborDto? Labor {get; set; }
    public WorkOrderState State {get; set; }
    public DateTimeOffset StartsAtUtc {get; set; }
    public DateTimeOffset EndsAtUtc {get; set; }
    public List<RepairTaskDto> RepairTasks {get; set;} = [];
    public decimal TotalPartCost {get; set; }
    public decimal TotalLaborCost {get; set; }
    public decimal TotalCost {get; set; }
    public int TotalDurationInMins {get; set; }
    public DateTimeOffset CreatedAt {get; set; }

}
