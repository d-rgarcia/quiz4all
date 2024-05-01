public static class CorrelationIdMiddlewareExtension
{
    public static IApplicationBuilder UseCorrelationIdMiddleware(this IApplicationBuilder builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}