namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// External provider request/correlation identifier. Not a PaymentId or PaymentAttemptId (P20-R3).
/// </summary>
public readonly record struct ProviderRequestReference
{
    public const int MaxLength = 128;

    public string Value { get; }

    public ProviderRequestReference(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("ProviderRequestReference cannot be empty.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException(
                $"ProviderRequestReference cannot exceed {MaxLength} characters.",
                nameof(value));
        }

        Value = trimmed;
    }

    public override string ToString() => Value;
}
