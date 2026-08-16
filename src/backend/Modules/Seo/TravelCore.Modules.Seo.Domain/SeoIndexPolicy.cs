using NodaTime;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// SEO-owned configured index/crawl posture for a resource+locale binding.
/// Never stored on Destination; missing policy means conservative noindex.
/// </summary>
public sealed class SeoIndexPolicy
{
    private SeoIndexPolicy()
    {
        Locale = null!;
    }

    private SeoIndexPolicy(
        SeoIndexPolicyId id,
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        SeoIndexDirective indexDirective,
        SeoFollowDirective followDirective,
        Instant updatedAt)
    {
        Id = id;
        ResourceType = resourceType;
        ResourceId = resourceId;
        Locale = locale;
        IndexDirective = indexDirective;
        FollowDirective = followDirective;
        UpdatedAt = updatedAt;
    }

    public SeoIndexPolicyId Id { get; private set; }

    public SeoResourceType ResourceType { get; private set; }

    public Guid ResourceId { get; private set; }

    public string Locale { get; private set; }

    public SeoIndexDirective IndexDirective { get; private set; }

    public SeoFollowDirective FollowDirective { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static SeoIndexPolicy Create(
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        SeoIndexDirective indexDirective,
        SeoFollowDirective followDirective,
        Instant now,
        SeoIndexPolicyId? id = null)
    {
        if (!Enum.IsDefined(resourceType))
        {
            throw new ArgumentOutOfRangeException(nameof(resourceType));
        }

        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        if (!Enum.IsDefined(indexDirective))
        {
            throw new ArgumentOutOfRangeException(nameof(indexDirective));
        }

        if (!Enum.IsDefined(followDirective))
        {
            throw new ArgumentOutOfRangeException(nameof(followDirective));
        }

        return new SeoIndexPolicy(
            id ?? SeoIndexPolicyId.New(),
            resourceType,
            resourceId,
            SeoRoute.NormalizeLocale(locale),
            indexDirective,
            followDirective,
            now);
    }

    public void Replace(
        SeoIndexDirective indexDirective,
        SeoFollowDirective followDirective,
        Instant now)
    {
        if (!Enum.IsDefined(indexDirective))
        {
            throw new ArgumentOutOfRangeException(nameof(indexDirective));
        }

        if (!Enum.IsDefined(followDirective))
        {
            throw new ArgumentOutOfRangeException(nameof(followDirective));
        }

        IndexDirective = indexDirective;
        FollowDirective = followDirective;
        UpdatedAt = now;
    }
}

public enum SeoIndexDirective : short
{
    NoIndex = 1,
    Index = 2,
}

public enum SeoFollowDirective : short
{
    Follow = 1,
    NoFollow = 2,
}
