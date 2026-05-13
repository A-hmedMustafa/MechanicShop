using System.Security.Cryptography.X509Certificates;
using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class LoggingBehaviorTests
{
    private readonly ILogger<DummyRequest> _logger = Substitute.For<ILogger<DummyRequest>>();
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly LoggingBehavior<DummyRequest> _sut;

    public LoggingBehaviorTests()
    {
        _sut = new LoggingBehavior<DummyRequest>(_logger, _user, _identityService);
    }

    [Fact]
    public async Task Process_WithUserId_LogsRequestWithUserName()
    {
        var request = new DummyRequest();
        _user.Id.Returns("c2000");
        _identityService.GetUserNameAsync("c2000").Returns("Ahmed");

        await _sut.Process(request, CancellationToken.None);
        await _identityService.Received(1).GetUserNameAsync("c2000");

        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(message => message.ToString()!.Contains("Request")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    [Fact]
    public async Task Process_WithUserIdEmpty_LogRequestWithEmptyUserName()
    {
        var request = new DummyRequest();
        _user.Id.Returns((string?)null);

        await _sut.Process(request, CancellationToken.None);

        await _identityService.DidNotReceive().GetUserNameAsync(Arg.Any<string>());

        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(message => message.ToString()!.Contains("Request")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>()
        );
        
    }

    public class DummyRequest;
}