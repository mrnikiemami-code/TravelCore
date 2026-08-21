using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Destination.Infrastructure.Services;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Place.Contracts;
using TravelCore.Modules.Tour.Contracts;

namespace TravelCore.Tools.DemoFeed;

/// <summary>
/// TC-P32-T002 / T008 — enrich DEMOFEED Place/Tour/Destination media from the P32 demo asset pack via owner APIs.
/// </summary>
internal static class DemoMediaPackEnricher
{
    private const string EnrichmentTaskId = "TC-P32-T008";

    public static async Task<int> EnrichAsync(IServiceProvider root, string[] args, CancellationToken ct)
    {
        var packRoot = ResolvePackRoot(args);
        var manifestPath = Path.Combine(packRoot, "manifest.json");
        if (!File.Exists(manifestPath))
        {
            Console.Error.WriteLine($"Manifest not found: {manifestPath}");
            return 3;
        }

        var manifest = JsonSerializer.Deserialize<DemoMediaManifest>(
            await File.ReadAllTextAsync(manifestPath, ct),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (manifest?.Assets is null || manifest.Assets.Count == 0)
        {
            Console.Error.WriteLine("Manifest has no assets.");
            return 3;
        }

        Console.WriteLine($"Pack root: {packRoot}");
        Console.WriteLine($"Manifest: {manifest.PackId} v{manifest.Version} ({manifest.Assets.Count} assets)");
        Console.WriteLine($"Enrichment task: {EnrichmentTaskId}");

        var ledgerPath = Path.Combine(DemoFeedHost.ResolveMediaRoot(), "enrichment-ledger.json");
        var ledger = await LoadLedgerAsync(ledgerPath, ct);

        await using var scope = root.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var destinationApp = sp.GetRequiredService<DestinationApplicationService>();
        var destinationMedia = sp.GetRequiredService<IDestinationMediaService>();
        var placeApp = sp.GetRequiredService<IPlaceService>();
        var tourProducts = sp.GetRequiredService<ITourProductService>();
        var tourMedia = sp.GetRequiredService<ITourProductMediaService>();
        var mediaUpload = sp.GetRequiredService<IMediaUploadService>();
        var translations = sp.GetRequiredService<IMediaAssetTranslationService>();

        var stats = new EnrichStats();

        foreach (var asset in manifest.Assets)
        {
            var filePath = Path.Combine(packRoot, asset.File);
            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"Missing file: {asset.File}");
                stats.Failed++;
                continue;
            }

            switch (asset.EntityType.ToLowerInvariant())
            {
                case "destination":
                    await EnrichDestinationAsync(
                        destinationApp,
                        destinationMedia,
                        mediaUpload,
                        translations,
                        ledger,
                        asset,
                        filePath,
                        stats,
                        ct);
                    break;

                case "hotel":
                    await EnrichHotelAsync(
                        placeApp,
                        mediaUpload,
                        translations,
                        ledger,
                        asset,
                        filePath,
                        stats,
                        ct);
                    break;

                case "tour":
                    await EnrichTourAsync(
                        tourProducts,
                        tourMedia,
                        mediaUpload,
                        translations,
                        ledger,
                        asset,
                        filePath,
                        stats,
                        ct);
                    break;

                default:
                    Console.Error.WriteLine($"Unknown entityType: {asset.EntityType}");
                    stats.Failed++;
                    break;
            }
        }

        await SaveLedgerAsync(ledgerPath, ledger, ct);

