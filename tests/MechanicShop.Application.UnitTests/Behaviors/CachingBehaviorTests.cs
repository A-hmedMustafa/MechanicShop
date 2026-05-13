using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Xunit;
using Azure.Core;
namespace MechanicShop.Application.UnitTests.Behaviors;

public class CachingBehaviorTests
{
    private readonly HybridCache _cache = Substitute.For<HybridCache>();

    private readonly ILogger<CachingBehavior<CachedQuery, Result<string>>> _logger = Substitute.For<ILogger<CachingBehavior<CachedQuery, Result<string>>>>();

    private readonly CachingBehavior<CachedQuery, Result<string>> _sut;

    public CachingBehaviorTests()
    {
        _sut = new CachingBehavior<CachedQuery, Result<string>>(_cache, _logger);
    }


    [Fact]
    public async Task Handle_WhenNotCachedQuery_ShouldSkipCachingAndReturnResult()
    {
        var uncachedRequest = new NonCachedQuery();
        var behavior = new CachingBehavior<NonCachedQuery, string>(
            _cache, Substitute.For<ILogger<CachingBehavior<NonCachedQuery, string>>>());    
    
        var result = await behavior.Handle(uncachedRequest, _=> Task.FromResult("OK"), CancellationToken.None);

        Assert.Equal("OK", result);

        await _cache.DidNotReceive().SetAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>());


    }

    [Fact]
    public async Task Handle_WhenCachedQueryAndResultIsSuccess_ShouldCacheResult()
    {
        var cachedRequest = new CachedQuery();
        var response = (Result<string>)"test-value";

        string? actualKey = null;
        object? actualValue = null;
        HybridCacheEntryOptions? actualOptions = null;
        string[]? actualTags = null;
        CancellationToken actualToken = default;

        _cache.SetAsync(
            Arg.Do<string>(passedKey => actualKey = passedKey),
            Arg.Do<object>(passedValue => actualValue = passedValue),
            Arg.Do<HybridCacheEntryOptions>(passedOptions => actualOptions = passedOptions),
            Arg.Do<string[]>(passedTags => actualTags = passedTags),
            Arg.Do<CancellationToken>(passedCancellationToken => actualToken = passedCancellationToken)
        ).Returns(ValueTask.CompletedTask);

        var result = await _sut.Handle(cachedRequest, _=> Task.FromResult(response), CancellationToken.None);
    
        Assert.True(result.IsSuccess);

        var resultType = Assert.IsType<Result<string>>(actualValue);

        Assert.True(resultType.IsSuccess);
        Assert.Equal("test-value", resultType.Value);
        Assert.Equal(cachedRequest.CacheKey, actualKey);
        Assert.Equal(cachedRequest.Tags, actualTags);
        Assert.Equal(cachedRequest.Expiration, actualOptions!.Expiration);
    
    }
    
    [Fact]
    public async Task Handle_WhenCachedQueryAndResultIsError_ShouldNotCacheResult()
    {
        var cachedRequest = new CachedQuery();
        var errorResponse = (Result<string>)Error.Validation("code", "message");

        var result = await _sut.Handle(cachedRequest, _=> Task.FromResult(errorResponse), CancellationToken.None);

        Assert.True(result.IsError);

        var allCacheCalls = _cache.ReceivedCalls();
        var setAsyncOnlyCalls = allCacheCalls.Where(
            call => call.GetMethodInfo().Name == nameof(HybridCache.SetAsync) &&
            call.GetMethodInfo().IsGenericMethod &&
            call.GetMethodInfo().GetGenericArguments().FirstOrDefault() == typeof(Result<string>)
        );

        Assert.Empty(setAsyncOnlyCalls);
    }
    
    public class NonCachedQuery;
    public class CachedQuery : ICachedQuery
    {
        public string CacheKey => "test-key";

        public string[] Tags => ["unit-test"];

        public TimeSpan Expiration => TimeSpan.FromMinutes(5);
    }
}