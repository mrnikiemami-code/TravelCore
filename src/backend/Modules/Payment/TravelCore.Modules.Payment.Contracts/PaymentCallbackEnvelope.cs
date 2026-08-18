namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Transient inbound callback payload for adapter verification. Not persisted (P20-R3).
/// </summary>
public sealed class PaymentCallbackEnvelope
{
    public required ProviderKey ProviderKey { get; init; }

    public IReadOnlyDictionary<string, string> Headers { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> Query { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string Body { get; init; } = string.Empty;
}
