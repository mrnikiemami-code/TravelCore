using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modules.Destination.Infrastructure;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Infrastructure;
using TravelCore.Modules.Place.Contracts;
using TravelCore.Modules.Place.Infrastructure;
using TravelCore.Modules.ReferenceData.Infrastructure;

namespace TravelCore.Tools.DemoFeed;

/// <summary>
/// Deterministic Hotel Place demo seed (TC-DEMOFEED-T004).
/// Linked to demofeed Destination cities. Identifiable via demofeed- codes/slugs.
/// Optional Media cover via Media upload + Place SetCover — synthetic placeholder bytes only.
/// </summary>
internal static class PlaceDemoSeed
{
    // Minimal 1×1 PNG — generated placeholder, not scraped / not competitor content.
    private static readonly byte[] SyntheticPngPlaceholder = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    public static async Task<int> EnsureSchemaAsync(IServiceProvider root, CancellationToken ct)
    {
        await using var scope = root.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var referenceData = sp.GetRequiredService<ReferenceDataDbContext>();
        var destination = sp.GetRequiredService<DestinationDbContext>();
        var place = sp.GetRequiredService<PlaceDbContext>();
        var media = sp.GetRequiredService<MediaDbContext>();

        Console.WriteLine("Migrating ReferenceData (owner migrator)...");
        await ReferenceDataMigrator.MigrateAsync(referenceData, ct);
        Console.WriteLine("Migrating Destination (owner migrator)...");
        await DestinationMigrator.MigrateAsync(destination, ct);
        Console.WriteLine("Migrating Place (owner migrator)...");
        await PlaceMigrator.MigrateAsync(place, ct);
        Console.WriteLine("Migrating Media (owner migrator)...");
        await MediaMigrator.MigrateAsync(media, ct);
        Console.WriteLine("Owner schemas ready (ReferenceData · Destination · Place · Media).");
        return 0;
    }

    public static async Task<int> SeedAsync(IServiceProvider root, bool ensureSchema, CancellationToken ct)
    {
        if (ensureSchema)
        {
            var schemaExit = await EnsureSchemaAsync(root, ct);
            if (schemaExit != 0)
            {
                return schemaExit;
            }
        }

        await using var scope = root.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var destinations = sp.GetRequiredService<DestinationDbContext>();
        var places = sp.GetRequiredService<PlaceDbContext>();
        var placeApp = sp.GetRequiredService<IPlaceService>();
        var mediaUpload = sp.GetRequiredService<IMediaUploadService>();

        var tehranId = await RequireDestinationIdAsync(destinations, "demofeed-ir-teh", ct);
        var istanbulId = await RequireDestinationIdAsync(destinations, "demofeed-tr-ist", ct);

        await EnsureHotelAsync(
            places,
            placeApp,
            mediaUpload,
            code: "demofeed-hotel-teh-1",
            englishName: "DEMOFEED Sample Hotel Tehran 1",
            destinationId: tehranId,
            starRating: 4,
            faName: "هتل نمونه دمو تهران ۱ (DEMOFEED)",
            enName: "DEMOFEED Sample Hotel Tehran 1",
            faSlug: "demofeed-hotel-tehran-1",
            enSlug: "demofeed-hotel-tehran-1",
            ct);

        await EnsureHotelAsync(
            places,
            placeApp,
            mediaUpload,
            code: "demofeed-hotel-ist-1",
            englishName: "DEMOFEED Sample Hotel Istanbul 1",
            destinationId: istanbulId,
            starRating: 5,
            faName: "هتل نمونه دمو استانبول ۱ (DEMOFEED)",
            enName: "DEMOFEED Sample Hotel Istanbul 1",
            faSlug: "demofeed-hotel-istanbul-1",
            enSlug: "demofeed-hotel-istanbul-1",
            ct);

        var count = await places.Places.AsNoTracking()
            .CountAsync(x => x.Code.StartsWith(DemoFeedHost.DemoCodePrefix), ct);

        Console.WriteLine($"Place (Hotel) DEMOFEED seed complete. Rows with prefix '{DemoFeedHost.DemoCodePrefix}': {count}");
        return 0;
    }

