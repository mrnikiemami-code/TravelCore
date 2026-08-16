using TravelCore.Identifiers;

namespace TravelCore.Modules.Content.Domain;

/// <summary>Strongly typed Content Block identity (UUID v7). P08-R2 relational storage.</summary>
public readonly record struct ContentBlockId(Guid Value) : IEquatable<ContentBlockId>
{
    public static ContentBlockId New() => new(Uuid7.New());

    public static ContentBlockId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ContentBlockId cannot be empty.", nameof(value));
        }

        return new ContentBlockId(value);
    }

    public override string ToString() => Value.ToString("D");
}
