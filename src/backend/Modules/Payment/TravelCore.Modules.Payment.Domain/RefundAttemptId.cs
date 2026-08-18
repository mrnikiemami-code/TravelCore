using TravelCore.Identifiers;

namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Strongly typed RefundAttempt identity (UUID v7). Not a RefundId or provider reference (P20-R6).
/// </summary>
public readonly record struct RefundAttemptId(Guid Value)
{
    public static RefundAttemptId New() => new(Uuid7.New());

    public static RefundAttemptId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("RefundAttemptId cannot be empty.", nameof(value));
        }

        return new RefundAttemptId(value);
    }

    public override string ToString() => Value.ToString("D");
}
