using System.ComponentModel.DataAnnotations;

namespace TravelCore.Modules.Media.Infrastructure.Storage;

/// <summary>
/// Media-owned object storage options. Section: <c>Media:ObjectStorage</c>.
/// </summary>
public sealed class MediaObjectStorageOptions
{
    public const string SectionName = "Media:ObjectStorage";

    /// <summary>
    /// Development local-filesystem root. Relative paths resolve against content root.
    /// Empty = default <c>.local/media-objects</c> under content root.
    /// </summary>
    [MaxLength(1024)]
    public string? LocalRootPath { get; set; }

    /// <summary>
    /// When true, register in-memory adapter (tests). Default false = local filesystem.
    /// </summary>
    public bool UseInMemory { get; set; }
}
