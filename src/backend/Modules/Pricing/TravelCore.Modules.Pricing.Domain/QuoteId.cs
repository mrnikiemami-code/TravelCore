using TravelCore.Identifiers;

namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Strongly typed Quote identity (UUID v7).
/// </summary>
public readonly record struct QuoteId(Guid Value) : IEquatable<QuoteId>
{
    public static QuoteId New() => new(Uuid7.New());

    public static QuoteId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("QuoteId cannot be empty.", nameof(value));
        }

        return new QuoteId(value);
    }

    public override string ToString() => Value.ToString("D");
}
