using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure.Storage;

/// <summary>
/// Server-side opaque storage key generator. Never trusts caller filenames as physical path.
/// </summary>
public static class MediaStorageKeyGenerator
{
    public static string NewObjectKey(string contentType)
    {
        var mime = MediaAsset.NormalizeContentType(contentType);
        var extension = GuessExtension(mime);
        var id = MediaAssetId.New().Value.ToString("N");
        var now = DateTime.UtcNow;
        return $"{now:yyyy}/{now:MM}/{now:dd}/{id}{extension}";
    }

    private static string GuessExtension(string mime) =>
        mime switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "image/avif" => ".avif",
            _ => ".bin"
        };
}
