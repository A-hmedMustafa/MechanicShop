using System;
using MechanicShop.Contracts.Common;

namespace MechanicShop.Client.Models;

public class AvailabilitySlotModel
{
    public Guid? WorkOrderId {get; set; }
    public Spot Spot {get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public string Vehicle {get; set; } = string.Empty;
    public LaborModel? Labor {get; set; }
    public bool IsOccupied {get; set; }
    public bool IsAvailable {get; set; }
    public bool WorkOrderLocked {get; set; }
    public WorkOrderState? State {get; set; }
    public RepairTaskModel[] RepairTasks {get; set; } = [];
}