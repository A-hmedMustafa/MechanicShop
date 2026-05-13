using System.Linq.Expressions;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviors;

public class UnhandledExceptionBehavior<TRequest, TResponse>(ILogger<TRequest> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch(Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(ex, "Request: Unhandled Exception For Request {Name} {@Request}", requestName, request);
            
            throw;
        }
    }
}