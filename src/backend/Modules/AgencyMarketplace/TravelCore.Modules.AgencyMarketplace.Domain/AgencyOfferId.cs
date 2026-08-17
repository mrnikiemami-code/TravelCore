using TravelCore.Identifiers;

namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Strongly typed AgencyOffer identity (UUID v7).
/// </summary>
public readonly record struct AgencyOfferId(Guid Value) : IEquatable<AgencyOfferId>
{
    public static AgencyOfferId New() => new(Uuid7.New());

    public static AgencyOfferId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("AgencyOfferId cannot be empty.", nameof(value));
        }

        return new AgencyOfferId(value);
    }

    public override string ToString() => Value.ToString("D");
}
