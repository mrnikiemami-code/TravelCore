using TravelCore.Identifiers;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Strongly typed SeoContentGraphNode identity (UUID v7).
/// </summary>
public readonly record struct SeoContentGraphNodeId(Guid Value) : IEquatable<SeoContentGraphNodeId>
{
    public static SeoContentGraphNodeId New() => new(Uuid7.New());

    public static SeoContentGraphNodeId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("SeoContentGraphNodeId cannot be empty.", nameof(value));
        }

        return new SeoContentGraphNodeId(value);
    }

    public override string ToString() => Value.ToString("D");
}
