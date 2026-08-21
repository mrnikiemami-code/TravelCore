using Microsoft.Extensions.Options;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Infrastructure.Options;
using TravelCore.Modules.Payment.Infrastructure.Providers;
using Xunit;

namespace TravelCore.Modules.Payment.UnitTests;

public sealed class PaymentStripeProviderTests
{
    private static readonly ProviderKey StripeKey = new(PaymentStripeGate.ProviderKeyValue);

    [Fact]
    public void Production_Cannot_Allow_Stripe_Test_Mode()
    {
        Assert.False(PaymentStripeGate.IsAllowed("Production", enabled: true, secretKey: "sk_test_x"));
        Assert.False(PaymentStripeGate.IsAllowed("production", enabled: true, secretKey: "sk_test_x"));
    }

    [Fact]
    public void Live_Secret_Is_Rejected_Even_In_Development()
    {
        Assert.False(PaymentStripeGate.IsAllowed("Development", enabled: true, secretKey: "sk_live_x"));
    }

    [Theory]
    [InlineData("Development", true, "sk_test_abc", true)]
    [InlineData("Local", true, "sk_test_abc", true)]
    [InlineData("Staging", true, "sk_test_abc", true)]
    [InlineData("Development", false, "sk_test_abc", false)]
    [InlineData("Development", true, null, false)]
    [InlineData("Production", true, "sk_test_abc", false)]
    [InlineData(null, true, "sk_test_abc", false)]
    public void Stripe_Gate_Is_Fail_Closed(string? environment, bool enabled, string? secret, bool expected)
    {
        Assert.Equal(expected, PaymentStripeGate.IsAllowed(environment, enabled, secret));
    }

    [Fact]
    public void NamedProductionAdapterImplemented_Stays_False_With_Stripe_Test_Mode()
    {
        Assert.False(PaymentProviderTrustBoundary.NamedProductionAdapterImplemented);
        Assert.Equal(
            "stripe test mode != production activation",
            PaymentProviderTrustBoundary.StripeTestModeIsNotProductionActivation);
    }

    [Theory]
    [InlineData(12.90, "USD", 1290L)]
    [InlineData(100, "AED", 10000L)]
    [InlineData(0.01, "USD", 1L)]
    public void Amount_To_Minor_Units_AED_USD(decimal amount, string currency, long expected)
    {
        Assert.True(StripePaymentProviderGateway.TryToMinorUnits(amount, currency, out var minor));
        Assert.Equal(expected, minor);
    }

    [Theory]
    [InlineData(12.901, "USD")]
    [InlineData(10, "IRR")]
    [InlineData(-1, "USD")]
    public void Unsupported_Or_Invalid_Amount_Fails(decimal amount, string currency)
    {
        Assert.False(StripePaymentProviderGateway.TryToMinorUnits(amount, currency, out _));
    }

    [Fact]
    public async Task Initiate_Creates_Checkout_Redirect_For_AED()
    {
        var client = new FakeStripeClient();
        var gateway = CreateGateway(client);
        var result = await gateway.InitiatePaymentAsync(NewRequest(50m, "AED"));

        Assert.Equal(PaymentInitiationOutcome.Initiated, result.Outcome);
        Assert.Equal(StripeKey, result.ProviderKey);
        Assert.NotNull(result.RedirectUri);
        Assert.Equal("cs_test_1", result.RequestReference!.Value.Value);
        Assert.Equal(5000L, client.LastCreate!.AmountMinorUnits);
        Assert.Equal("AED", client.LastCreate.CurrencyCode);
    }

    [Fact]
    public async Task Unsupported_Currency_Fails_Closed()
    {
        var gateway = CreateGateway(new FakeStripeClient());
        var result = await gateway.InitiatePaymentAsync(NewRequest(10m, "IRR"));
        Assert.Equal(PaymentInitiationOutcome.DefinitiveFailure, result.Outcome);
    }

    [Fact]
    public async Task Valid_Webhook_Signature_Maps_Success()
    {
        var client = new FakeStripeClient
        {
            NextParse = new StripeWebhookParseResult(
                true,
                "evt_1",
                "checkout.session.completed",
                "cs_test_1",
                "pi_1",
                "paid",
                5000,
                "aed",
                "attempt"),
        };
        var gateway = CreateGateway(client);
        var verified = await gateway.VerifyCallbackAsync(new PaymentCallbackEnvelope
        {
            ProviderKey = StripeKey,
            Body = "{}",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [PaymentStripeGate.SignatureHeaderName] = "t=1,v1=abc",
            },
        });

