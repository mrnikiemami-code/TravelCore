namespace TravelCore.Modules.Payment.Infrastructure.Options;

/// <summary>
/// Non-production sandbox Payment provider settings (TC-P34-T003).
/// Secrets belong in local/secure config — never commit real values.
/// </summary>
public sealed class PaymentSandboxOptions
{
    public const string SectionName = "Payment:Sandbox";

    /// <summary>
    /// Explicit opt-in. Registration still requires a non-production environment.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Shared secret for HMAC callback verification. Placeholder only in Development samples.
    /// </summary>
    public string HmacSecret { get; set; } = string.Empty;

    /// <summary>
    /// Optional absolute public API base (e.g. https://localhost:7xxx) used to build sandbox redirect URIs.
    /// When empty, Initiate returns a root-relative URI.
    /// </summary>
    public string? PublicBaseUrl { get; set; }
}
