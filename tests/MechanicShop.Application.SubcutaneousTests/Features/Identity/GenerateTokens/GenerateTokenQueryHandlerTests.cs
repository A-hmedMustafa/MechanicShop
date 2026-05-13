using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Infrastructure.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

public class GenerateTokenQueryHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ITokenProvider _tokenProvider = Substitute.For<ITokenProvider>();
    private readonly ILogger<GenerateTokenQueryHandler> _logger = Substitute.For<ILogger<GenerateTokenQueryHandler>>();

    private readonly GenerateTokenQueryHandler _sut;

    public GenerateTokenQueryHandlerTests()
    {
        _sut = new GenerateTokenQueryHandler(_logger, _identityService, _tokenProvider);
    }

    private static GenerateTokenQuery ValidQuery() => new(
        Email: "user@example.com",
        Password: "password123"
    );

    private static AppUserDto ValidAppUserDto() => new(
        UserId: Guid.NewGuid().ToString(),
        Email: "user@example.com",
        Roles: ["Labor"],
        Claims: []
    );
    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnToken()
    {
        var generateTokenQuery = ValidQuery();
        var appUserDto = ValidAppUserDto();

        var expectedToken = new TokenResponse{
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1)};

        _identityService
            .AuthenticateAsync(generateTokenQuery.Email, generateTokenQuery.Password)
            .Returns(appUserDto);

        _tokenProvider    
            .GenerateJwtTokenAsync(appUserDto, Arg.Any<CancellationToken>())
            .Returns(expectedToken);

     
        var generateTokenQueryResult = await _sut.Handle(generateTokenQuery, CancellationToken.None);

      
        Assert.True(generateTokenQueryResult.IsSuccess);
        Assert.Equal(expectedToken.AccessToken, generateTokenQueryResult.Value.AccessToken);
        Assert.Equal(expectedToken.RefreshToken, generateTokenQueryResult.Value.RefreshToken);
    }
    
    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldFail()
    {
        var generateTokenQuery = ValidQuery();
        
        _identityService.AuthenticateAsync(generateTokenQuery.Email, generateTokenQuery.Password)
            .Returns(Error.NotFound("User_Not_Found", "User Not Found"));

        var generateTokenQueryResult = await _sut.Handle(generateTokenQuery, CancellationToken.None);

        Assert.True(generateTokenQueryResult.IsError);
        Assert.Equal("User_Not_Found", generateTokenQueryResult.TopError.Code);

        await _tokenProvider.DidNotReceive().GenerateJwtTokenAsync(Arg.Any<AppUserDto>(), Arg.Any<CancellationToken>());

    }

    [Fact]
    public async Task Handle_WhenAuthenticationFails_ShouldNotCallTokenProvider()
    {
    
        var generateTokenQuery = ValidQuery();

        _identityService
            .AuthenticateAsync(generateTokenQuery.Email, generateTokenQuery.Password)
           .Returns(Error.Failure("Auth_Failed", "Authentication failed"));

    
        var generateTokenQueryResult = await _sut.Handle(generateTokenQuery, CancellationToken.None);

    
        Assert.True(generateTokenQueryResult.IsError);

        await _tokenProvider
            .DidNotReceive()
            .GenerateJwtTokenAsync(Arg.Any<AppUserDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTokenGenerationFails_ShouldReturnErrors()
    {
        
        var generateTokenQuery = ValidQuery();
        var appUserDto = ValidAppUserDto();

        _identityService
            .AuthenticateAsync(generateTokenQuery.Email, generateTokenQuery.Password)
            .Returns(appUserDto);

        _tokenProvider
            .GenerateJwtTokenAsync(appUserDto, Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Token_Generation_Failed", "Could not generate token."));

        
        var generateTokenQueryResult = await _sut.Handle(generateTokenQuery, CancellationToken.None);

       
        Assert.True(generateTokenQueryResult.IsError);
        Assert.Equal("Token_Generation_Failed", generateTokenQueryResult.TopError.Code);
    }
}