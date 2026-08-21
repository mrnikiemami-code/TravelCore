using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using TravelCore.Modules.Payment.Infrastructure.Options;

namespace TravelCore.Modules.Payment.Infrastructure.Providers;

/// <summary>
/// Official Stripe.net client wrapper. Types stay inside Infrastructure (TC-P35-T008).
/// </summary>
internal sealed class StripeNetCheckoutClient : IStripeCheckoutClient
{
    private readonly IOptions<PaymentStripeOptions> _options;

    public StripeNetCheckoutClient(IOptions<PaymentStripeOptions> options)
    {
        _options = options;
    }

    public async Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        StripeCheckoutSessionCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var service = new SessionService(CreateClient());
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            ClientReferenceId = request.ClientReferenceId,
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = request.CurrencyCode.ToLowerInvariant(),
                        UnitAmount = request.AmountMinorUnits,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "TravelCore booking payment",
                        },
                    },
                },
            ],
            Metadata = request.Metadata.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
        };

        var requestOptions = new RequestOptions
        {
            IdempotencyKey = request.IdempotencyKey,
        };

        var session = await service.CreateAsync(options, requestOptions, cancellationToken);
        return MapSession(session);
    }

    public async Task<StripeCheckoutSessionResult?> GetCheckoutSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        var service = new SessionService(CreateClient());
        var session = await service.GetAsync(sessionId, cancellationToken: cancellationToken);
        return session is null ? null : MapSession(session);
    }

    public async Task<StripeRefundResult> CreateRefundAsync(
        StripeRefundCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var service = new RefundService(CreateClient());
        var options = new RefundCreateOptions
        {
            PaymentIntent = request.PaymentIntentId,
            Amount = request.AmountMinorUnits,
        };
        var requestOptions = new RequestOptions
        {
            IdempotencyKey = request.IdempotencyKey,
        };
        var refund = await service.CreateAsync(options, requestOptions, cancellationToken);
        return new StripeRefundResult(
            refund.Id,
            refund.Status,
            refund.Amount,
            refund.Currency);
    }

    public StripeWebhookParseResult ParseWebhookEvent(string payload, string stripeSignatureHeader, string webhookSecret)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                stripeSignatureHeader,
                webhookSecret,
                throwOnApiVersionMismatch: false);

            string? sessionId = null;
            string? paymentIntentId = null;
            string? paymentStatus = null;
            long? amountTotal = null;
            string? currency = null;
            string? clientReferenceId = null;

            if (stripeEvent.Data.Object is Session session)
            {
                sessionId = session.Id;
                paymentIntentId = session.PaymentIntentId;
                paymentStatus = session.PaymentStatus;
                amountTotal = session.AmountTotal;
                currency = session.Currency;
                clientReferenceId = session.ClientReferenceId;
            }
            else if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
            {
                paymentIntentId = paymentIntent.Id;
                paymentStatus = paymentIntent.Status;
                amountTotal = paymentIntent.Amount;
                currency = paymentIntent.Currency;
            }

            return new StripeWebhookParseResult(
                IsValid: true,
                EventId: stripeEvent.Id,
                EventType: stripeEvent.Type,
                SessionId: sessionId,
                PaymentIntentId: paymentIntentId,
                PaymentStatus: paymentStatus,
                AmountTotal: amountTotal,
                CurrencyCode: currency,
                ClientReferenceId: clientReferenceId);
        }
        catch (StripeException)
        {
            return new StripeWebhookParseResult(
                IsValid: false,
                EventId: null,
                EventType: null,
                SessionId: null,
                PaymentIntentId: null,
                PaymentStatus: null,
                AmountTotal: null,
                CurrencyCode: null,
                ClientReferenceId: null);
        }
    }

    private StripeClient CreateClient()
    {
        var secret = _options.Value.SecretKey?.Trim()
            ?? throw new InvalidOperationException("Payment:Stripe:SecretKey is required.");
        return new StripeClient(secret);
    }

    private static StripeCheckoutSessionResult MapSession(Session session) =>
        new(
            session.Id,
            session.PaymentIntentId,
            session.Url,
            session.Status,
            session.PaymentStatus,
            session.AmountTotal,
            session.Currency);
}