        Console.WriteLine();
        Console.WriteLine(
            $"Enrich complete. applied={stats.Applied} skipped={stats.Skipped} failed={stats.Failed}");
        return stats.Failed > 0 ? 3 : 0;
    }

    private static async Task EnrichDestinationAsync(
        DestinationApplicationService destinationApp,
        IDestinationMediaService destinationMedia,
        IMediaUploadService mediaUpload,
        IMediaAssetTranslationService translations,
        Dictionary<string, Guid> ledger,
        DemoMediaAsset asset,
        string filePath,
        EnrichStats stats,
        CancellationToken ct)
    {
        var destination = await destinationApp.GetByCodeAsync(asset.EntityCode, locale: null, cancellationToken: ct);
        if (destination is null)
        {
            Console.Error.WriteLine($"Destination not found: {asset.EntityCode}");
            stats.Failed++;
            return;
        }

        if (asset.Role.Equals("gallery", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(
                $"SKIP destination gallery ({asset.EntityCode}/{asset.File}): Gallery deferred (Option A Cover-only).");
            stats.Skipped++;
            return;
        }

        var key = LedgerKey(asset);
        const string role = "Cover";

        if (ledger.TryGetValue(key, out var existingAssetId))
        {
            var links = await destinationMedia.ListMediaLinksAsync(destination.Id, ct);
            if (links.Any(l =>
                    l.MediaAssetId == existingAssetId &&
                    string.Equals(l.Role, role, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"SKIP destination {role} already linked: {asset.EntityCode} ← {asset.File}");
                stats.Skipped++;
                return;
            }
        }

        var uploaded = await UploadWithAltAsync(mediaUpload, translations, asset, filePath, ct);

        var existingLinks = await destinationMedia.ListMediaLinksAsync(destination.Id, ct);
        if (existingLinks.Any(l => string.Equals(l.Role, "Cover", StringComparison.OrdinalIgnoreCase)))
        {
            await destinationMedia.RemoveCoverAsync(destination.Id, ct);
        }

        await destinationMedia.SetCoverAsync(destination.Id, new SetDestinationCoverRequest(uploaded.Id), ct);
        Console.WriteLine($"SET destination cover: {asset.EntityCode} ← {asset.File} ({uploaded.Id})");

        ledger[key] = uploaded.Id;
        stats.Applied++;
    }

    private static async Task EnrichHotelAsync(
        IPlaceService placeApp,
        IMediaUploadService mediaUpload,
        IMediaAssetTranslationService translations,
        Dictionary<string, Guid> ledger,
        DemoMediaAsset asset,
        string filePath,
        EnrichStats stats,
        CancellationToken ct)
    {
        var place = await placeApp.GetByCodeAsync(asset.EntityCode, locale: null, cancellationToken: ct);
        if (place is null)
        {
            Console.Error.WriteLine($"Hotel not found: {asset.EntityCode}");
            stats.Failed++;
            return;
        }

        var key = LedgerKey(asset);
        var role = asset.Role.Equals("gallery", StringComparison.OrdinalIgnoreCase) ? "Gallery" : "Cover";

        if (ledger.TryGetValue(key, out var existingAssetId))
        {
            var links = await placeApp.ListMediaLinksAsync(place.Id, ct);
            if (links.Any(l =>
                    l.MediaAssetId == existingAssetId &&
                    string.Equals(l.Role, role, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"SKIP hotel {role} already linked: {asset.EntityCode} ← {asset.File}");
                stats.Skipped++;
                return;
            }
        }

        var uploaded = await UploadWithAltAsync(mediaUpload, translations, asset, filePath, ct);

        if (role == "Cover")
        {
            var links = await placeApp.ListMediaLinksAsync(place.Id, ct);
            if (links.Any(l => string.Equals(l.Role, "Cover", StringComparison.OrdinalIgnoreCase)))
            {
                await placeApp.RemoveCoverAsync(place.Id, ct);
            }

            await placeApp.SetCoverAsync(place.Id, new SetPlaceCoverRequest(uploaded.Id), ct);
            Console.WriteLine($"SET hotel cover: {asset.EntityCode} ← {asset.File} ({uploaded.Id})");
        }
        else
        {
            await placeApp.AddGalleryItemAsync(
                place.Id,
                new AddPlaceGalleryItemRequest(uploaded.Id),
                ct);
            Console.WriteLine($"ADD hotel gallery: {asset.EntityCode} ← {asset.File} ({uploaded.Id})");
        }

        ledger[key] = uploaded.Id;
        stats.Applied++;
    }

    private static async Task EnrichTourAsync(
        ITourProductService tourProducts,
        ITourProductMediaService tourMedia,
        IMediaUploadService mediaUpload,
        IMediaAssetTranslationService translations,
        Dictionary<string, Guid> ledger,
        DemoMediaAsset asset,
        string filePath,
        EnrichStats stats,
        CancellationToken ct)
    {
        var tour = await tourProducts.GetByCodeAsync(asset.EntityCode, localeCode: null, cancellationToken: ct);
        if (tour is null)
        {
            Console.Error.WriteLine($"Tour not found: {asset.EntityCode}");
            stats.Failed++;
            return;
        }

        var key = LedgerKey(asset);
        var role = asset.Role.Equals("gallery", StringComparison.OrdinalIgnoreCase) ? "Gallery" : "Cover";
        var media = await tourMedia.GetAsync(tour.Id, ct);

        if (ledger.TryGetValue(key, out var existingAssetId))
        {
            var already =
                (role == "Cover" && media?.Cover?.MediaAssetId == existingAssetId) ||
                (role == "Gallery" && media?.Gallery.Any(g => g.MediaAssetId == existingAssetId) == true);
            if (already)
            {
                Console.WriteLine($"SKIP tour {role} already linked: {asset.EntityCode} ← {asset.File}");
                stats.Skipped++;
                return;
            }
        }

        var uploaded = await UploadWithAltAsync(mediaUpload, translations, asset, filePath, ct);

        if (role == "Cover")
        {
            if (media?.Cover is not null)
            {
                await tourMedia.RemoveCoverAsync(tour.Id, ct);
            }

            await tourMedia.SetCoverAsync(tour.Id, uploaded.Id, ct);
            Console.WriteLine($"SET tour cover: {asset.EntityCode} ← {asset.File} ({uploaded.Id})");
        }
        else
        {
            await tourMedia.AddGalleryItemAsync(tour.Id, uploaded.Id, sortOrder: null, ct);
            Console.WriteLine($"ADD tour gallery: {asset.EntityCode} ← {asset.File} ({uploaded.Id})");
        }

        ledger[key] = uploaded.Id;
        stats.Applied++;
    }

    private static async Task<MediaAssetResponse> UploadWithAltAsync(
        IMediaUploadService mediaUpload,
        IMediaAssetTranslationService translations,
        DemoMediaAsset asset,
        string filePath,
        CancellationToken ct)
    {
        await using var stream = File.OpenRead(filePath);
        var uploaded = await mediaUpload.UploadAsync(
            stream,
            contentType: "image/png",
            fileName: asset.File,
            contentLength: stream.Length,
            cancellationToken: ct);

        if (!string.IsNullOrWhiteSpace(asset.Alt?.Fa))
        {
            await translations.UpsertAsync(
                uploaded.Id,
                "fa",
                new UpsertMediaAssetTranslationRequest(asset.Alt.Fa, Caption: null, PublicationStatus: "Published"),
                ct);
        }

        if (!string.IsNullOrWhiteSpace(asset.Alt?.En))
        {
            await translations.UpsertAsync(
                uploaded.Id,
                "en",
                new UpsertMediaAssetTranslationRequest(asset.Alt.En, Caption: null, PublicationStatus: "Published"),
                ct);
        }

        return uploaded;
    }

    private static string LedgerKey(DemoMediaAsset asset) =>
        $"{asset.EntityType}:{asset.EntityCode}:{asset.Role}:{asset.File}".ToLowerInvariant();

    private static string ResolvePackRoot(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals("--pack-root", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(args[i + 1]);
            }
        }

        // Prefer repo docs pack: tools/demofeed → repo root → docs/product-experience/assets/demo-media
        var fromCwd = Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(),
            "docs",
            "product-experience",
            "assets",
            "demo-media"));
        if (File.Exists(Path.Combine(fromCwd, "manifest.json")))
        {
            return fromCwd;
        }

        var fromBase = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "docs",
            "product-experience",
            "assets",
            "demo-media"));
        return fromBase;
    }

    private static async Task<Dictionary<string, Guid>> LoadLedgerAsync(string path, CancellationToken ct)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var json = await File.ReadAllTextAsync(path, ct);
            var data = JsonSerializer.Deserialize<Dictionary<string, Guid>>(json);
            return data is null
                ? new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, Guid>(data, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static async Task SaveLedgerAsync(string path, Dictionary<string, Guid> ledger, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var json = JsonSerializer.Serialize(ledger, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json, ct);
    }

    private sealed class EnrichStats
    {
        public int Applied { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
    }

    private sealed class DemoMediaManifest
    {
        [JsonPropertyName("packId")]
        public string PackId { get; set; } = "";

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("assets")]
        public List<DemoMediaAsset> Assets { get; set; } = [];
    }

    private sealed class DemoMediaAsset
    {
        [JsonPropertyName("file")]
        public string File { get; set; } = "";

        [JsonPropertyName("entityType")]
        public string EntityType { get; set; } = "";

        [JsonPropertyName("entityCode")]
        public string EntityCode { get; set; } = "";

        [JsonPropertyName("role")]
        public string Role { get; set; } = "cover";

        [JsonPropertyName("alt")]
        public DemoMediaAlt? Alt { get; set; }
    }

    private sealed class DemoMediaAlt
    {
        [JsonPropertyName("fa")]
        public string? Fa { get; set; }

        [JsonPropertyName("en")]
        public string? En { get; set; }
    }
}
