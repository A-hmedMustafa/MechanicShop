using Asp.Versioning;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Application.Features.Scheduling.Queries.GetDailySchedule;
using MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOder;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderByIdQuery;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;
using MechanicShop.Contracts.Requests.WorkOrders;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.WorkOrders.Enums;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace MechanicShop.Api.Controllers;


[Route("api/v{version:apiVersion}/workorders")]
[ApiVersion("1.0")]
[Authorize]
public sealed class WorkOrdersController(
    ISender sender
) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<WorkOrderListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a paginated list of work orders.")]
    [EndpointDescription(
        "Supports filtering by date range, status, vehicle, labor, spot, and searching by term. Pagination and sorting are supported.")]
    [EndpointName("GetWorkOrders")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Get(
        [FromQuery] WorkOrderFilterRequest filterRequest,
        [FromQuery] PageRequest pageRequest, 
        CancellationToken cancellationToken)
    {
        if (pageRequest.Page <= 0)
        {
            return BadRequest("Page must be greater than 0");
        }

        if (pageRequest.PageSize <= 0 || pageRequest.PageSize > 100)
        {
            return BadRequest("PageSize must be between 1 and 100");
        }

        var query = new GetWorkOrdersQuery(
            pageRequest.Page,
            pageRequest.PageSize,
            filterRequest.SearchTerm,
            filterRequest.SortColumn,
            filterRequest.SortDirection,
            filterRequest.State is not null ? (WorkOrderState)(int)filterRequest.State : null,
            filterRequest.VehicleId,
            filterRequest.LaborId,
            filterRequest.StartDateFrom,
            filterRequest.StartDateTo,
            filterRequest.EndDateFrom,
            filterRequest.EndDateTo,
            filterRequest.Spot is not null ? (Spot)(int)filterRequest.Spot : null
        );

        var result = await sender.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            Problem 
        );

    }


    
    [HttpGet("{workOrderId:guid}", Name = "GetWorkOrderById")]
    [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a work order by its ID.")]
    [EndpointDescription(
        "Returns detailed information about the specified work order if it exists.")]
    [EndpointName("GetWorkOrderById")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetById(Guid workOrderId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkOrderByIdQuery(workOrderId), cancellationToken);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpPost]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Creates a new work order.")]
    [EndpointDescription(
        "Creates a new work order for a vehicle, specifying labor, tasks, and other required information.")]
    [EndpointName("CreateWorkOrder")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateWorkOrderCommand(
            (Spot)(int)request.Spot, 
            request.VehicleId, 
            request.StartAtUtc, 
            request.RepairTaskIds, 
            request.LaborId);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            response => CreatedAtRoute(
                routeName: "GetWorkOrderById",
                routeValues: new {version = "1.0", workOrderId = response.WorkOrderId},
                value: response
            ),
            Problem
        );    
    }

    [HttpPut("{workOrderId:guid}/relocation")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Relocates a work order to a new time and spot.")]
    [EndpointDescription(
        "Updates the scheduled time and assigned bay for a work order. Only users with the Manager role can perform this action.")]
    [EndpointName("RescheduleWorkOrder")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Relocate(
        Guid workOrderId,
        RelocateWorkOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RelocateWorkOrderCommand(
            workOrderId,
            request.NewStartAtUtc,
            (Spot)(int)request.NewSpot);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(_ => NoContent(), Problem);
    }

    
    [HttpPut("{workOrderId:guid}/labor")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Assigns a labor to a work order.")]
    [EndpointDescription(
        "Associates a labor definition with a specific work order. Only managers can perform this operation.")]
    [EndpointName("AssignLaborToWorkOrder")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> AssignLabor(
        Guid workOrderId,
        AssignLaborRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignLaborCommand(workOrderId, Guid.Parse(request.LaborId));

        var result = await sender.Send(command, cancellationToken);

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPut("{workOrderId:guid}/state")]
    [Authorize(
        Roles = $"{nameof(Role.Manager)},{nameof(Role.Labor)}",
        Policy = "AssignedLaborOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Changes the state of a work order.")]
    [EndpointDescription(
        "Updates the current state of the specified work order. Only users with the Manager role are authorized.")]
    [EndpointName("UpdateWorkOrderState")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateState(
        Guid workOrderId,
        UpdateWorkOrderStateRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateWorkOrderStateCommand(
            workOrderId,
            (WorkOrderState)(int)request.State);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(_ => NoContent(), Problem);
    }
    
    [HttpPut("{workOrderId:guid}/repair-task")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRepairTasks(
        Guid workOrderId,
        ModifyRepairTaskRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateWorkOrderRepairTasksCommand(workOrderId, request.RepairTaskIds);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(_ => NoContent(), Problem);
    }

    
    [HttpDelete("{workOrderId:guid}")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Deletes a work order.")]
    [EndpointDescription(
        "Deletes the specified work order permanently. Only users with the Manager role are authorized.")]
    [EndpointName("DeleteWorkOrder")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid workOrderId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteWorkOrderCommand(workOrderId), cancellationToken);

        return result.Match(_ => NoContent(), Problem);
    }


    [HttpGet("schedule/{date}")]
    [Authorize]
    [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves the schedule for a given day.")]
    [EndpointDescription(
        "Returns a schedule view for the specified date. If no date is provided, today's schedule is returned. You can optionally filter by labor ID.")]
    [EndpointName("GetDailySchedule")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetSchedule(
        DateOnly? date,
        [FromQuery] Guid? laborId,
        [FromHeader(Name = "X-TimeZone")] string? timeZone,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
        {
            return Problem(
                detail: "Missing Time Zone in 'X-TimeZone' header",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Time Zone Required"    
                );
        }

        TimeZoneInfo tZone;

        try
        {
            tZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        }
        catch
        {
            return Problem(
                detail: $"Invalid or unknown time zone: '{timeZone}'.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Time Zone");
        }

        var scheduleDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var query = new GetDailyScheduleQuery(tZone, scheduleDate, laborId);
        var result = await sender.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response), Problem
        );
    }
        

}