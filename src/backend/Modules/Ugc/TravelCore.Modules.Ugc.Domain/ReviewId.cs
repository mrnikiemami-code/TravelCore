using TravelCore.Identifiers;

namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Strongly typed Review identity (UUID v7).
/// </summary>
public readonly record struct ReviewId(Guid Value) : IEquatable<ReviewId>
{
    public static ReviewId New() => new(Uuid7.New());

    public static ReviewId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ReviewId cannot be empty.", nameof(value));
        }

        return new ReviewId(value);
    }

    public override string ToString() => Value.ToString("D");
}
