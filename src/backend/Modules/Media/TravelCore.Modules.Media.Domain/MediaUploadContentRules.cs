using System.Text;

namespace TravelCore.Modules.Media.Domain;

/// <summary>
/// Upload MIME/size/payload rules for TC-P06-T004.
/// P06-R6 RESOLVED: SVG is denied (declared type, extension, and payload detection).
/// Extension alone and declared Content-Type alone are not authority.
/// </summary>
public static class MediaUploadContentRules
{
    public const long DefaultMaxBytes = 10 * 1024 * 1024;

    public static readonly HashSet<string> AllowedContentTypes = new(StringComparer.Ordinal)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    private static readonly HashSet<string> DeniedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".svg",
        ".svgz"
    };

    public static string NormalizeAndRequireAllowedContentType(string contentType)
    {
        var normalized = MediaAsset.NormalizeContentType(contentType);
        if (IsDeniedSvgContentType(normalized))
        {
            throw new ArgumentException(
                "SVG uploads are denied (P06-R6). image/svg+xml is not accepted.",
                nameof(contentType));
        }

        if (!AllowedContentTypes.Contains(normalized))
        {
            throw new ArgumentException(
                $"ContentType '{normalized}' is not in the P06 upload allowlist.",
                nameof(contentType));
        }

        return normalized;
    }

    public static long NormalizeAndRequireSize(long byteSize, long maxBytes = DefaultMaxBytes)
    {
        var size = MediaAsset.NormalizeByteSize(byteSize);
        if (maxBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBytes), maxBytes, "MaxBytes must be positive.");
        }

        if (size == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteSize), size, "Upload content cannot be empty.");
        }

        if (size > maxBytes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteSize),
                size,
                $"Upload exceeds max size of {maxBytes} bytes.");
        }

        return size;
    }

    public static void ValidateFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var trimmed = fileName.Trim();
        var extension = Path.GetExtension(trimmed);
        if (!string.IsNullOrEmpty(extension) && DeniedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                "SVG uploads are denied (P06-R6). .svg file names are not accepted.",
                nameof(fileName));
        }
    }

    /// <summary>
    /// Sniffs payload head: rejects SVG/XML SVG; optionally verifies raster magic vs declared type.
    /// </summary>
    public static void ValidatePayload(ReadOnlySpan<byte> head, string normalizedContentType)
    {
        if (head.IsEmpty)
        {
            throw new ArgumentException("Upload payload is empty.", nameof(head));
        }

        if (LooksLikeSvg(head))
        {
            throw new ArgumentException(
                "SVG/XML payload detected; SVG uploads are denied (P06-R6).",
                nameof(head));
        }

        if (!HasExpectedRasterSignature(head, normalizedContentType))
        {
            throw new ArgumentException(
                $"Payload signature does not match declared ContentType '{normalizedContentType}'.",
                nameof(head));
        }
    }

    public static bool IsDeniedSvgContentType(string normalizedContentType) =>
        normalizedContentType.Equals("image/svg+xml", StringComparison.Ordinal)
        || normalizedContentType.Equals("image/svg", StringComparison.Ordinal);

    public static bool LooksLikeSvg(ReadOnlySpan<byte> head)
    {
        var sample = Encoding.UTF8.GetString(head);
        var trimmed = sample.TrimStart('\uFEFF', ' ', '\t', '\r', '\n');
        if (trimmed.StartsWith("<svg", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (trimmed.Contains("<!DOCTYPE svg", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (trimmed.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase)
            && trimmed.Contains("<svg", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // SVG renamed to .png still often starts with whitespace + '<'
        if (trimmed.Contains("<svg", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    public static bool HasExpectedRasterSignature(ReadOnlySpan<byte> head, string normalizedContentType)
    {
        return normalizedContentType switch
        {
            "image/jpeg" => head.Length >= 3 && head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF,
            "image/png" => head.Length >= 8
                && head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47
                && head[4] == 0x0D && head[5] == 0x0A && head[6] == 0x1A && head[7] == 0x0A,
            "image/gif" => head.Length >= 6
                && head[0] == (byte)'G' && head[1] == (byte)'I' && head[2] == (byte)'F'
                && head[3] == (byte)'8' && (head[4] == (byte)'7' || head[4] == (byte)'9') && head[5] == (byte)'a',
            "image/webp" => head.Length >= 12
                && head[0] == (byte)'R' && head[1] == (byte)'I' && head[2] == (byte)'F' && head[3] == (byte)'F'
                && head[8] == (byte)'W' && head[9] == (byte)'E' && head[10] == (byte)'B' && head[11] == (byte)'P',
            _ => false
        };
    }
}
