namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Directed semantic internal link edge reference between publishable resources.
/// </summary>
public readonly record struct SeoInternalLinkReference(
    SeoResourceType SourceResourceType,
    Guid SourceResourceId,
    SeoResourceType TargetResourceType,
    Guid TargetResourceId,
    SeoInternalLinkDirection Direction)
{
    public static SeoInternalLinkReference Outbound(
        SeoResourceType sourceResourceType,
        Guid sourceResourceId,
        SeoResourceType targetResourceType,
        Guid targetResourceId) =>
        new(sourceResourceType, sourceResourceId, targetResourceType, targetResourceId, SeoInternalLinkDirection.Outbound);
}
