using System.Diagnostics;

namespace RoomSplit.API.Middlewares;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = GetClientIp(context);

        _logger.LogInformation(
            "HTTP {Method} {Path} from {IpAddress}",
            context.Request.Method,
            context.Request.Path,
            ipAddress);

        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        _logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {Duration}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }

    private static string GetClientIp(HttpContext context)
    {
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        }
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = context.Connection.RemoteIpAddress?.ToString();
        }
        return ipAddress ?? "unknown";
    }
}
