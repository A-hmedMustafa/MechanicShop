using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.GenerateTokens;

public class GenerateTokenQueryValidatorTests
{
    private readonly GenerateTokenQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var generateTokenQuery = new GenerateTokenQuery(Email: "user@example.com", Password: "pass123");

        var generateTokenQueryResult = _validator.Validate(generateTokenQuery);

        Assert.True(generateTokenQueryResult.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldFail()
    {
        var generateTokenQuery = new GenerateTokenQuery(Email: "", Password: "pass123");

        var generateTokenQueryResult = _validator.Validate(generateTokenQuery);

        Assert.False(generateTokenQueryResult.IsValid);
        Assert.Contains(generateTokenQueryResult.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WithEmptyPassword_ShouldFail()
    {
        var generateTokenQuery = new GenerateTokenQuery(Email: "user@example.com", Password: "");

        var generateTokenQueryResult = _validator.Validate(generateTokenQuery);

        Assert.False(generateTokenQueryResult.IsValid);
        Assert.Contains(generateTokenQueryResult.Errors, e => e.PropertyName == "Password");
    }
}