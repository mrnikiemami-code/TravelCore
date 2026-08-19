namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Logical hub/cluster taxonomy reference for SEO graph semantics.
/// </summary>
public readonly record struct SeoHubClusterReference(
    SeoHubClusterKind Kind,
    SeoResourceType ResourceType,
    Guid ResourceId)
{
    public static SeoHubClusterReference DestinationHub(SeoResourceType resourceType, Guid resourceId) =>
        new(SeoHubClusterKind.DestinationHub, resourceType, resourceId);

    public static SeoHubClusterReference ContentCluster(SeoResourceType resourceType, Guid resourceId) =>
        new(SeoHubClusterKind.ContentCluster, resourceType, resourceId);
}
