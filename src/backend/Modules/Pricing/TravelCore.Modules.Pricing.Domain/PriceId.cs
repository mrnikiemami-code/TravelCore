using TravelCore.Identifiers;

namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Strongly typed Price identity (UUID v7).
/// </summary>
public readonly record struct PriceId(Guid Value) : IEquatable<PriceId>
{
    public static PriceId New() => new(Uuid7.New());

    public static PriceId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PriceId cannot be empty.", nameof(value));
        }

        return new PriceId(value);
    }

    public override string ToString() => Value.ToString("D");
}
