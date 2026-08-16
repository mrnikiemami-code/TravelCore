using TravelCore.Identifiers;

namespace TravelCore.Modules.Content.Domain;

/// <summary>Strongly typed Content Category identity (UUID v7).</summary>
public readonly record struct ContentCategoryId(Guid Value) : IEquatable<ContentCategoryId>
{
    public static ContentCategoryId New() => new(Uuid7.New());

    public static ContentCategoryId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ContentCategoryId cannot be empty.", nameof(value));
        }

        return new ContentCategoryId(value);
    }

    public override string ToString() => Value.ToString("D");
}
