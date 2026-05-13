using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed record CreateWorkOrderCommand(
    Spot Spot,
    Guid VehicleId,
    DateTimeOffset StartsAt,
    List<Guid> RepairTaskIds,
    Guid? LaborId
) : IRequest<Result<WorkOrderDto>>;