using Castle.Core.Logging;
using NSubstitute;
using Microsoft.Extensions.Logging;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Behaviors;
using Xunit;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Azure.Core;
namespace MechanicShop.Application.UnitTests.Behaviors;

public class PerformanceBehaviorTests
{
    private readonly ILogger<TestRequest> _logger = Substitute.For<ILogger<TestRequest>>();
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly PerformanceBehavior<TestRequest, TestResponse> _sut;
    public PerformanceBehaviorTests()
    {
        _sut = new PerformanceBehavior<TestRequest, TestResponse>(_logger, _user, _identityService);
    }

    [Fact]
    public async Task Handle_ShouldAlwaysReturnsResponseFromNext()
    {
        var request = new TestRequest { Name = "Test"};
        var expectedResult = new TestResponse {Result = "Success"};
        var cancellationToken = CancellationToken.None;

        var actualResult = await _sut.Handle(request, _ => Task.FromResult(expectedResult), cancellationToken);

        Assert.Equal(expectedResult, actualResult);
    }

    [Fact]
    public async Task Handle_WhenNextThrowsException_ShouldNotCatchException()
    {
        var request = new TestRequest { Name = "Test" };
        var expectedException = new InvalidOperationException("Test Exception");
        var cancellationToken = CancellationToken.None;

        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Handle(request, _ => throw expectedException, cancellationToken)
        );
        
        Assert.Equal(expectedException, actualException);
    }   
    [Fact]
    public async Task Handle_WhenRequestTakesLessThan500Ms_ShouldNotLogWarning()
    {
        var request = new TestRequest { Name = "Test"};
        var expectedResult = new TestResponse { Result = "Success"};
        var cancellationToken = CancellationToken.None;

        var actualResult = await _sut.Handle(request, _=> Task.FromResult(expectedResult), cancellationToken);

        Assert.Equal(expectedResult, actualResult);

        _logger.DidNotReceive().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>()       
        );
    

    }
  
    [Fact]
    public async Task Handle_WhenRequestTakesMoreThan500Ms_ShouldLogWarning()
    {
        var request = new TestRequest { Name = "Test"};
        var expectedResult = new TestResponse { Result = "Success"};
        var cancellationToken = CancellationToken.None;
        const string userId = "c2000";
        const string userName = "Ahmed";

        _user.Id.Returns(userId);
        _identityService.GetUserNameAsync(userId).Returns(userName);

        var actualResult = await _sut.Handle(request, async _ =>
        {
            await Task.Delay(600, cancellationToken);
            return expectedResult;
        }, cancellationToken);

        Assert.Equal(expectedResult, actualResult);

        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(message =>
                message.ToString()!.Contains("Long Running Request") &&
                message.ToString()!.Contains("TestRequest")          &&
                message.ToString()!.Contains(userId)                 &&
                message.ToString()!.Contains(userName)),
            null,
            Arg.Any<Func<object, Exception?, string>>()    
        );

    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldLogWarningWithEmptyUserInfo()
    {
        var request = new TestRequest { Name = "Test"};
        var expectedResult = new TestResponse { Result = "Success"};
        var cancellationToken = CancellationToken.None;
        _user.Id.Returns((string?)null);

        var actualResult = await _sut.Handle(request, async _ =>
        {
            await Task.Delay(700, cancellationToken);
            return expectedResult;
        }, cancellationToken);

        Assert.Equal(expectedResult, actualResult);

        await _identityService.DidNotReceive().GetUserNameAsync(Arg.Any<string>());

        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(message =>
                message.ToString()!.Contains("Long Running Request") &&
                message.ToString()!.Contains("TestRequest")),
            null,
            Arg.Any<Func<object, Exception?, string>>()    
        );

    }
       
    [Fact]
    public async Task Handle_WhenUserIdIsEmpty_ShouldLogWarningWithEmptyUserInfo()
    {
       
        var request = new TestRequest { Name = "Test" };
        var expectedResult = new TestResponse { Result = "Success" };
        var cancellationToken = CancellationToken.None;

        _user.Id.Returns(string.Empty);

        var actualResult = await _sut.Handle(request, async _ =>
        {
            await Task.Delay(600, cancellationToken);
            return expectedResult;
        }, cancellationToken);

       
        Assert.Equal(expectedResult, actualResult);
        await _identityService.DidNotReceive().GetUserNameAsync(Arg.Any<string>());

        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(message => 
                message.ToString()!.Contains("Long Running Request") &&
                message.ToString()!.Contains("TestRequest")),
            null,
            Arg.Any<Func<object, Exception?, string>>());

    }

    [Fact]
    public async Task Handle_WhenIdentityServiceReturnsNull_ShouldLogWarningWithNullUserName()
    {
        var request = new TestRequest { Name = "Test" };
        var expectedResult = new TestResponse { Result = "Success" };
        var cancellationToken = CancellationToken.None;
        const string userId = "user123";

        _user.Id.Returns(userId);
        _identityService.GetUserNameAsync(userId).Returns((string?)null);

      
        var actualResult = await _sut.Handle(request, async _ =>
        {
            await Task.Delay(600, cancellationToken); 
            return expectedResult;
        }, cancellationToken);

        
        Assert.Equal(expectedResult, actualResult);

        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(message =>
                message.ToString()!.Contains("Long Running Request") && 
                message.ToString()!.Contains("TestRequest")          &&
                message.ToString()!.Contains(userId)),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
   
    public class TestRequest
    {
        public string Name {get; set; } = string.Empty;
    }
    public class TestResponse
    {
        public string Result {get; set; } = string.Empty;
    }
}