using TravelCore.Identifiers;

namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Strongly typed Refund identity (UUID v7). Not a PaymentId and not a provider reference (P20-R6).
/// </summary>
public readonly record struct RefundId(Guid Value)
{
    public static RefundId New() => new(Uuid7.New());

    public static RefundId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("RefundId cannot be empty.", nameof(value));
        }

        return new RefundId(value);
    }

    public override string ToString() => Value.ToString("D");
}
