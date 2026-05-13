using Xunit;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
namespace MechanicShop.Domain.UnitTests.WorkOrders;

public class WorkOrderTests
{
    [Fact]
    public void CreateWorkOrder_WithIdEmpty_ShouldFail()
    {
        var workOrderCreationResult = WorkOrder.Create(
            id: Guid.Empty,
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]
        );

        Assert.False(workOrderCreationResult.IsSuccess);

        Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Code, workOrderCreationResult.TopError.Code);
        
    }

    [Fact]
    public void CreateWorkOrder_WhenVehicleIdIsEmpty_ShouldFail()
    {
        var workOrderCreationResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.Empty,
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.False(workOrderCreationResult.IsSuccess);

        Assert.Equal(WorkOrderErrors.VehicleIdRequired.Code, workOrderCreationResult.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_WhenNoRepairTasks_ShouldFail()
    {
        var workOrderCreationResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: []);

        Assert.False(workOrderCreationResult.IsSuccess);

        Assert.Equal(WorkOrderErrors.RepairTasksRequired.Code, workOrderCreationResult.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_WhenLaborIdEmpty_ShouldFail()
    {
        var workOrderCreationResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.Empty,
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.False(workOrderCreationResult.IsSuccess);

        Assert.Equal(WorkOrderErrors.LaborIdRequired.Code, workOrderCreationResult.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_WhenTimingInvalid_ShouldFail()
    {
        var workOrderCreationResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow.AddHours(1),
            endAt: DateTimeOffset.UtcNow,
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.False(workOrderCreationResult.IsSuccess);

        Assert.Equal(WorkOrderErrors.InvalidTiming.Code, workOrderCreationResult.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_WhenSpotInvalid_ShouldFail()
    {
        const Spot invalidSpot = (Spot)999;

        var workOrderCreationResult = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: invalidSpot,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.False(workOrderCreationResult.IsSuccess);

        Assert.Equal(WorkOrderErrors.SpotInvalid.Code, workOrderCreationResult.TopError.Code);
    }

    [Fact]
    public void AddRepairTask_WhenNotEditable_ShouldFail()
    {
        var workOrder = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        workOrder.UpdateState(WorkOrderState.InProgress);
        workOrder.UpdateState(WorkOrderState.Completed);

        var result = workOrder.AddRepairTask(RepairTaskFactory.CreateRepairTask().Value);

        Assert.False(result.IsSuccess);
        Assert.True(result.Errors.Count > 0);
    }
    
    [Fact]
    public void UpdateLabor_WhenLaborIdEmpty_ShouldFail()
    {
        var workOrder = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = workOrder.UpdateLabor(Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.LaborIdEmpty(workOrder.Id.ToString()).Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateSpot_WhenSpotInvalid_ShouldFail()
    {
        var workOrder = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        const Spot invalidSpot = (Spot)999;
        var result = workOrder.UpdateSpot(invalidSpot);

        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.SpotInvalid.Code, result.TopError.Code);
    }


    [Fact]
    public void UpdateTiming_WhenTimeInvalid_ShouldFail()
    {
        var workOrder = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = workOrder.UpdateTiming(DateTimeOffset.UtcNow.AddHours(2), DateTimeOffset.UtcNow);

        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.InvalidTiming.Code, result.TopError.Code);
    }

    
    [Fact]
    public void UpdateState_WhenTransitionInvalid_ShouldFail()
    {
        var workOrder = WorkOrder.Create(
                      id: Guid.NewGuid(),
                      vehicleId: Guid.NewGuid(),
                      startAt: DateTimeOffset.UtcNow,
                      endAt: DateTimeOffset.UtcNow.AddHours(1),
                      laborId: Guid.NewGuid(),
                      spot: Spot.A,
                      repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = workOrder.UpdateState(WorkOrderState.Completed);

        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.InvalidStateTransition(WorkOrderState.Scheduled, WorkOrderState.Completed).Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateLabor_SetNewValidLaborId_ShouldSucceed()
    {
        var workOrder = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var newLabor = Guid.NewGuid();
        var result = workOrder.UpdateLabor(newLabor);

        Assert.True(result.IsSuccess);
        Assert.Equal(newLabor, workOrder.LaborId);
    }

    [Fact]
    public void UpdateSpot_SetNewValidSpot_ShouldSucceed()
    {
        var workOrder = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = workOrder.UpdateSpot(Spot.B);

        Assert.True(result.IsSuccess);
        Assert.Equal(Spot.B, workOrder.Spot);
    }

    [Fact]
    public void UpdateTiming_SetNewValidTiming_ShouldSucceed()
    {
        var workOrder = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var newStart = workOrder.StartsAtUtc.AddHours(2);
        var newEnd = newStart.AddHours(1);
        var result = workOrder.UpdateTiming(newStart, newEnd);

        Assert.True(result.IsSuccess);
        Assert.Equal(newStart, workOrder.StartsAtUtc);
        Assert.Equal(newEnd, workOrder.EndsAtUtc);
    }

    [Fact]
    public void UpdateState_SetStateToInProgress_ShouldSucceed()
    {
        var workOrder = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = workOrder.UpdateState(WorkOrderState.InProgress);

        Assert.True(result.IsSuccess);
        Assert.Equal(WorkOrderState.InProgress, workOrder.State);
    }
}