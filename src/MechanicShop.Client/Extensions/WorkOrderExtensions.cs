using System;
using MechanicShop.Client.Models;

namespace MechanicShop.Client.Extensions;

public static class WorkOrderExtensions
{
    public static void AdjustTimeToLocal(this WorkOrderModel workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        workOrder.StartsAtUtc = workOrder.StartsAtUtc.ToLocalTime();
        workOrder.EndsAtUtc = workOrder.EndsAtUtc.ToLocalTime();
    }

    
    public static void AdjustTimeToLocal(this WorkOrderListItemModel workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        workOrder.StartsAtUtc = workOrder.StartsAtUtc.ToLocalTime();
        workOrder.EndsAtUtc = workOrder.EndsAtUtc.ToLocalTime();
    }

}