using NodaTime;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// SEO-owned graph node registry entry for a publishable resource identity.
/// Stores graph mechanics only — never editorial bodies or Destination hierarchy facts.
/// </summary>
public sealed class SeoContentGraphNode
{
    private SeoContentGraphNode()
    {
    }

    private SeoContentGraphNode(
        SeoContentGraphNodeId id,
        SeoResourceType resourceType,
        Guid resourceId,
        Instant createdAt)
    {
        Id = id;
        ResourceType = resourceType;
        ResourceId = resourceId;
        CreatedAt = createdAt;
    }

    public SeoContentGraphNodeId Id { get; private set; }

    public SeoResourceType ResourceType { get; private set; }

    /// <summary>Opaque identity owned by the business module.</summary>
    public Guid ResourceId { get; private set; }

    public Instant CreatedAt { get; private set; }

    public static SeoContentGraphNode Register(
        SeoResourceType resourceType,
        Guid resourceId,
        Instant createdAt)
    {
        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        if (!Enum.IsDefined(resourceType))
        {
            throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, "Unsupported SeoResourceType.");
        }

        return new SeoContentGraphNode(
            SeoContentGraphNodeId.New(),
            resourceType,
            resourceId,
            createdAt);
    }
}