        Assert.True(verified.IsVerified);
        Assert.Equal(ProviderVerificationOutcome.Succeeded, verified.Result!.Outcome);
        Assert.Equal(50m, verified.Result.ReportedAmount);
        Assert.Equal("AED", verified.Result.ReportedCurrencyCode);
    }

    [Fact]
    public async Task Invalid_Webhook_Is_Unverified()
    {
        var client = new FakeStripeClient
        {
            NextParse = new StripeWebhookParseResult(false, null, null, null, null, null, null, null, null),
        };
        var gateway = CreateGateway(client);
        var verified = await gateway.VerifyCallbackAsync(new PaymentCallbackEnvelope
        {
            ProviderKey = StripeKey,
            Body = "{}",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [PaymentStripeGate.SignatureHeaderName] = "bad",
            },
        });

        Assert.False(verified.IsVerified);
    }

    [Fact]
    public async Task Missing_Signature_Is_Unverified()
    {
        var gateway = CreateGateway(new FakeStripeClient());
        var verified = await gateway.VerifyCallbackAsync(new PaymentCallbackEnvelope
        {
            ProviderKey = StripeKey,
            Body = "{}",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
        });
        Assert.False(verified.IsVerified);
    }

    [Fact]
    public async Task Duplicate_Event_Parse_Remains_Idempotent_At_Gateway()
    {
        var client = new FakeStripeClient
        {
            NextParse = new StripeWebhookParseResult(
                true,
                "evt_dup",
                "checkout.session.completed",
                "cs_test_1",
                "pi_1",
                "paid",
                100,
                "usd",
                null),
        };
        var gateway = CreateGateway(client);
        var envelope = new PaymentCallbackEnvelope
        {
            ProviderKey = StripeKey,
            Body = "{}",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [PaymentStripeGate.SignatureHeaderName] = "t=1,v1=abc",
            },
        };

        var first = await gateway.VerifyCallbackAsync(envelope);
        var second = await gateway.VerifyCallbackAsync(envelope);
        Assert.True(first.IsVerified);
        Assert.True(second.IsVerified);
        Assert.Equal(first.Result!.Outcome, second.Result!.Outcome);
    }

    [Fact]
    public void Resolver_Exposes_Stripe_Test_For_Public_Initiation_Without_Production_Flag()
    {
        Assert.False(PaymentProviderTrustBoundary.NamedProductionAdapterImplemented);
        var gateway = CreateGateway(new FakeStripeClient());
        var resolver = new PaymentProviderResolver([gateway]);
        var descriptor = resolver.Describe(StripeKey);
        Assert.NotNull(descriptor);
        Assert.True(descriptor!.AvailableForPublicInitiation);
        Assert.Equal(PaymentStripeGate.DisplayName, descriptor.DisplayName);
    }

    private static StripePaymentProviderGateway CreateGateway(IStripeCheckoutClient client)
    {
        var options = Options.Create(new PaymentStripeOptions
        {
            Enabled = true,
            SecretKey = "sk_test_unit",
            WebhookSecret = "whsec_unit",
            PublicBaseUrl = "https://api.test.local",
            SuccessUrl = "https://app.test.local/return?ok=1",
            CancelUrl = "https://app.test.local/return?ok=0",
        });
        return new StripePaymentProviderGateway(options, client);
    }

    private static PaymentInitiationRequest NewRequest(decimal amount, string currency) =>
        new(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            StripeKey,
            amount,
            currency);

    private sealed class FakeStripeClient : IStripeCheckoutClient
    {
        public StripeCheckoutSessionCreateRequest? LastCreate { get; private set; }
        public StripeWebhookParseResult NextParse { get; set; } =
            new(false, null, null, null, null, null, null, null, null);

        public Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(
            StripeCheckoutSessionCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            LastCreate = request;
            return Task.FromResult(new StripeCheckoutSessionResult(
                "cs_test_1",
                "pi_test_1",
                "https://checkout.stripe.com/c/pay/cs_test_1",
                "open",
                "unpaid",
                request.AmountMinorUnits,
                request.CurrencyCode.ToLowerInvariant()));
        }

        public Task<StripeCheckoutSessionResult?> GetCheckoutSessionAsync(
            string sessionId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<StripeCheckoutSessionResult?>(new StripeCheckoutSessionResult(
                sessionId,
                "pi_test_1",
                "https://checkout.stripe.com/c/pay/" + sessionId,
                "complete",
                "paid",
                5000,
                "aed"));

        public Task<StripeRefundResult> CreateRefundAsync(
            StripeRefundCreateRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new StripeRefundResult("re_1", "succeeded", request.AmountMinorUnits, "aed"));

        public StripeWebhookParseResult ParseWebhookEvent(string payload, string stripeSignatureHeader, string webhookSecret) =>
            NextParse;
    }
}
