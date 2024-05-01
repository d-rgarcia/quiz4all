using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptionsMonitor<CorrelationIdOptions> _correlationIdOptions;

    public CorrelationIdMiddleware(RequestDelegate next, IOptionsMonitor<CorrelationIdOptions> correlationIdOptions)
    {
        _next = next;
        _correlationIdOptions = correlationIdOptions;
    }

    public Task InvokeAsync(HttpContext httpContext)
    {
        if (_correlationIdOptions.CurrentValue.Enabled)
        {
            string correlationId = GetCorrelationId(httpContext);

            using (var logContext = LogContext.PushProperty(_correlationIdOptions.CurrentValue.PropertyName, correlationId))
            {
                return _next.Invoke(httpContext);
            }
        }
        return _next.Invoke(httpContext);
    }

    private string GetCorrelationId(HttpContext httpContext)
    {
        httpContext.Request.Headers.TryGetValue(_correlationIdOptions.CurrentValue.Header, out StringValues correlationIdHeader);

        return correlationIdHeader.FirstOrDefault() ?? httpContext.TraceIdentifier;
    }
}