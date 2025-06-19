using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class PoweredByMiddleware
{
    private readonly RequestDelegate _next;

    public PoweredByMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Powered-By"] = "ASP.NET Core Ali El-Zein";
            return Task.CompletedTask;
        });

        await _next(context);
    }
}