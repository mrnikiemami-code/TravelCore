using TravelCore.Identifiers;

namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Strongly typed AgencyProfile identity (UUID v7).
/// </summary>
public readonly record struct AgencyProfileId(Guid Value) : IEquatable<AgencyProfileId>
{
    public static AgencyProfileId New() => new(Uuid7.New());

    public static AgencyProfileId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("AgencyProfileId cannot be empty.", nameof(value));
        }

        return new AgencyProfileId(value);
    }

    public override string ToString() => Value.ToString("D");
}
