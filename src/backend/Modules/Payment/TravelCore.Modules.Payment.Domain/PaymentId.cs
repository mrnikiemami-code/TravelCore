using TravelCore.Identifiers;

namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Strongly typed Payment identity (UUID v7). Not a provider transaction reference (P20-R2).
/// </summary>
public readonly record struct PaymentId(Guid Value)
{
    public static PaymentId New() => new(Uuid7.New());

    public static PaymentId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(value));
        }

        return new PaymentId(value);
    }

    public override string ToString() => Value.ToString("D");
}
