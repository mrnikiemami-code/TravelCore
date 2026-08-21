using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Infrastructure.Options;

namespace TravelCore.Modules.Payment.Infrastructure.Providers;

/// <summary>
/// Labeled non-production Payment provider. Never a production adapter (TC-P34-T003).
/// </summary>
internal sealed class SandboxPaymentProviderGateway : IPaymentProviderGateway
{
    public static readonly ProviderKey FixedKey = new(PaymentSandboxGate.ProviderKeyValue);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IOptions<PaymentSandboxOptions> _options;
    private readonly SandboxPaymentSessionStore _sessions;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public SandboxPaymentProviderGateway(
        IOptions<PaymentSandboxOptions> options,
        SandboxPaymentSessionStore sessions,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _options = options;
        _sessions = sessions;
        _httpContextAccessor = httpContextAccessor;
    }

    public ProviderKey Key => FixedKey;

    public PaymentProviderCapability Capabilities =>
        PaymentProviderCapability.RedirectInitiation
        | PaymentProviderCapability.CallbackVerification
        | PaymentProviderCapability.PaymentStatusQuery;

    public Task<PaymentInitiationResult> InitiatePaymentAsync(
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        var requestReference = new ProviderRequestReference($"sbx-req-{request.PaymentAttemptId:N}");
        var transactionReference = new ProviderTransactionReference($"sbx-txn-{request.PaymentAttemptId:N}");
        _sessions.TrackInitiated(new SandboxPaymentSession(
            requestReference,
            transactionReference,
            request.Amount,
            request.CurrencyCode,
            ProviderVerificationOutcome.PendingUnknown));

        return Task.FromResult(new PaymentInitiationResult
        {
            Outcome = PaymentInitiationOutcome.Initiated,
            ProviderKey = Key,
            RequestReference = requestReference,
            TransactionReference = transactionReference,
            RedirectUri = BuildOutcomeRedirectUri(
                requestReference,
                transactionReference,
                request.Amount,
                request.CurrencyCode),
        });
    }

    public Task<PaymentVerificationResult> VerifyPaymentAsync(
        PaymentVerificationRequest request,
        CancellationToken cancellationToken = default) =>
        QueryPaymentStatusAsync(request, cancellationToken);

    public Task<PaymentVerificationResult> QueryPaymentStatusAsync(
        PaymentVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        if (request.RequestReference is { } req && _sessions.TryGet(req, out var session))
        {
            return Task.FromResult(new PaymentVerificationResult
            {
                Outcome = session.Outcome,
                ProviderKey = Key,
                RequestReference = session.RequestReference,
                TransactionReference = session.TransactionReference ?? request.TransactionReference,
                ReportedAmount = session.Amount,
                ReportedCurrencyCode = session.CurrencyCode,
            });
        }

        return Task.FromResult(new PaymentVerificationResult
        {
            Outcome = ProviderVerificationOutcome.PendingUnknown,
            ProviderKey = Key,
            RequestReference = request.RequestReference,
            TransactionReference = request.TransactionReference,
        });
    }

    public Task<PaymentCallbackVerification> VerifyCallbackAsync(
        PaymentCallbackEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(envelope);

        var secret = _options.Value.HmacSecret;
        if (string.IsNullOrWhiteSpace(secret))
        {
            return Task.FromResult(PaymentCallbackVerification.Unverified());
        }

        if (!envelope.Headers.TryGetValue(PaymentSandboxGate.SignatureHeaderName, out var signature)
            || string.IsNullOrWhiteSpace(signature))
        {
            return Task.FromResult(PaymentCallbackVerification.Unverified());
        }

        var expected = ComputeHmacHex(secret, envelope.Body ?? string.Empty);
        if (!SignaturesMatch(expected, signature))
        {
            return Task.FromResult(PaymentCallbackVerification.Unverified());
        }

        SandboxCallbackPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<SandboxCallbackPayload>(envelope.Body ?? string.Empty, JsonOptions);
        }
        catch (JsonException)
        {
            return Task.FromResult(PaymentCallbackVerification.Unverified());
        }

        if (payload is null
            || string.IsNullOrWhiteSpace(payload.RequestReference)
            || string.IsNullOrWhiteSpace(payload.Outcome))
        {
            return Task.FromResult(PaymentCallbackVerification.Unverified());
        }

        if (!TryMapOutcome(payload.Outcome, out var outcome))
        {
            return Task.FromResult(PaymentCallbackVerification.Unverified());
        }

