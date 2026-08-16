using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure.Storage;

/// <summary>
/// Local-filesystem development adapter behind Media storage port (TC-P06-T003).
/// Not an architectural SoT — only a concrete development provider.
/// </summary>
public sealed class LocalFileSystemMediaObjectStorage : IMediaObjectStorage
{
    private readonly string _root;

    public LocalFileSystemMediaObjectStorage(
        IOptions<MediaObjectStorageOptions> options,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(environment);

        var configured = options.Value.LocalRootPath;
        _root = ResolveRoot(configured, environment.ContentRootPath);
        Directory.CreateDirectory(_root);
    }

    public async Task PutAsync(MediaObjectPutRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Content);

        var key = MediaAsset.NormalizeStorageKey(request.StorageKey)
            ?? throw new ArgumentException("StorageKey is required.", nameof(request));
        var path = ResolveSafePath(key);

        if (File.Exists(path))
        {
            throw new InvalidOperationException($"Storage object already exists for key '{key}'.");
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var file = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            options: FileOptions.Asynchronous);

        await request.Content.CopyToAsync(file, cancellationToken);
        await file.FlushAsync(cancellationToken);

        // Content-type sidecar for local adapter (provider-local detail; not domain SoR).
        await File.WriteAllTextAsync(path + ".contenttype", MediaAsset.NormalizeContentType(request.ContentType), cancellationToken);
    }

    public async Task<MediaObjectReadResult?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var key = MediaAsset.NormalizeStorageKey(storageKey)
            ?? throw new ArgumentException("StorageKey is required.", nameof(storageKey));
        var path = ResolveSafePath(key);
        if (!File.Exists(path))
        {
            return null;
        }

        var contentType = "application/octet-stream";
        var typePath = path + ".contenttype";
        if (File.Exists(typePath))
        {
            contentType = (await File.ReadAllTextAsync(typePath, cancellationToken)).Trim();
        }

        var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            options: FileOptions.Asynchronous);

        return new MediaObjectReadResult(stream, contentType, stream.Length);
    }

    public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var key = MediaAsset.NormalizeStorageKey(storageKey)
            ?? throw new ArgumentException("StorageKey is required.", nameof(storageKey));
        var path = ResolveSafePath(key);
        return Task.FromResult(File.Exists(path));
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var key = MediaAsset.NormalizeStorageKey(storageKey)
            ?? throw new ArgumentException("StorageKey is required.", nameof(storageKey));
        var path = ResolveSafePath(key);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var typePath = path + ".contenttype";
        if (File.Exists(typePath))
        {
            File.Delete(typePath);
        }

        return Task.CompletedTask;
    }

    private static string ResolveRoot(string? configured, string contentRoot)
    {
        if (string.IsNullOrWhiteSpace(configured))
        {
            return Path.GetFullPath(Path.Combine(contentRoot, ".local", "media-objects"));
        }

        var trimmed = configured.Trim();
        return Path.IsPathRooted(trimmed)
            ? Path.GetFullPath(trimmed)
            : Path.GetFullPath(Path.Combine(contentRoot, trimmed));
    }

    private string ResolveSafePath(string storageKey)
    {
        // Normalize key already rejected .. \ and leading /
        var combined = Path.GetFullPath(Path.Combine(_root, storageKey.Replace('/', Path.DirectorySeparatorChar)));
        var rootFull = Path.GetFullPath(_root);
        if (!combined.StartsWith(rootFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(combined, rootFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Resolved storage path escapes configured root.");
        }

        return combined;
    }
}
