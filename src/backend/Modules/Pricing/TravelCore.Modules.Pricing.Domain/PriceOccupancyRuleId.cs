using TravelCore.Identifiers;

namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Strongly typed identity for <see cref="PriceOccupancyRule"/> (UUID v7).
/// </summary>
public readonly record struct PriceOccupancyRuleId(Guid Value) : IEquatable<PriceOccupancyRuleId>
{
    public static PriceOccupancyRuleId New() => new(Uuid7.New());

    public static PriceOccupancyRuleId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PriceOccupancyRuleId cannot be empty.", nameof(value));
        }

        return new PriceOccupancyRuleId(value);
    }

    public override string ToString() => Value.ToString("D");
}
