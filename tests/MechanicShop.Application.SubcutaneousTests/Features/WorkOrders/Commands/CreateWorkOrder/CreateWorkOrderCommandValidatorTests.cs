using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Domain.WorkOrders.Enums;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.CreateWorkOrder;

public class CreateWorkOrderCommandValidatorTests
{
    private readonly CreateWorkOrderCommandValidator _validator;
    public CreateWorkOrderCommandValidatorTests()
    {
        _validator = new CreateWorkOrderCommandValidator();
    }

    [Fact]
    public void Validate_WhenVehicleIdEmpty_ShouldFail()
    {
        var creationCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: Guid.Empty,
            StartsAt: DateTimeOffset.UtcNow.AddHours(1),
            RepairTaskIds: [Guid.NewGuid()],
            LaborId: Guid.NewGuid()
        );

        var commandCreationResult = _validator.Validate(creationCommand);

        Assert.False(commandCreationResult.IsValid);
        Assert.Contains(commandCreationResult.Errors, failure => failure.PropertyName == "VehicleId");
    }
    [Fact]
    public void Validate_WhenStartDateInThePast_ShouldFail()
    {
         var creationCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: Guid.NewGuid(),
            StartsAt: DateTimeOffset.UtcNow.AddHours(-1),
            RepairTaskIds: [Guid.NewGuid()],
            LaborId: Guid.NewGuid()
        );

        var commandCreationResult = _validator.Validate(creationCommand);
        
        Assert.False(commandCreationResult.IsValid);
        Assert.Contains(commandCreationResult.Errors, failure => failure.PropertyName == "StartsAt");
    }

    [Fact]
    public void Validate_WhenRepairTasksIdsEmpty_ShouldFail()
    {
         var creationCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: Guid.NewGuid(),
            StartsAt: DateTimeOffset.UtcNow.AddHours(1),
            RepairTaskIds: [],
            LaborId: Guid.NewGuid()
        );

        var commandCreationResult = _validator.Validate(creationCommand);
        
        Assert.False(commandCreationResult.IsValid);
        Assert.Contains(commandCreationResult.Errors, failure => failure.PropertyName == "RepairTaskIds");
    }

    [Fact]
    public void Validate_LaborIdEmpty_ShouldFail()
    {
         var creationCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: Guid.NewGuid(),
            StartsAt: DateTimeOffset.UtcNow.AddHours(1),
            RepairTaskIds: [Guid.NewGuid()],
            LaborId: Guid.Empty
        );

        var commandCreationResult = _validator.Validate(creationCommand);
        
        Assert.False(commandCreationResult.IsValid);
        Assert.Contains(commandCreationResult.Errors, failure => failure.PropertyName == "LaborId");
    }

    [Fact]
    public void Validate_WhenSpotInvalid_ShouldFail()
    {
        var creationCommand = new CreateWorkOrderCommand(
            Spot: (Spot)800,
            VehicleId: Guid.NewGuid(),
            StartsAt: DateTimeOffset.UtcNow.AddHours(1),
            RepairTaskIds: [Guid.NewGuid()],
            LaborId: Guid.NewGuid()
        );

        var commandCreationResult = _validator.Validate(creationCommand);
        
        Assert.False(commandCreationResult.IsValid);
        Assert.Contains(commandCreationResult.Errors, failure => failure.PropertyName == "Spot");
    }

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var creationCommand = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: Guid.NewGuid(),
            StartsAt: DateTimeOffset.UtcNow.AddHours(1),
            RepairTaskIds: [Guid.NewGuid()],
            LaborId: Guid.NewGuid()
        );

        var commandCreationResult = _validator.Validate(creationCommand);
        
        Assert.True(commandCreationResult.IsValid);
    }
}