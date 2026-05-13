using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(vehicle => vehicle.Make).NotEmpty().MaximumLength(50);

        RuleFor(vehicle => vehicle.Model).NotEmpty().MaximumLength(50);

        RuleFor(vehicle => vehicle.LicensePlate).NotEmpty().MaximumLength(10);
    }
}
