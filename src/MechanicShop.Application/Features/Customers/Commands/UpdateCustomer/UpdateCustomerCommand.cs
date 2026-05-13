using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Domain.Common.Results;
using MediatR;

public sealed record UpdateCustomerCommand(
    Guid CustomerId,
    string Name,
    string PhoneNumber,
    string Email,
    List<UpdateVehicleCommand> Vehicles

) : IRequest<Result<Updated>>;