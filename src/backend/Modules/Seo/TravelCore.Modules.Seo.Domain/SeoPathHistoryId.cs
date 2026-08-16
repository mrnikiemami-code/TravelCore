using TravelCore.Identifiers;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Strongly typed SeoPathHistoryEntry identity (UUID v7).
/// </summary>
public readonly record struct SeoPathHistoryId(Guid Value) : IEquatable<SeoPathHistoryId>
{
    public static SeoPathHistoryId New() => new(Uuid7.New());

    public static SeoPathHistoryId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("SeoPathHistoryId cannot be empty.", nameof(value));
        }

        return new SeoPathHistoryId(value);
    }

    public override string ToString() => Value.ToString("D");
}
