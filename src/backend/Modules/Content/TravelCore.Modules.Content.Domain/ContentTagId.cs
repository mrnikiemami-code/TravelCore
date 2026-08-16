using TravelCore.Identifiers;

namespace TravelCore.Modules.Content.Domain;

/// <summary>Strongly typed Content Tag identity (UUID v7).</summary>
public readonly record struct ContentTagId(Guid Value) : IEquatable<ContentTagId>
{
    public static ContentTagId New() => new(Uuid7.New());

    public static ContentTagId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ContentTagId cannot be empty.", nameof(value));
        }

        return new ContentTagId(value);
    }

    public override string ToString() => Value.ToString("D");
}
