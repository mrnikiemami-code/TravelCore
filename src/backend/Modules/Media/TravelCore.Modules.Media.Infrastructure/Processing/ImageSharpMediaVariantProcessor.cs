using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure.Processing;

/// <summary>
/// ImageSharp-backed raster processor (Infrastructure only — no Domain leak).
/// Output keeps source format: JPEG→JPEG, PNG→PNG, WebP→WebP. GIF is rejected by the use-case.
/// </summary>
public sealed class ImageSharpMediaVariantProcessor
{
    public async Task<DecodedRasterImage> DecodeAsync(
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        var normalized = MediaAsset.NormalizeContentType(contentType);
        EnsureSupportedOutputFormat(normalized);

        Image image;
        try
        {
            image = await Image.LoadAsync(content, cancellationToken);
        }
        catch (UnknownImageFormatException ex)
        {
            throw new ArgumentException(
                $"Unable to decode image content for ContentType '{normalized}'.",
                nameof(contentType),
                ex);
        }
        catch (InvalidImageContentException ex)
        {
            throw new ArgumentException(
                $"Image content is invalid or corrupt for ContentType '{normalized}'.",
                nameof(contentType),
                ex);
        }

        try
        {
            MediaVariantSizingPolicy.EnsureWithinDecodeLimits(image.Width, image.Height);
            return new DecodedRasterImage(image, normalized);
        }
        catch
        {
            image.Dispose();
            throw;
        }
    }

    public async Task<EncodedVariantBytes> EncodeFitWithinAsync(
        DecodedRasterImage source,
        int targetWidth,
        int targetHeight,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (targetWidth <= 0 || targetHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetWidth),
                $"Target dimensions must be positive (got {targetWidth}x{targetHeight}).");
        }

        // Clone so parallel profile encoding does not mutate the shared decoded buffer.
        using var working = source.Image.Clone(ctx =>
        {
            if (source.Image.Width != targetWidth || source.Image.Height != targetHeight)
            {
                ctx.Resize(targetWidth, targetHeight);
            }
        });

        await using var ms = new MemoryStream();
        switch (source.ContentType)
        {
            case "image/jpeg":
                await working.SaveAsJpegAsync(ms, new JpegEncoder { Quality = 85 }, cancellationToken);
                break;
            case "image/png":
                await working.SaveAsPngAsync(ms, new PngEncoder(), cancellationToken);
                break;
            case "image/webp":
                await working.SaveAsWebpAsync(ms, new WebpEncoder { Quality = 85 }, cancellationToken);
                break;
            default:
                throw new ArgumentException(
                    $"Unsupported variant output ContentType '{source.ContentType}'.",
                    nameof(source));
        }

        return new EncodedVariantBytes(
            ms.ToArray(),
            working.Width,
            working.Height,
            source.ContentType);
    }

    public static void EnsureSupportedOutputFormat(string normalizedContentType)
    {
        if (normalizedContentType.Equals("image/gif", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "GIF variant policy is unresolved; variant generation for image/gif is denied (fail-closed).",
                nameof(normalizedContentType));
        }

        if (normalizedContentType is not ("image/jpeg" or "image/png" or "image/webp"))
        {
            throw new ArgumentException(
                $"ContentType '{normalizedContentType}' is not supported for variant generation.",
                nameof(normalizedContentType));
        }
    }
}

public sealed class DecodedRasterImage : IDisposable
{
    public DecodedRasterImage(Image image, string contentType)
    {
        Image = image ?? throw new ArgumentNullException(nameof(image));
        ContentType = contentType;
    }

    public Image Image { get; }

    public string ContentType { get; }

    public int Width => Image.Width;

    public int Height => Image.Height;

    public void Dispose() => Image.Dispose();
}

public sealed record EncodedVariantBytes(
    byte[] Bytes,
    int Width,
    int Height,
    string ContentType);
