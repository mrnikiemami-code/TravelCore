using TravelCore.Identifiers;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>Strongly typed SEO IndexPolicy identity (UUID v7).</summary>
public readonly record struct SeoIndexPolicyId(Guid Value) : IEquatable<SeoIndexPolicyId>
{
    public static SeoIndexPolicyId New() => new(Uuid7.New());

    public static SeoIndexPolicyId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("SeoIndexPolicyId cannot be empty.", nameof(value));
        }

        return new SeoIndexPolicyId(value);
    }

    public override string ToString() => Value.ToString("D");
}
