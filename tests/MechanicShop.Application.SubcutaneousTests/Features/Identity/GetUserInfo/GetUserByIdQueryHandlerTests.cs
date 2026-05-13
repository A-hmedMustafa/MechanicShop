using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Queries.GetUserInfo;
using MechanicShop.Domain.Common.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.GetUserInfo;
public class GetUserByIdQueryHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ILogger<GetUserByIdQueryHandler> _logger = Substitute.For<ILogger<GetUserByIdQueryHandler>>();

    private readonly GetUserByIdQueryHandler _sut;

    public GetUserByIdQueryHandlerTests()
    {
        _sut = new GetUserByIdQueryHandler(_logger, _identityService);
    }

    private static AppUserDto ValidAppUserDto() => new(
        UserId: Guid.NewGuid().ToString(),
        Email: "user@example.com",
        Roles: ["Labor"],
        Claims: []
    );

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnUser()
    {
       
        var getUserByIdQuery = new GetUserByIdQuery(Guid.NewGuid().ToString());
        var appUserDto = ValidAppUserDto();

        _identityService
            .GetUserByIdAsync(getUserByIdQuery.UserId!)
            .Returns(appUserDto);

        var getUserByIdQueryResult = await _sut.Handle(getUserByIdQuery, CancellationToken.None);

      
        Assert.True(getUserByIdQueryResult.IsSuccess);
        Assert.Equal(appUserDto.UserId, getUserByIdQueryResult.Value.UserId);
        Assert.Equal(appUserDto.Email, getUserByIdQueryResult.Value.Email);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnErrors()
    {
       
        var getUserByIdQuery = new GetUserByIdQuery(Guid.NewGuid().ToString());

        _identityService
            .GetUserByIdAsync(getUserByIdQuery.UserId!)
            .Returns(Error.NotFound("User_Not_Found", "User not found"));

    
        var getUserByIdQueryResult = await _sut.Handle(getUserByIdQuery, CancellationToken.None);

        
        Assert.True(getUserByIdQueryResult.IsError);
        Assert.Equal("User_Not_Found", getUserByIdQueryResult.TopError.Code);
    }
}