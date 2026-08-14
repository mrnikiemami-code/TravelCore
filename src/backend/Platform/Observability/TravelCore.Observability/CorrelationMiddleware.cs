using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace TravelCore.Observability;

/// <summary>
/// Selects a safe application correlation ID, returns it on the response, and opens a logging scope.
/// Does not mutate <see cref="Activity"/> or <see cref="HttpContext.TraceIdentifier"/>.
/// </summary>
public sealed class CorrelationMiddleware
{
    private const string HttpContextItemKey = "__TravelCore.CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationMiddleware> _logger;

    public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = ResolveCorrelationId(context);
        context.Items[HttpContextItemKey] = correlationId;

        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            if (!headers.ContainsKey(TravelCoreCorrelationHeaders.CorrelationId))
            {
                headers[TravelCoreCorrelationHeaders.CorrelationId] = correlationId;
            }

            return Task.CompletedTask;
        });

        var activity = Activity.Current;
        if (activity is not null)
        {
            using (_logger.BeginScope(
                      "CorrelationId {CorrelationId} TraceId {TraceId}",
                      correlationId,
                      activity.TraceId.ToString()))
            {
                await _next(context);
            }
        }
        else
        {
            using (_logger.BeginScope("CorrelationId {CorrelationId}", correlationId))
            {
                await _next(context);
            }
        }
    }

    /// <summary>
    /// Reads the application correlation ID selected for this request, if middleware has run.
    /// </summary>
    public static string? GetCorrelationId(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Items.TryGetValue(HttpContextItemKey, out var value)
            ? value as string
            : null;
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(TravelCoreCorrelationHeaders.CorrelationId, out StringValues values)
            && CorrelationIdValidator.TryGetValid(values, out var incoming))
        {
            return incoming;
        }

        var activity = Activity.Current;
        if (activity is not null)
        {
            return activity.TraceId.ToString();
        }

        return context.TraceIdentifier;
    }
}
