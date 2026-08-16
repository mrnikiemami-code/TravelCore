namespace TravelCore.Modules.Media.Contracts;

/// <summary>
/// Cross-module MediaAsset Ready probe for consumer relationship attach (TC-P07-T005).
/// Exists + Ready only — never exposes StorageKey / EF entities.
/// </summary>
public interface IMediaAssetReadinessQuery
{
    /// <summary>
    /// Returns true only when the asset exists and Status is Ready.
    /// Empty Guid / nonexistent / non-Ready ⇒ false.
    /// </summary>
    Task<bool> IsReadyAsync(Guid mediaAssetId, CancellationToken cancellationToken = default);
}
