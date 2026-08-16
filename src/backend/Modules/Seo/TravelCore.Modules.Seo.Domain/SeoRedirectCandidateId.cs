using TravelCore.Identifiers;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Strongly typed SeoRedirectCandidate identity (UUID v7).
/// </summary>
public readonly record struct SeoRedirectCandidateId(Guid Value) : IEquatable<SeoRedirectCandidateId>
{
    public static SeoRedirectCandidateId New() => new(Uuid7.New());

    public static SeoRedirectCandidateId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("SeoRedirectCandidateId cannot be empty.", nameof(value));
        }

        return new SeoRedirectCandidateId(value);
    }

    public override string ToString() => Value.ToString("D");
}
