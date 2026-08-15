using TravelCore.Identifiers;

namespace TravelCore.Modules.Party.Domain;

/// <summary>
/// Strongly typed Party identity (UUID v7 under the hood).
/// </summary>
public readonly record struct PartyId(Guid Value) : IEquatable<PartyId>
{
    public static PartyId New() => new(Uuid7.New());

    public static PartyId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PartyId cannot be empty.", nameof(value));
        }

        return new PartyId(value);
    }

    public override string ToString() => Value.ToString("D");
}
