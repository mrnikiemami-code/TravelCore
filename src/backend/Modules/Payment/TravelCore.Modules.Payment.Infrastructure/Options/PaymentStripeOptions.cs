namespace TravelCore.Modules.Payment.Infrastructure.Options;

/// <summary>
/// Stripe TEST-MODE options only (TC-P35-T008). Production activation remains blocked.
/// </summary>
public sealed class PaymentStripeOptions
{
    public const string SectionName = "Payment:Stripe";

    /// <summary>Must be true to register the Stripe adapter (still fail-closed in Production).</summary>
    public bool Enabled { get; set; }

    /// <summary>Stripe secret key. Test mode requires <c>sk_test_</c> prefix.</summary>
    public string? SecretKey { get; set; }

    /// <summary>Webhook signing secret (<c>whsec_...</c>). Required for callback verification.</summary>
    public string? WebhookSecret { get; set; }

    /// <summary>Optional absolute public API base for success/cancel URL composition.</summary>
    public string? PublicBaseUrl { get; set; }

    /// <summary>Browser return success URL template. Not authoritative payment success.</summary>
    public string? SuccessUrl { get; set; }

    /// <summary>Browser return cancel URL template. Not authoritative payment failure alone.</summary>
    public string? CancelUrl { get; set; }
}
