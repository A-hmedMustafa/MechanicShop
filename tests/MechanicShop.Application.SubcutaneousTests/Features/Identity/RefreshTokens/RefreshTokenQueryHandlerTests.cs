using System.Linq.Expressions;
using System.Security.Claims;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Queries.RefreshTokens;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Xunit;


[Collection(IdentityTestCollection.CollectionName)]
public class RefreshTokenQueryHandlerTests : SubcutaneousTestBase
{
    private readonly IIdentityService _identityService;
    private readonly ITokenProvider _tokenProvider;
    private readonly IdentityTestWebAppFactory _factory;

    public RefreshTokenQueryHandlerTests(IdentityTestWebAppFactory factory) : base(factory)
    {
        _factory = factory;
        _identityService = factory.FakeIdentityService;
        _tokenProvider = factory.FakeTokenProvider;

        _identityService.ClearSubstitute();
        _tokenProvider.ClearSubstitute();
    }
    

    private static RefreshTokenQuery ValidQuery() => new(
        RefreshToken: "valid-refresh-token",
        ExpiredAccessToken: "expired-access-token"
    );

    private static AppUserDto ValidAppUserDto() => new(
        UserId: Guid.NewGuid().ToString(),
        Email: "user@example.com",
        Roles: ["Labor"],
        Claims: []
    );

    private static TokenResponse ValidTokenResponse() => new()
    {
        AccessToken = "new-access-token",
        RefreshToken = "new-refresh-token",
        ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
    };

    private static ClaimsPrincipal ValidPrincipal(string userId) =>
        new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ]));

    private async Task SeedRefreshTokenAsync(string token, string userId)
    {
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), token, userId,  DateTime.UtcNow.AddHours(1)).Value;

        await  _context.RefreshTokens.AddAsync(refreshToken, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);
        TrackEntity(refreshToken);
    } 
    [Fact]
    public async Task Handle_WithValidData_ShouldReturnNewToken()
    {
        var query = ValidQuery();
        var userId = Guid.NewGuid().ToString();
        var appUserDto = ValidAppUserDto();
        var expectedToken = ValidTokenResponse();

        await SeedRefreshTokenAsync(query.RefreshToken, userId);

        _tokenProvider
            .GetPrincipalFromExpiredToken(query.ExpiredAccessToken)
            .Returns(ValidPrincipal(userId));

        _identityService
            .GetUserByIdAsync(userId)
            .Returns(appUserDto);
       
        _tokenProvider
            .GenerateJwtTokenAsync(appUserDto, Arg.Any<CancellationToken>())
            .Returns(expectedToken);

    
       

        var result = await _mediator.Send(query, CancellationToken.None);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedToken.AccessToken, result.Value.AccessToken);
    }

    [Fact]
    public async Task Handle_WhenExpiredTokenInvalid_ShouldFail()
    {
       
        var query = ValidQuery();

        _tokenProvider
            .GetPrincipalFromExpiredToken(query.ExpiredAccessToken)
            .Returns((ClaimsPrincipal?)null);

        
        var result = await _mediator.Send(query, CancellationToken.None);

        
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.ExpiredAccessTokenInvalid.Code, result.TopError.Code);

        await _identityService
            .DidNotReceive()
            .GetUserByIdAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenUserIdClaimMissing_ShouldFail()
    {
        
        var query = ValidQuery();

       
        var principalWithoutClaim = new ClaimsPrincipal(new ClaimsIdentity([]));

        _tokenProvider
            .GetPrincipalFromExpiredToken(query.ExpiredAccessToken)
            .Returns(principalWithoutClaim);

        var result = await _mediator.Send(query, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.UserIdClaimInvalid.Code, result.TopError.Code);

        await _identityService
            .DidNotReceive()
            .GetUserByIdAsync(Arg.Any<string>());
    }
    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldFail()
    {
        var query = ValidQuery();
        var userId = Guid.NewGuid().ToString();

        _tokenProvider
            .GetPrincipalFromExpiredToken(query.ExpiredAccessToken)
            .Returns(ValidPrincipal(userId));

        _identityService
            .GetUserByIdAsync(userId)
            .Returns(Error.NotFound("User_Not_Found", "User not found"));

      
        var result = await _mediator.Send(query, CancellationToken.None);

       
        Assert.True(result.IsError);
        Assert.Equal("User_Not_Found", result.TopError.Code);

        await _tokenProvider
            .DidNotReceive()
            .GenerateJwtTokenAsync(Arg.Any<AppUserDto>());
    }

    [Fact]
    public async Task Handle_WhenTokenGenerationFails_ShouldFail()
    {
      
        var query = ValidQuery();
        var userId = Guid.NewGuid().ToString();
        var appUserDto = ValidAppUserDto();

        await SeedRefreshTokenAsync(query.ExpiredAccessToken, userId);

        _tokenProvider
            .GetPrincipalFromExpiredToken(query.ExpiredAccessToken)
            .Returns(ValidPrincipal(userId));

        _identityService
            .GetUserByIdAsync(userId)
            .Returns(appUserDto);

        

        _tokenProvider
            .GenerateJwtTokenAsync(appUserDto)
            .Returns(Error.Failure("Token_Generation_Failed", "Could not generate token"));

        var result = await _mediator.Send(query, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Token_Generation_Failed", result.TopError.Code);
    }
    
    
}
