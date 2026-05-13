using System.Security.Principal;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;


public class GetWorkOrdersQueryHandler(
    IAppDbContext context)
    : IRequestHandler<GetWorkOrdersQuery, Result<PaginatedList<WorkOrderListItemDto>>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<PaginatedList<WorkOrderListItemDto>>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        var customizeWorkOrdersQuery = _context.WorkOrders
            .AsNoTracking()
            .Include(wOrder => wOrder.Vehicle!).ThenInclude(veh => veh.Customer)
            .Include(wOrder => wOrder.Labor)
            .Include(wOrder => wOrder.RepairTasks).ThenInclude(rTask => rTask.Parts)
            .Include(wOrder => wOrder.Invoice)
            .AsQueryable();    

        customizeWorkOrdersQuery = ApplyFilters(customizeWorkOrdersQuery, request);
        
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            customizeWorkOrdersQuery = ApplySearchTerm(customizeWorkOrdersQuery, request.SearchTerm);
        }

        customizeWorkOrdersQuery = ApplySorting(customizeWorkOrdersQuery, request.SortColumn, request.SortDirection);

        var workOrdersCount = await customizeWorkOrdersQuery.CountAsync(cancellationToken); 

        var paginatedResult = await customizeWorkOrdersQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(wOrder => new WorkOrderListItemDto
            {
                WorkOrderId = wOrder.Id,
                InvoiceId = wOrder.Invoice == null ? null : wOrder.Invoice.Id,
                Spot = wOrder.Spot,
                StartsAtUtc = wOrder.StartsAtUtc,
                EndsAtUtc = wOrder.EndsAtUtc,
                Vehicle = wOrder.Vehicle!.ToDto(),
                Customer = wOrder.Vehicle!.Customer!.Name,
                Labor = wOrder.Labor == null ? null : wOrder.Labor.FirstName + " " + wOrder.Labor.LastName,
                State = wOrder.State,
                RepairTasks = wOrder.RepairTasks.Select(rTask => rTask.Name).ToList()
            }).ToListAsync(cancellationToken);

        return new PaginatedList<WorkOrderListItemDto>
        {
            Items = paginatedResult,
            PageSize = request.PageSize,
            PageNumber = request.Page,
            TotalCount = workOrdersCount,
            TotalPages = (int)Math.Ceiling(workOrdersCount / (double)request.PageSize)
        };    
    }

    private static IQueryable<WorkOrder> ApplyFilters(IQueryable<WorkOrder> query, GetWorkOrdersQuery request)
    {
        if(request.State.HasValue)
            query = query.Where(wOrder => wOrder.State == request.State.Value);


        if(request.VehicleId.HasValue && request.VehicleId != Guid.Empty)
            query = query.Where(wOrder => wOrder.VehicleId == request.VehicleId.Value);


        if(request.LaborId.HasValue && request.LaborId != Guid.Empty)
            query = query.Where(wOrder => wOrder.LaborId == request.LaborId.Value);


        if(request.StartDateFrom.HasValue)
            query = query.Where(wOrder => wOrder.StartsAtUtc >= request.StartDateFrom.Value);


        if(request.StartDateTo.HasValue)
            query = query.Where(wOrder => wOrder.StartsAtUtc <= request.StartDateTo.Value);


        if(request.EndDateFrom.HasValue)
            query = query.Where(wOrder => wOrder.EndsAtUtc >= request.EndDateFrom.Value);


        if(request.EndDateTo.HasValue)
            query = query.Where(wOrder => wOrder.EndsAtUtc <= request.EndDateTo.Value); 


        if(request.Spot.HasValue)
            query = query.Where(wOrder => wOrder.Spot == request.Spot.Value);               
   
        return query;
    }

    private static IQueryable<WorkOrder> ApplySearchTerm(IQueryable<WorkOrder> query, string searchTerm)
    {
        var normalized = searchTerm.Trim().ToLower();

        return query.Where(wOrder =>
            (wOrder.Vehicle != null &&(
                wOrder.Vehicle.Make.ToLower().Contains(normalized)    ||
                wOrder.Vehicle.Model.ToLower().Contains(normalized)   ||
                wOrder.Vehicle.LicensePlate.ToLower().Contains(normalized)
            )) ||
            (wOrder.Labor != null && (
                wOrder.Labor.FirstName.ToLower().Contains(normalized) ||
                wOrder.Labor.LastName.ToLower().Contains(normalized)  ||
                (wOrder.Labor.FirstName + " " + wOrder.Labor.LastName).ToLower().Contains(normalized)
            )) ||
            wOrder.RepairTasks.Any(
                rTask => rTask.Name.ToLower().Contains(normalized))   ||
            wOrder.Id.ToString().ToLower().Contains(normalized));
    }

    private static IQueryable<WorkOrder> ApplySorting(IQueryable<WorkOrder> query, string sortColumn, string sortDirection)
    {
        var isDescending = sortDirection.Equals("desc", StringComparison.CurrentCultureIgnoreCase);

        return sortColumn.ToLower() switch
        {
            "createdat" => isDescending ? query.OrderByDescending(wOrder => wOrder.CreatedAtUtc)
                 : query.OrderBy(wOrder => wOrder.CreatedAtUtc),

            "updatedat" => isDescending ? query.OrderByDescending(wOrder => wOrder.LastModifiedUtc)
                 : query.OrderBy(wOrder => wOrder.LastModifiedUtc),

            "startat" => isDescending ? query.OrderByDescending(wOrder => wOrder.StartsAtUtc)
                : query.OrderBy(wOrder => wOrder.StartsAtUtc),  

            "endat" => isDescending ? query.OrderByDescending(wOrder => wOrder.EndsAtUtc)
                : query.OrderBy(wOrder => wOrder.EndsAtUtc), 

            "state" => isDescending ? query.OrderByDescending(wOrder => wOrder.State)
                : query.OrderBy(wOrder => wOrder.State),   

            "spot" => isDescending ? query.OrderByDescending(wOrder => wOrder.Spot)
                : query.OrderBy(wOrder => wOrder.Spot),   

            "total" => isDescending ? query.OrderByDescending(wOrder => wOrder.Total)
                : query.OrderBy(wOrder => wOrder.Total),  

            "vehicleid" => isDescending ? query.OrderByDescending(wOrder => wOrder.VehicleId)
                : query.OrderBy(wOrder => wOrder.VehicleId),   

            "laborid" => isDescending ? query.OrderByDescending(wOrder => wOrder.LaborId)
                : query.OrderBy(wOrder => wOrder.LaborId),   

            _ => query.OrderByDescending(wOrder => wOrder.CreatedAtUtc)
        };
    }
}