using TravelCore.Identifiers;

namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Strongly typed PaymentAttempt identity (UUID v7). Not a provider transaction reference (P20-R2).
/// </summary>
public readonly record struct PaymentAttemptId(Guid Value)
{
    public static PaymentAttemptId New() => new(Uuid7.New());

    public static PaymentAttemptId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PaymentAttemptId cannot be empty.", nameof(value));
        }

        return new PaymentAttemptId(value);
    }

    public override string ToString() => Value.ToString("D");
}