    public static async Task<int> ListAsync(IServiceProvider root, CancellationToken ct)
    {
        await using var scope = root.CreateAsyncScope();
        var places = scope.ServiceProvider.GetRequiredService<PlaceDbContext>();
        var rows = await places.Places.AsNoTracking()
            .Where(x => x.Code.StartsWith(DemoFeedHost.DemoCodePrefix))
            .OrderBy(x => x.Code)
            .ToListAsync(ct);

        Console.WriteLine($"DEMOFEED places ({rows.Count}):");
        foreach (var row in rows)
        {
            Console.WriteLine(
                $"- {row.Code} | {row.Kind} | {row.EnglishName} | status={row.CatalogStatus} | dest={row.DestinationId} | id={row.Id.Value}");
        }

        return 0;
    }

    private static async Task<Guid> RequireDestinationIdAsync(
        DestinationDbContext db,
        string code,
        CancellationToken ct)
    {
        var id = await db.Destinations.AsNoTracking()
            .Where(x => x.Code == code)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(ct);

        if (id.Value == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"Required DEMOFEED destination '{code}' was not found. Run: seed destinations --ensure-schema");
        }

        return id.Value;
    }

    private static async Task EnsureHotelAsync(
        PlaceDbContext db,
        IPlaceService placeApp,
        IMediaUploadService mediaUpload,
        string code,
        string englishName,
        Guid destinationId,
        short starRating,
        string faName,
        string enName,
        string faSlug,
        string enSlug,
        CancellationToken ct)
    {
        var existing = await db.Places.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code, ct);

        PlaceResponse response;
        if (existing is not null)
        {
            Console.WriteLine($"Exists: {code}");
            response = (await placeApp.GetByCodeAsync(code, cancellationToken: ct))!;
        }
        else
        {
            Console.WriteLine($"Create: {code}");
            response = await placeApp.CreateAsync(
                new CreatePlaceRequest(
                    Kind: "Hotel",
                    Code: code,
                    EnglishName: englishName,
                    StarRating: starRating,
                    DestinationId: destinationId),
                ct);
        }

        await placeApp.UpsertTranslationAsync(
            response.Id,
            "fa",
            new UpsertPlaceTranslationRequest(faName, "داده نمونه DEMOFEED — غیرتولیدی.", faSlug),
            ct);

        await placeApp.UpsertTranslationAsync(
            response.Id,
            "en",
            new UpsertPlaceTranslationRequest(enName, "DEMOFEED sample data — non-production.", enSlug),
            ct);

        // Public browse typically needs Active; still labeled DEMOFEED in names.
        if (!string.Equals(response.CatalogStatus, "Active", StringComparison.OrdinalIgnoreCase))
        {
            response = await placeApp.SetCatalogStatusAsync(
                response.Id,
                new SetPlaceCatalogStatusRequest("Active"),
                ct);
            Console.WriteLine($"CatalogStatus Active: {code}");
        }

        await EnsureCoverAsync(placeApp, mediaUpload, response.Id, code, ct);
    }

    private static async Task EnsureCoverAsync(
        IPlaceService placeApp,
        IMediaUploadService mediaUpload,
        Guid placeId,
        string placeCode,
        CancellationToken ct)
    {
        var links = await placeApp.ListMediaLinksAsync(placeId, ct);
        if (links.Any(l => string.Equals(l.Role, "Cover", StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine($"Cover exists: {placeCode}");
            return;
        }

        await using var stream = new MemoryStream(SyntheticPngPlaceholder, writable: false);
        var asset = await mediaUpload.UploadAsync(
            stream,
            contentType: "image/png",
            fileName: $"{placeCode}-cover.png",
            contentLength: SyntheticPngPlaceholder.LongLength,
            cancellationToken: ct);

        await placeApp.SetCoverAsync(placeId, new SetPlaceCoverRequest(asset.Id), ct);
        Console.WriteLine($"Cover attached (MediaAsset {asset.Id}): {placeCode}");
    }
}
