using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.DeleteCustomer;

public class RemoveCustomerCommandValidatorTests
{
    private readonly RemoveCustomerCommandValidator _validator;

    public RemoveCustomerCommandValidatorTests()
    {
        _validator = new RemoveCustomerCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCustomerId_ShouldSucceed()
    {
        var removeCustomerCommand = new RemoveCustomerCommand(Guid.NewGuid());

        var removeCustomerCommandResult = _validator.Validate(removeCustomerCommand);

        Assert.True(removeCustomerCommandResult.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyCustomerId_ShouldFail()
    {
        var removeCustomerCommand = new RemoveCustomerCommand(Guid.Empty);

        var removeCustomerCommandResult = _validator.Validate(removeCustomerCommand);

        Assert.False(removeCustomerCommandResult.IsValid);
        Assert.Contains(removeCustomerCommandResult.Errors, e => e.PropertyName == "CustomerId");
    }
}