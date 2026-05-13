using System.Net.Cache;
using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;


public sealed class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderCommandValidator()
    {
        RuleFor(request => request.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId Is Required.");

        RuleFor(request => request.StartsAt)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("StartAt Must Be In The Future.");

        RuleFor(request => request.RepairTaskIds)
            .NotEmpty()
            .WithMessage("At least one repair task must be selected");
        RuleFor(request => request.LaborId)
            .Must(request => request is null || request != Guid.Empty)
            .WithMessage("If provided, LaborId must not be empty.");
        RuleFor(request => request.Spot)
            .IsInEnum()
            .WithErrorCode("Spot_Invalid")
            .WithMessage("Spot must be a valid Spot value. [A, B, C, D]");
    }
}