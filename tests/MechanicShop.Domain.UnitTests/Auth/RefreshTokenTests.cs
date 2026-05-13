using MechanicShop.Tests.Common.Auth;
using Xunit;

namespace MechanicShop.Domain.UnitTests.Auth;

public class RefreshTokenTests
{
    [Fact]
    public void CreateRefreshToken_ShouldSucceed_WithValidData()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var expiresAtUtc = DateTimeOffset.UtcNow.AddDays(7);
        var token = "token";

        var tokenCreationResult = RefreshTokenFactory.CreateRefreshToken(
            id,token,userId,expiresAtUtc);

        var newToken = tokenCreationResult.Value;

        Assert.True(tokenCreationResult.IsSuccess);
        Assert.NotNull(newToken);
        Assert.True(newToken.ExpiresAtUtc > DateTimeOffset.UtcNow);
        Assert.False(string.IsNullOrWhiteSpace(newToken.UserId));
        Assert.Equal(userId, newToken.UserId);
        Assert.Equal(token, newToken.Token);

    }
   
    [Fact]
    public void CreateRefreshToken_ShouldFail_WhenIdEmpty()
    {
        var tokenCreationResult = RefreshTokenFactory.CreateRefreshToken(id: Guid.Empty);

        Assert.True(tokenCreationResult.IsError);
        Assert.Equal("RefreshToken_Id_Required", tokenCreationResult.TopError.Code);
    }

    [Fact]
    public void CreateRefreshToken_ShouldFail_WhenExpiresAtUtcIsInPast()
    {
        var tokenCreationResult = RefreshTokenFactory.CreateRefreshToken(expiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(-1));

        Assert.True(tokenCreationResult.IsError);
        Assert.Equal("RefreshToken_Expiry_Invalid", tokenCreationResult.TopError.Code);

    }
    
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateRefreshToken_ShouldFail_WhenTokenInvalid(string? invalidToken)
    {
       var tokenCreationResult = RefreshTokenFactory.CreateRefreshToken(token: invalidToken);

       Assert.True(tokenCreationResult.IsError);
       Assert.Equal("RefreshToken_Token_Required", tokenCreationResult.TopError.Code);
    }


    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateRefreshToken_ShouldFail_WhenUserIdInvalid(string? userId)
    {
        var tokenCreationResult = RefreshTokenFactory.CreateRefreshToken(userId: userId);
    
        Assert.True(tokenCreationResult.IsError);
        Assert.Equal("RefreshToken_UserId_Required", tokenCreationResult.TopError.Code);
    }

}