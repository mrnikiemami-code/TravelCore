namespace TravelCore.Modules.Seo.Contracts;

/// <summary>
/// Logical publishable resource reference for SEO content graph mechanics.
/// Does not imply a peer-schema FK or editorial/content ownership transfer.
/// </summary>
public readonly record struct SeoContentGraphReference(string ResourceType, Guid ResourceId)
{
    public static SeoContentGraphReference From(string resourceType, Guid resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceType))
        {
            throw new ArgumentException("ResourceType is required.", nameof(resourceType));
        }

        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        return new SeoContentGraphReference(resourceType.Trim(), resourceId);
    }
}
