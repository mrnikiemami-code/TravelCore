namespace TravelCore.Modules.Media.Contracts;

/// <summary>
/// Cross-module logical reference to a MediaAsset (TC-P06-T010 / P06-R5 CONTRACT-ONLY).
/// <para>
/// Consumers own business relationship semantics in their own schema when a real role exists.
/// Until then, this type documents the stable reference surface: MediaAsset public identity only.
/// </para>
/// <para>
/// Forbidden as a cross-module reference: StorageKey, filesystem paths, provider URLs,
/// Media.Infrastructure types, MediaDbContext, or Media-owned generic consumer link tables.
/// </para>
/// </summary>
public readonly record struct MediaAssetReference
{
    public MediaAssetReference(Guid mediaAssetId)
    {
        if (mediaAssetId == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(mediaAssetId));
        }

        MediaAssetId = mediaAssetId;
    }

    /// <summary>Stable Media-owned public identity (UUID). Not a storage key.</summary>
    public Guid MediaAssetId { get; }

    public static MediaAssetReference From(Guid mediaAssetId) => new(mediaAssetId);

    public override string ToString() => MediaAssetId.ToString("D");
}

/// <summary>
/// Marker documenting the accepted consumer → Media dependency direction for architecture proofs.
/// Consumers may reference <see cref="MediaAssetReference"/> / Media.Contracts only —
/// never Media.Infrastructure.
/// </summary>
public static class MediaConsumerReferenceBoundary
{
    public const string AllowedConsumerDependency = "TravelCore.Modules.Media.Contracts";

    public const string ForbiddenConsumerDependencyInfrastructure = "TravelCore.Modules.Media.Infrastructure";

    public const string ForbiddenConsumerDependencyDbContext = "MediaDbContext";

    /// <summary>
    /// Presentation URLs are derived via Media presentation contracts (T009 app proxy),
    /// not persisted on the consumer reference.
    /// </summary>
    public const string PresentationBoundary = "Media presentation contracts (app proxy); not StorageKey.";
}
