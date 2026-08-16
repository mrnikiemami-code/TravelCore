using TravelCore.Identifiers;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Strongly typed live SEO redirect identity (UUID v7).
/// </summary>
public readonly record struct SeoRedirectId(Guid Value) : IEquatable<SeoRedirectId>
{
    public static SeoRedirectId New() => new(Uuid7.New());

    public static SeoRedirectId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("SeoRedirectId cannot be empty.", nameof(value));
        }

        return new SeoRedirectId(value);
    }

    public override string ToString() => Value.ToString("D");
}
