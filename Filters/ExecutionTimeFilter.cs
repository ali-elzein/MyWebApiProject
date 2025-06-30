using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

public class ExecutionTimeFilter : Attribute, IActionFilter
{
    private readonly ILogger<ExecutionTimeFilter> _logger;
    private Stopwatch? _stopwatch;

    public ExecutionTimeFilter(ILogger<ExecutionTimeFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _stopwatch = Stopwatch.StartNew();
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _stopwatch?.Stop();
        var elapsedMs = _stopwatch?.ElapsedMilliseconds ?? 0;

        if (context.Result is ObjectResult originalResult)
        {
            context.Result = new OkObjectResult(new
            {
                Data = originalResult.Value,
                ExecutionTimeMs = elapsedMs
            });
        }
    }
}