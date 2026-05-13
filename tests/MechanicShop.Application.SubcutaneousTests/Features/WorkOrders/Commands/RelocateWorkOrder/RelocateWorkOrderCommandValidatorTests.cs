using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOder;
using MechanicShop.Domain.WorkOrders.Enums;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.RelocateWorkOrder;

public class RelocateWorkOrderCommandValidatorTests
{
    private readonly RescheduleAppointmentCommandValidator _validator;
    public RelocateWorkOrderCommandValidatorTests()
    {
        _validator = new RescheduleAppointmentCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {     
        var relocationCommand = new RelocateWorkOrderCommand(
            WorkOrderId: Guid.NewGuid(),
            NewStartAt: DateTimeOffset.UtcNow.AddDays(1).AddHours(12),
            NewSpot: Spot.A
        );

        var relocationCommandResult = _validator.Validate(relocationCommand);

        Assert.True(relocationCommandResult.IsValid);
    }


    [Fact]
    public void Validate_WhenWorkOrderIdEmpty_ShouldFail()
    {
        var relocationCommand = new RelocateWorkOrderCommand(
            WorkOrderId: Guid.Empty,
            NewStartAt: DateTimeOffset.UtcNow.AddDays(1).AddHours(12),
            NewSpot: Spot.A
        );

        var relocationCommandResult = _validator.Validate(relocationCommand);

        Assert.False(relocationCommandResult.IsValid);
        Assert.Contains(relocationCommandResult.Errors, e => e.PropertyName == "WorkOrderId");

    }


    [Fact]
    public void Validate_WithInvalidSpot_ShouldFail()
    {
        var relocationCommand = new RelocateWorkOrderCommand(
            WorkOrderId: Guid.NewGuid(),
            NewStartAt: DateTimeOffset.UtcNow.AddDays(1).AddHours(12),
            NewSpot: (Spot)503
        );

        var relocationCommandResult = _validator.Validate(relocationCommand);

        Assert.False(relocationCommandResult.IsValid);
        Assert.Contains(relocationCommandResult.Errors, e => e.PropertyName == "NewSpot");
    }


    [Fact]
    public void Validate_WhenStartInThePast_ShouldFail()
    {
        var relocationCommand = new RelocateWorkOrderCommand(
            WorkOrderId: Guid.NewGuid(),
            NewStartAt: DateTimeOffset.UtcNow.AddDays(-1).AddHours(12),
            NewSpot: Spot.A
        );

        var relocationCommandResult = _validator.Validate(relocationCommand);

        Assert.False(relocationCommandResult.IsValid);
        Assert.Contains(relocationCommandResult.Errors, e => e.PropertyName == "NewStartAt");
    }
}