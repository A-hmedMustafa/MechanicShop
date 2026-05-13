using Castle.Core.Logging;
using MechanicShop.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
namespace MechanicShop.Application.UnitTests.Behaviors;

public class UnhandledExceptionBehaviorTests
{

    private readonly ILogger<DummyRequest> _logger = Substitute.For<ILogger<DummyRequest>>();
    private readonly UnhandledExceptionBehavior<DummyRequest, string> _sut;
    
    public UnhandledExceptionBehaviorTests()
    {
        _sut = new UnhandledExceptionBehavior<DummyRequest, string>(_logger);
    }

    [Fact]
    public async Task Handle_WhenNoException_InvokesNextAndReturnsResult()
    {
        var request = new DummyRequest();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        var cancellationToken = CancellationToken.None;
        next.Invoke(cancellationToken).Returns("OK");

        var actualResult = await _sut.Handle(request, next, cancellationToken);
    
        Assert.Equal("OK", actualResult);
    }

    [Fact]
    public async Task Handle_WhenExceptionThrown_LogsErrorAndRethrow()
    {
        var request = new DummyRequest();
        var expectedException = new InvalidOperationException("test failure");
        var cancellationToken = CancellationToken.None;
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke(cancellationToken).Returns<Task<string>>(_ => throw expectedException);

        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _sut.Handle(request, next, cancellationToken));

        Assert.Equal(expectedException, actualException);

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(message => message.ToString()!.Contains("Unhandled Exception")),
            expectedException,
            Arg.Any<Func<object, Exception?, string>>()
        );

    }
    public class DummyRequest;
}