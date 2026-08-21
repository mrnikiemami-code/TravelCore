using Microsoft.Extensions.Options;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Infrastructure.Options;
using TravelCore.Modules.Payment.Infrastructure.Providers;
using TravelCore.Modules.Payment.Infrastructure.Services;
using Xunit;

namespace TravelCore.Modules.Payment.UnitTests;

public sealed class PaymentSandboxProviderTests
{
    private static readonly ProviderKey SandboxKey = new(PaymentSandboxGate.ProviderKeyValue);

    [Fact]
    public void Production_Cannot_Allow_Sandbox_Even_When_Enabled()
    {
        Assert.False(PaymentSandboxGate.IsAllowed("Production", enabled: true));
        Assert.False(PaymentSandboxGate.IsAllowed("production", enabled: true));
    }

    [Theory]
    [InlineData("Development", true, true)]
    [InlineData("Local", true, true)]
    [InlineData("Staging", true, true)]
    [InlineData("Development", false, false)]
    [InlineData("Production", true, false)]
    [InlineData(null, true, false)]
    public void Sandbox_Gate_Is_Fail_Closed(string? environment, bool enabled, bool expected)
    {
        Assert.Equal(expected, PaymentSandboxGate.IsAllowed(environment, enabled));
    }

    [Fact]
    public void NamedProductionAdapterImplemented_Stays_False_With_Sandbox()
    {
        Assert.False(PaymentProviderTrustBoundary.NamedProductionAdapterImplemented);
        Assert.Equal(
            "sandbox != production provider",
            PaymentProviderTrustBoundary.SandboxIsNotProductionProvider);
        Assert.NotEqual("sandbox", PaymentProviderTrustBoundary.NamedProviderSelected);
    }

    [Fact]
    public void Sandbox_Is_Not_Production_Provider_Key()
    {
        Assert.Equal("sandbox", PaymentSandboxGate.ProviderKeyValue);
        Assert.Equal("Sandbox (non-production)", PaymentSandboxGate.DisplayName);
        Assert.True(PaymentSandboxGate.IsSandboxProviderKey("sandbox"));
        Assert.False(PaymentSandboxGate.IsSandboxProviderKey("stripe"));
        Assert.False(PaymentProviderTrustBoundary.NamedProductionAdapterImplemented);
    }

    [Fact]
    public async Task Sandbox_Initiate_Returns_Labeled_Outcome_Redirect()
    {
        var gateway = CreateGateway();
        var result = await gateway.InitiatePaymentAsync(
            new PaymentInitiationRequest(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                SandboxKey,
                1290m,
                "USD"));

        Assert.Equal(PaymentInitiationOutcome.Initiated, result.Outcome);
        Assert.Equal(SandboxKey, result.ProviderKey);
        Assert.NotNull(result.RedirectUri);
        Assert.Contains(
            PaymentSandboxOutcomeEndpoints.OutcomePath,
            result.RedirectUri!.ToString(),
            StringComparison.Ordinal);
        Assert.True(result.RequestReference is not null);
        Assert.True(result.TransactionReference is not null);
    }

    [Fact]
    public async Task Sandbox_Verified_Success_Callback_Requires_Valid_Hmac()
    {
        var gateway = CreateGateway(secret: "unit-test-secret");
        var initiation = await gateway.InitiatePaymentAsync(
            new PaymentInitiationRequest(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                SandboxKey,
                50m,
                "USD"));

        var body = SandboxPaymentProviderGateway.CreateSignedCallbackBody(
            "Succeeded",
            initiation.RequestReference!.Value,
            initiation.TransactionReference,
            50m,
            "USD");
        var signature = SandboxPaymentProviderGateway.ComputeHmacHex("unit-test-secret", body);

        var verified = await gateway.VerifyCallbackAsync(new PaymentCallbackEnvelope
        {
            ProviderKey = SandboxKey,
            Body = body,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [PaymentSandboxGate.SignatureHeaderName] = signature,
            },
        });

