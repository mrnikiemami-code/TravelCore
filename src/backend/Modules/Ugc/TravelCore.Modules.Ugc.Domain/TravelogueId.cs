using TravelCore.Identifiers;

namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Strongly typed Travelogue identity (UUID v7).
/// </summary>
public readonly record struct TravelogueId(Guid Value) : IEquatable<TravelogueId>
{
    public static TravelogueId New() => new(Uuid7.New());

    public static TravelogueId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("TravelogueId cannot be empty.", nameof(value));
        }

        return new TravelogueId(value);
    }

    public override string ToString() => Value.ToString("D");
}
