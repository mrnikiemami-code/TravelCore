using TravelCore.Identifiers;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Strongly typed SeoRoute identity (UUID v7).
/// </summary>
public readonly record struct SeoRouteId(Guid Value) : IEquatable<SeoRouteId>
{
    public static SeoRouteId New() => new(Uuid7.New());

    public static SeoRouteId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("SeoRouteId cannot be empty.", nameof(value));
        }

        return new SeoRouteId(value);
    }

    public override string ToString() => Value.ToString("D");
}
