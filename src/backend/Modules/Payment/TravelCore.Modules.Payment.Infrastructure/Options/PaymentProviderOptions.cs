namespace TravelCore.Modules.Payment.Infrastructure.Options;

/// <summary>
/// Server-controlled Payment provider selection. Secrets stay in secure configuration, not source control (P20-R3).
/// </summary>
public sealed class PaymentProviderOptions
{
    public const string SectionName = "Payment:Provider";

    /// <summary>
    /// Trusted default ProviderKey. Optional so the host can start without a production adapter.
    /// </summary>
    public string? DefaultProviderKey { get; set; }
}
