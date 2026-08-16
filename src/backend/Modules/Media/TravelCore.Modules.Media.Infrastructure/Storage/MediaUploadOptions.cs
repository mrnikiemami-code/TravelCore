using System.ComponentModel.DataAnnotations;
using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure.Storage;

/// <summary>
/// Media upload policy options. Section: <c>Media:Upload</c>.
/// </summary>
public sealed class MediaUploadOptions
{
    public const string SectionName = "Media:Upload";

    /// <summary>Maximum accepted upload size in bytes. Default 10 MiB.</summary>
    [Range(1, long.MaxValue)]
    public long MaxBytes { get; set; } = MediaUploadContentRules.DefaultMaxBytes;
}
