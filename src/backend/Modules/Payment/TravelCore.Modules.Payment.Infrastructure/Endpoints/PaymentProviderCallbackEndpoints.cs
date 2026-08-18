using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Infrastructure.Services;

namespace TravelCore.Modules.Payment.Infrastructure.Endpoints;

/// <summary>
/// Provider-integration callback only. Not public Payment UX and not a success authority (P20-R3).
/// </summary>
internal static class PaymentProviderCallbackEndpoints
{
    public const string CallbackGroup = "/api/payment/providers";

    public static IEndpointRouteBuilder MapPaymentProviderCallbackEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup(CallbackGroup)
            .WithTags("PaymentProviderCallback")
            .AllowAnonymous();

        group.MapPost("/{providerKey}/callback", ProcessCallbackAsync);
        return endpoints;
    }

    private static async Task<IResult> ProcessCallbackAsync(
        string providerKey,
        HttpContext httpContext,
        PaymentCallbackProcessor processor,
        CancellationToken cancellationToken)
    {
        if (!ProviderKey.TryParse(providerKey, out var key))
        {
            return Results.NotFound();
        }

        using var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var body = await reader.ReadToEndAsync(cancellationToken);
        var headers = httpContext.Request.Headers.ToDictionary(
            header => header.Key,
            header => header.Value.ToString(),
            StringComparer.OrdinalIgnoreCase);
        var query = httpContext.Request.Query.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ToString(),
            StringComparer.OrdinalIgnoreCase);

        var result = await processor.ProcessAsync(
            new PaymentCallbackEnvelope
            {
                ProviderKey = key,
                Headers = headers,
                Query = query,
                Body = body,
            },
            cancellationToken);

        return result.Status switch
        {
            PaymentCallbackProcessStatus.Applied => Results.NoContent(),
            PaymentCallbackProcessStatus.Ignored => Results.NoContent(),
            PaymentCallbackProcessStatus.Unverified => Results.Unauthorized(),
            PaymentCallbackProcessStatus.UnknownProvider => Results.NotFound(),
            PaymentCallbackProcessStatus.UnknownAttempt => Results.NotFound(),
            _ => Results.BadRequest(),
        };
    }
}