        var requestReference = new ProviderRequestReference(payload.RequestReference);
        ProviderTransactionReference? transactionReference = string.IsNullOrWhiteSpace(payload.TransactionReference)
            ? null
            : new ProviderTransactionReference(payload.TransactionReference);

        _sessions.RecordOutcome(requestReference, outcome, transactionReference);

        return Task.FromResult(PaymentCallbackVerification.Verified(new PaymentVerificationResult
        {
            Outcome = outcome,
            ProviderKey = Key,
            RequestReference = requestReference,
            TransactionReference = transactionReference,
            ReportedAmount = payload.Amount,
            ReportedCurrencyCode = payload.CurrencyCode,
        }));
    }

    public Task<PaymentInitiationResult> InitiateRefundAsync(
        RefundInitiationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new PaymentInitiationResult
        {
            Outcome = PaymentInitiationOutcome.DefinitiveFailure,
            ProviderKey = Key,
        });
    }

    public Task<PaymentVerificationResult> VerifyRefundAsync(
        PaymentVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new PaymentVerificationResult
        {
            Outcome = ProviderVerificationOutcome.Failed,
            ProviderKey = Key,
            RequestReference = request.RequestReference,
            TransactionReference = request.TransactionReference,
        });
    }

    public Task<PaymentVerificationResult> QueryRefundStatusAsync(
        PaymentVerificationRequest request,
        CancellationToken cancellationToken = default) =>
        VerifyRefundAsync(request, cancellationToken);

    public static string ComputeHmacHex(string secret, string body)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(body);
        var hash = HMACSHA256.HashData(key, data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    internal static bool SignaturesMatch(string expectedHex, string provided)
    {
        var normalized = provided.Trim();
        if (normalized.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["sha256=".Length..];
        }

        normalized = normalized.ToLowerInvariant();
        if (expectedHex.Length != normalized.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(expectedHex),
            Encoding.ASCII.GetBytes(normalized));
    }

    public static string CreateSignedCallbackBody(
        string outcome,
        ProviderRequestReference requestReference,
        ProviderTransactionReference? transactionReference,
        decimal? amount,
        string? currencyCode)
    {
        var payload = new SandboxCallbackPayload
        {
            Outcome = outcome,
            RequestReference = requestReference.Value,
            TransactionReference = transactionReference?.Value,
            Amount = amount,
            CurrencyCode = currencyCode,
        };
        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private Uri BuildOutcomeRedirectUri(
        ProviderRequestReference requestReference,
        ProviderTransactionReference transactionReference,
        decimal amount,
        string currencyCode)
    {
        var path =
            $"{PaymentSandboxOutcomeEndpoints.OutcomePath}"
            + $"?requestReference={Uri.EscapeDataString(requestReference.Value)}"
            + $"&transactionReference={Uri.EscapeDataString(transactionReference.Value)}"
            + $"&amount={Uri.EscapeDataString(amount.ToString(System.Globalization.CultureInfo.InvariantCulture))}"
            + $"&currencyCode={Uri.EscapeDataString(currencyCode)}";

        var configuredBase = _options.Value.PublicBaseUrl?.Trim().TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(configuredBase)
            && Uri.TryCreate(configuredBase + path, UriKind.Absolute, out var absoluteFromConfig))
        {
            return absoluteFromConfig;
        }

        var http = _httpContextAccessor?.HttpContext?.Request;
        if (http is not null)
        {
            var built = $"{http.Scheme}://{http.Host.Value}{path}";
            if (Uri.TryCreate(built, UriKind.Absolute, out var absoluteFromRequest))
            {
                return absoluteFromRequest;
            }
        }

        return new Uri(path, UriKind.Relative);
    }

    private static bool TryMapOutcome(string raw, out ProviderVerificationOutcome outcome)
    {
        if (string.Equals(raw, "Succeeded", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "Success", StringComparison.OrdinalIgnoreCase))
        {
            outcome = ProviderVerificationOutcome.Succeeded;
            return true;
        }

        if (string.Equals(raw, "Failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "Failure", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "Cancelled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "Canceled", StringComparison.OrdinalIgnoreCase))
        {
            // Cancelled uses existing Failed semantics — no new domain enum.
            outcome = ProviderVerificationOutcome.Failed;
            return true;
        }

        outcome = default;
        return false;
    }

    private sealed class SandboxCallbackPayload
    {
        public string? Outcome { get; init; }
        public string? RequestReference { get; init; }
        public string? TransactionReference { get; init; }
        public decimal? Amount { get; init; }
        public string? CurrencyCode { get; init; }
    }
}
