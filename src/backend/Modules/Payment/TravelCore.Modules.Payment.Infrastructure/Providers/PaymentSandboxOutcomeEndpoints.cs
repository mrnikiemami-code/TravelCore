using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Infrastructure.Endpoints;
using TravelCore.Modules.Payment.Infrastructure.Options;

namespace TravelCore.Modules.Payment.Infrastructure.Providers;

/// <summary>
/// Minimal NON-PRODUCTION sandbox outcome UI + signed callback poster (TC-P34-T003).
/// Browser GET alone never marks Payment success.
/// </summary>
internal static class PaymentSandboxOutcomeEndpoints
{
    public const string OutcomePath = "/api/payment/providers/sandbox/outcome";

    public static IEndpointRouteBuilder MapSandboxPaymentOutcomeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup(OutcomePath)
            .WithTags("PaymentSandbox")
            .AllowAnonymous();

        group.MapGet("/", RenderOutcomePageAsync);
        group.MapPost("/", SubmitOutcomeAsync);
        return endpoints;
    }

    private static IResult RenderOutcomePageAsync(
        string? requestReference,
        string? transactionReference,
        string? amount,
        string? currencyCode,
        IPaymentProviderResolver resolver)
    {
        if (resolver.Resolve(SandboxPaymentProviderGateway.FixedKey) is null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(requestReference))
        {
            return Results.BadRequest("requestReference is required.");
        }

        var safeRef = System.Net.WebUtility.HtmlEncode(requestReference);
        var safeTxn = System.Net.WebUtility.HtmlEncode(transactionReference ?? string.Empty);
        var safeAmount = System.Net.WebUtility.HtmlEncode(amount ?? "—");
        var safeCurrency = System.Net.WebUtility.HtmlEncode(currencyCode ?? string.Empty);
        var safeAmountField = System.Net.WebUtility.HtmlEncode(amount ?? string.Empty);
        var browserRule = PaymentProviderTrustBoundary.BrowserReturnIsNotPaymentSuccess;
        var unverifiedRule = PaymentProviderTrustBoundary.UnverifiedCallbackIsNotPaymentSuccess;

        var html = $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>NON-PRODUCTION · Sandbox Payment</title>
              <style>
                body { font-family: system-ui, sans-serif; max-width: 36rem; margin: 2rem auto; padding: 0 1rem; background: #f6f3ee; color: #1a1a1a; }
                .banner { background: #7a1f1f; color: #fff; padding: 0.75rem 1rem; border-radius: 0.35rem; font-weight: 700; letter-spacing: 0.02em; }
                h1 { font-size: 1.25rem; margin-top: 1.25rem; }
                .meta { color: #444; font-size: 0.95rem; line-height: 1.45; }
                form { display: grid; gap: 0.65rem; margin-top: 1.5rem; }
                button { padding: 0.7rem 1rem; font-size: 1rem; cursor: pointer; border-radius: 0.35rem; border: 1px solid #333; }
                button[value="Succeeded"] { background: #1f6b3a; color: #fff; border-color: #1f6b3a; }
                button[value="Failed"] { background: #fff; }
                button[value="Cancelled"] { background: #eee; }
                .note { margin-top: 1.5rem; font-size: 0.85rem; color: #555; }
              </style>
            </head>
            <body>
              <div class="banner">NON-PRODUCTION / SANDBOX — not a real payment provider</div>
              <h1>Sandbox payment outcome</h1>
              <p class="meta">
                Choose an outcome. This page only posts a <strong>verified provider callback</strong>.
                Opening or returning here does <strong>not</strong> mark payment success by itself.
              </p>
              <p class="meta">Request: <code>{{safeRef}}</code><br/>
              Amount (display): <code>{{safeAmount}} {{safeCurrency}}</code></p>
              <form method="post" action="{{OutcomePath}}">
                <input type="hidden" name="requestReference" value="{{safeRef}}" />
                <input type="hidden" name="transactionReference" value="{{safeTxn}}" />
                <input type="hidden" name="amount" value="{{safeAmountField}}" />
                <input type="hidden" name="currencyCode" value="{{safeCurrency}}" />
                <button type="submit" name="outcome" value="Succeeded">Simulate Success</button>
                <button type="submit" name="outcome" value="Failed">Simulate Failure</button>
                <button type="submit" name="outcome" value="Cancelled">Simulate Cancelled</button>
              </form>
              <p class="note">{{browserRule}} · {{unverifiedRule}}</p>
            </body>
            </html>
            """;

        return Results.Content(html, "text/html; charset=utf-8");
    }

    private static async Task<IResult> SubmitOutcomeAsync(
        HttpContext httpContext,
        IPaymentProviderResolver resolver,
        IOptions<PaymentSandboxOptions> options,
        SandboxPaymentSessionStore sessions,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
    {
        if (resolver.Resolve(SandboxPaymentProviderGateway.FixedKey) is null)
        {
            return Results.NotFound();
        }

        var form = await httpContext.Request.ReadFormAsync(cancellationToken);
        var outcome = form["outcome"].ToString();
        var requestReferenceRaw = form["requestReference"].ToString();
        var transactionReferenceRaw = form["transactionReference"].ToString();
        var amountRaw = form["amount"].ToString();
        var currencyCode = form["currencyCode"].ToString();

        if (string.IsNullOrWhiteSpace(outcome) || string.IsNullOrWhiteSpace(requestReferenceRaw))
        {
            return Results.BadRequest("outcome and requestReference are required.");
        }

        ProviderRequestReference requestReference;
        try
        {
            requestReference = new ProviderRequestReference(requestReferenceRaw);
        }
        catch (ArgumentException)
        {
            return Results.BadRequest("requestReference is invalid.");
        }

        decimal? amount = null;
        if (decimal.TryParse(
                amountRaw,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsedAmount))
        {
            amount = parsedAmount;
        }

        ProviderTransactionReference? transactionReference = null;
        if (!string.IsNullOrWhiteSpace(transactionReferenceRaw))
        {
            try
            {
                transactionReference = new ProviderTransactionReference(transactionReferenceRaw);
            }
            catch (ArgumentException)
            {
                return Results.BadRequest("transactionReference is invalid.");
            }
        }

        if (sessions.TryGet(requestReference, out var session))
        {
            transactionReference ??= session.TransactionReference;
            amount ??= session.Amount;
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                currencyCode = session.CurrencyCode ?? string.Empty;
            }
        }

        var secret = options.Value.HmacSecret;
        if (string.IsNullOrWhiteSpace(secret))
        {
            return Results.Content(
                RenderResultHtml(
                    "Sandbox misconfigured",
                    "Payment:Sandbox:HmacSecret is missing. Callback was not posted. Browser return alone did not mark success."),
                "text/html; charset=utf-8");
        }

        var body = SandboxPaymentProviderGateway.CreateSignedCallbackBody(
            outcome,
            requestReference,
            transactionReference,
            amount,
            string.IsNullOrWhiteSpace(currencyCode) ? null : currencyCode);
        var signature = SandboxPaymentProviderGateway.ComputeHmacHex(secret, body);

        var callbackPath = $"{PaymentProviderCallbackEndpoints.CallbackGroup}/sandbox/callback";
        var request = httpContext.Request;
        var callbackUri = $"{request.Scheme}://{request.Host.Value}{callbackPath}";

        using var client = httpClientFactory.CreateClient("TravelCore.PaymentSandbox");
        using var message = new HttpRequestMessage(HttpMethod.Post, callbackUri)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        message.Headers.TryAddWithoutValidation(PaymentSandboxGate.SignatureHeaderName, signature);

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            return Results.Content(
                RenderResultHtml(
                    "Sandbox callback transport failed",
                    $"Could not POST verified callback: {System.Net.WebUtility.HtmlEncode(ex.Message)}. Browser return alone did not mark success."),
                "text/html; charset=utf-8");
        }

        using (response)
        {
            var status = (int)response.StatusCode;
            var note = status is >= 200 and < 300
                ? "Verified sandbox callback posted. Payment success (if any) comes only from Payment verification — not from this browser page."
                : $"Callback POST returned HTTP {status}. Payment was not marked successful by browser return alone.";

            return Results.Content(
                RenderResultHtml(
                    $"Sandbox outcome submitted · {System.Net.WebUtility.HtmlEncode(outcome)}",
                    note),
                "text/html; charset=utf-8");
        }
    }

    private static string RenderResultHtml(string title, string body)
    {
        var browserRule = PaymentProviderTrustBoundary.BrowserReturnIsNotPaymentSuccess;
        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <title>NON-PRODUCTION · Sandbox</title>
              <style>
                body { font-family: system-ui, sans-serif; max-width: 36rem; margin: 2rem auto; padding: 0 1rem; background: #f6f3ee; }
                .banner { background: #7a1f1f; color: #fff; padding: 0.75rem 1rem; border-radius: 0.35rem; font-weight: 700; }
              </style>
            </head>
            <body>
              <div class="banner">NON-PRODUCTION / SANDBOX</div>
              <h1>{{title}}</h1>
              <p>{{body}}</p>
              <p><small>{{browserRule}}</small></p>
            </body>
            </html>
            """;
    }
}
