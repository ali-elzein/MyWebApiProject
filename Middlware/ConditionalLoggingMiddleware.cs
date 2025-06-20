public class ConditionalLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ConditionalLoggingMiddleware> _logger;

    public ConditionalLoggingMiddleware(RequestDelegate next, ILogger<ConditionalLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var hasCustomHeader = context.Request.Headers.Keys
            .Any(k => k.Contains("x-skip-log", StringComparison.OrdinalIgnoreCase));

        if (hasCustomHeader)
        {
            _logger.LogWarning("Request contains 'x-skip-log' header — skipping detailed logging.");
        }
        else
        {
            // _logger.LogInformation($"Logging request: {context.Request.Method} {context.Request.Path}");
        }

        await _next(context);
    }
}