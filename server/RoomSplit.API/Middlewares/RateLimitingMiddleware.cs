using System.Collections.Concurrent;
using RoomSplit.API.DTOs;

namespace RoomSplit.API.Middlewares;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ConcurrentDictionary<string, Queue<DateTime>> _ipRequestLog = new();
    private const int MaxRequests = 5;
    private static readonly TimeSpan TimeWindow = TimeSpan.FromMinutes(1);

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsAuthEndpoint(context.Request.Path))
        {
            var ipAddress = GetClientIp(context);
            var requestQueue = _ipRequestLog.GetOrAdd(ipAddress, _ => new Queue<DateTime>());

            bool rateLimitExceeded = false;
            lock (requestQueue)
            {
                // Remove old requests outside time window
                while (requestQueue.Count > 0 &&
                       DateTime.UtcNow - requestQueue.Peek() > TimeWindow)
                {
                    requestQueue.Dequeue();
                }

                if (requestQueue.Count >= MaxRequests)
                {
                    rateLimitExceeded = true;
                }
                else
                {
                    requestQueue.Enqueue(DateTime.UtcNow);
                }
            }

            if (rateLimitExceeded)
            {
                _logger.LogWarning("Rate limit exceeded for IP: {IpAddress}", ipAddress);
                context.Response.StatusCode = 429;
                context.Response.Headers["Retry-After"] = "60";
                await context.Response.WriteAsJsonAsync(
                    ApiResponse.Fail("Too many requests. Please try again later."));
                return;
            }
        }

        await _next(context);
    }

    private static bool IsAuthEndpoint(PathString path)
    {
        return path.StartsWithSegments("/api/auth");
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