        Assert.True(verified.IsVerified);
        Assert.Equal(ProviderVerificationOutcome.Succeeded, verified.Result!.Outcome);
        Assert.Equal(50m, verified.Result.ReportedAmount);
        Assert.Equal("USD", verified.Result.ReportedCurrencyCode);
    }

    [Fact]
    public async Task Tampered_Or_Unsigned_Callback_Is_Unverified()
    {
        var gateway = CreateGateway(secret: "unit-test-secret");
        var body = """{"outcome":"Succeeded","requestReference":"sbx-req-x","amount":1,"currencyCode":"USD"}""";

        var unsigned = await gateway.VerifyCallbackAsync(new PaymentCallbackEnvelope
        {
            ProviderKey = SandboxKey,
            Body = body,
        });
        Assert.False(unsigned.IsVerified);

        var tampered = await gateway.VerifyCallbackAsync(new PaymentCallbackEnvelope
        {
            ProviderKey = SandboxKey,
            Body = body,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [PaymentSandboxGate.SignatureHeaderName] = "deadbeef",
            },
        });
        Assert.False(tampered.IsVerified);
    }

    [Fact]
    public async Task Refunds_Fail_Closed()
    {
        var gateway = CreateGateway();
        var initiation = await gateway.InitiateRefundAsync(
            new RefundInitiationRequest(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                SandboxKey,
                null,
                10m,
                "USD"));
        Assert.Equal(PaymentInitiationOutcome.DefinitiveFailure, initiation.Outcome);

        var verify = await gateway.VerifyRefundAsync(new PaymentVerificationRequest(SandboxKey, null, null));
        Assert.Equal(ProviderVerificationOutcome.Failed, verify.Outcome);
    }

    [Fact]
    public void Public_Initiation_Allows_Sandbox_Without_Production_Flag()
    {
        Assert.False(PaymentProviderTrustBoundary.NamedProductionAdapterImplemented);
        var gateway = CreateGateway();
        var resolver = new PaymentProviderResolver(
            [gateway],
            Options.Create(new PaymentProviderOptions { DefaultProviderKey = "sandbox" }));
        Assert.True(
            PublicPaymentInitiationEligibility.IsAvailable(
                Options.Create(new PaymentProviderOptions { DefaultProviderKey = "sandbox" }),
                resolver));

        var descriptor = resolver.Describe(SandboxKey);
        Assert.NotNull(descriptor);
        Assert.Equal(PaymentSandboxGate.DisplayName, descriptor!.DisplayName);
        Assert.True(descriptor.AvailableForPublicInitiation);
        Assert.False(descriptor.Capabilities.HasFlag(PaymentProviderCapability.RefundInitiation));
    }

    [Fact]
    public void Public_Initiation_Remains_Unavailable_Without_Sandbox_Or_Production()
    {
        var resolver = new PaymentProviderResolver([]);
        Assert.False(
            PublicPaymentInitiationEligibility.IsAvailable(
                Options.Create(new PaymentProviderOptions { DefaultProviderKey = "sandbox" }),
                resolver));
        Assert.False(
            PublicPaymentInitiationEligibility.IsAvailable(
                Options.Create(new PaymentProviderOptions { DefaultProviderKey = "stripe" }),
                resolver));
    }

    [Fact]
    public async Task Cancelled_Outcome_Maps_To_Failed_Without_New_Domain_Enum()
    {
        var gateway = CreateGateway(secret: "unit-test-secret");
        var initiation = await gateway.InitiatePaymentAsync(
            new PaymentInitiationRequest(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                SandboxKey,
                10m,
                "USD"));
        var body = SandboxPaymentProviderGateway.CreateSignedCallbackBody(
            "Cancelled",
            initiation.RequestReference!.Value,
            initiation.TransactionReference,
            10m,
            "USD");
        var signature = SandboxPaymentProviderGateway.ComputeHmacHex("unit-test-secret", body);
        var verified = await gateway.VerifyCallbackAsync(new PaymentCallbackEnvelope
        {
            ProviderKey = SandboxKey,
            Body = body,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [PaymentSandboxGate.SignatureHeaderName] = signature,
            },
        });
        Assert.True(verified.IsVerified);
        Assert.Equal(ProviderVerificationOutcome.Failed, verified.Result!.Outcome);
    }

    private static SandboxPaymentProviderGateway CreateGateway(string secret = "DEV-ONLY-CHANGE-ME-sandbox-hmac-secret")
    {
        return new SandboxPaymentProviderGateway(
            Options.Create(new PaymentSandboxOptions
            {
                Enabled = true,
                HmacSecret = secret,
                PublicBaseUrl = "https://localhost:5001",
            }),
            new SandboxPaymentSessionStore());
    }
}
