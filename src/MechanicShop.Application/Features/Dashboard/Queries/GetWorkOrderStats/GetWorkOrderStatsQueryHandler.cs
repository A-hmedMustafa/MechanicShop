using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats;


public class GetWorkOrderStatsQueryHandler(IAppDbContext context) : IRequestHandler<GetWorkOrderStatsQuery, Result<TodayWorkOrderStatsDto>>
{
    private readonly IAppDbContext _context = context;
    public async Task<Result<TodayWorkOrderStatsDto>> Handle(GetWorkOrderStatsQuery request, CancellationToken cancellationToken)
    {
        var start = request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = request.Date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var query = _context.WorkOrders
        .Include(workOrder => workOrder.Vehicle)
        .Include(workOrder => workOrder.RepairTasks).ThenInclude(repairTask => repairTask.Parts)
        .Include(workOrder => workOrder.Invoice)
        .Where(workOrder => workOrder.StartsAtUtc >= start && workOrder.EndsAtUtc <= end);

        var total = await query.CountAsync(cancellationToken);
        if(total == 0)
        {
            return new TodayWorkOrderStatsDto
            {
              Date = request.Date,
              Total = 0,
              Scheduled = 0,
              InProgress = 0,
              Completed = 0,
              Cancelled = 0,
              TotalRevenue = 0,
              TotalPartsCost = 0,
              TotalLaborCost = 0,
              UniqueVehicles = 0,
              UniqueCustomers = 0 
            };
        }

        var stats = await query.ToListAsync(cancellationToken);

        var totalRevenue = stats.Sum(stat => stat.Invoice?.Total ?? 0);
        var totalPartCost = stats.Where(stat=>stat.Invoice != null).Sum(stat => stat.TotalPartsCost ?? 0);
        var totalLaborCost = stats.Where(stat => stat.Invoice != null).Sum(stat => stat.TotalLaborCost ?? 0);
        var uniqueVehicles = stats.Select(stat => stat.VehicleId).Distinct().Count();
        var uniqueCustomers = stats.Select(stat => stat.Vehicle!.CustomerId).Distinct().Count();

        var netProfit = totalRevenue - totalPartCost - totalLaborCost;

        return new TodayWorkOrderStatsDto
        {
            Date = request.Date,
            Total = total,
            Scheduled = stats.Count(stat => stat.State == WorkOrderState.Scheduled),  
            InProgress = stats.Count(stat => stat.State == WorkOrderState.InProgress),  
            Completed = stats.Count(stat => stat.State == WorkOrderState.Completed),  
            Cancelled = stats.Count(stat => stat.State == WorkOrderState.Cancelled),
            TotalRevenue = totalRevenue,
            TotalPartsCost = totalPartCost,
            TotalLaborCost = totalLaborCost,
            UniqueVehicles = uniqueVehicles,
            UniqueCustomers = uniqueCustomers,
            NetProfit = netProfit,
            ProfitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0,
            CompletionRate = total > 0 ? ((decimal)stats.Count(stat => stat.State == WorkOrderState.Completed) / total) * 100 : 0,  
            AverageRevenuePerOrder = total > 0 ? totalRevenue / total : 0,
            OrdersPerVehicle = uniqueVehicles > 0 ? (decimal)total/uniqueVehicles : 0,
            PartsCostRatio = totalRevenue > 0 ? (totalPartCost / totalRevenue) * 100 : 0,
            LaborCostRatio = totalRevenue > 0 ? (totalLaborCost / totalRevenue) * 100 : 0,
            CancellationRate = total > 0 ? ((decimal)stats.Count(stat => stat.State == WorkOrderState.Cancelled) / total) * 100 : 0       
        };
    }
}