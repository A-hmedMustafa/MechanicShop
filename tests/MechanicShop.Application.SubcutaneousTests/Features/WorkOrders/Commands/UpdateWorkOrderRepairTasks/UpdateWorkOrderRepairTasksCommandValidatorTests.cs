using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using Microsoft.Identity.Client;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public class UpdateWorkOrderRepairTasksCommandValidatorTests
{
    private readonly UpdateWorkOrderRepairTasksCommandValidator _validator;

    public UpdateWorkOrderRepairTasksCommandValidatorTests()
    {
        _validator = new UpdateWorkOrderRepairTasksCommandValidator();        
    }

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var repairTasksUpdateCommand = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: Guid.NewGuid(),
            RepairTaskIds: [Guid.NewGuid()]);

        var repairTasksUpdateCommandResult = _validator.Validate(repairTasksUpdateCommand);

        Assert.True(repairTasksUpdateCommandResult.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyWorkOrderId_ShouldFail()
    {
        var repairTasksUpdateCommand = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: Guid.Empty,
            RepairTaskIds: [Guid.NewGuid()]);

        var repairTasksUpdateCommandResult = _validator.Validate(repairTasksUpdateCommand);

        Assert.False(repairTasksUpdateCommandResult.IsValid);
        Assert.Contains(repairTasksUpdateCommandResult.Errors, e => e.PropertyName == "WorkOrderId");
    }

    [Fact]
    public void Validate_WithEmptyRepairTaskIds_ShouldFail()
    {
        var repairTasksUpdateCommand = new UpdateWorkOrderRepairTasksCommand(
            WorkOrderId: Guid.NewGuid(),
            RepairTaskIds: []);

        var repairTasksUpdateCommandResult = _validator.Validate(repairTasksUpdateCommand);

        Assert.False(repairTasksUpdateCommandResult.IsValid);
        Assert.Contains(repairTasksUpdateCommandResult.Errors, e => e.PropertyName == "RepairTaskIds");

}}