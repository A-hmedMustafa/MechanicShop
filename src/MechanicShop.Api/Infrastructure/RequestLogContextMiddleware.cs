using Serilog.Context;

namespace MechanicShop.Api.Infrastructure;

public class RequestLogContextMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;
    public Task InvokeAsync(HttpContext httpContext)
    {
        using(LogContext.PushProperty("CorrelationId", httpContext.TraceIdentifier))
        {
            return _next(httpContext);
        }
    }
    
}