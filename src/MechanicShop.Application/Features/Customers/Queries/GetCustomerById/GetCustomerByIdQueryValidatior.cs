using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(customer => customer.CustomerId)
            .NotEmpty()
            .WithErrorCode("CustomerId_Is_Required")
            .WithMessage("CustomerId Is Required.");
    }
}