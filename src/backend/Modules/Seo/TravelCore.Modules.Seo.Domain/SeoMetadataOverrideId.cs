using TravelCore.Identifiers;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>Strongly typed SEO metadata-override identity (UUID v7).</summary>
public readonly record struct SeoMetadataOverrideId(Guid Value) : IEquatable<SeoMetadataOverrideId>
{
    public static SeoMetadataOverrideId New() => new(Uuid7.New());

    public static SeoMetadataOverrideId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("SeoMetadataOverrideId cannot be empty.", nameof(value));
        }

        return new SeoMetadataOverrideId(value);
    }

    public override string ToString() => Value.ToString("D");
}
