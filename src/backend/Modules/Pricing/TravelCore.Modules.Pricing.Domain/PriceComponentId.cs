using TravelCore.Identifiers;

namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Strongly typed identity for <see cref="PriceComponent"/> (UUID v7).
/// </summary>
public readonly record struct PriceComponentId(Guid Value) : IEquatable<PriceComponentId>
{
    public static PriceComponentId New() => new(Uuid7.New());

    public static PriceComponentId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PriceComponentId cannot be empty.", nameof(value));
        }

        return new PriceComponentId(value);
    }

    public override string ToString() => Value.ToString("D");
}
