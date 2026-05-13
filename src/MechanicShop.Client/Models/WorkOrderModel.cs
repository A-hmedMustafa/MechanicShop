using System;
using System.Collections.Generic;
using MechanicShop.Contracts.Common;

namespace MechanicShop.Client.Models;

public class WorkOrderModel
{
    public Guid WorkOrderId {get; set; }
    public Spot Spot {get; set; }
    public VehicleModel? Vehicle {get; set; }
    public LaborModel? Labor {get; set; }
    public WorkOrderState State {get; set; }
    public DateTimeOffset StartsAtUtc {get; set; }
    public DateTimeOffset EndsAtUtc {get; set; }
    public List<RepairTaskModel> RepairTasks {get; set;} = [];
    public decimal TotalPartCost {get; set; }
    public decimal TotalLaborCost {get; set; }
    public decimal TotalCost {get; set; }
    public int TotalDurationInMins {get; set; }

}
