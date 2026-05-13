using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Dashboard.Queries;

public class GetWorkOrderStatsQueryValidatorTests
{
    private readonly GetWorkOrderStatsQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidDate_ShouldSucceed()
    {
        var query = new GetWorkOrderStatsQuery(DateOnly.FromDateTime(DateTime.UtcNow));

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithDefaultDate_ShouldFail()
    {
        var query = new GetWorkOrderStatsQuery(default);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Date");
    }
}