using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries;

public class GetRepairTaskByIdQueryValidatorTests
{
    private readonly GetRepairTaskByIdQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidRepairTaskId_ShouldSucceed()
    {
        var getRepairTaskByIdQuery = new GetRepairTaskByIdQuery(Guid.NewGuid());

        var getRepairTaskByIdQueryResult = _validator.Validate(getRepairTaskByIdQuery);

        Assert.True(getRepairTaskByIdQueryResult.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyRepairTaskId_ShouldFail()
    {
        var getRepairTaskByIdQuery = new GetRepairTaskByIdQuery(Guid.Empty);

        var getRepairTaskByIdQueryResult = _validator.Validate(getRepairTaskByIdQuery);

        Assert.False(getRepairTaskByIdQueryResult.IsValid);
        Assert.Contains(getRepairTaskByIdQueryResult.Errors, e => e.PropertyName == "RepairTaskId");
    }
}