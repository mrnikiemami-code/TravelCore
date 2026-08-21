using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modules.Destination.Infrastructure;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Infrastructure;
using TravelCore.Modules.Place.Infrastructure;
using TravelCore.Modules.ReferenceData.Infrastructure;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Infrastructure;

namespace TravelCore.Tools.DemoFeed;

/// <summary>
/// Deterministic TourProduct demo seed (TC-DEMOFEED-T005).
/// Linked to demofeed Destination cities. Identifiable via demofeed- codes/slugs.
/// Optional Media cover via Media upload + Tour SetCover — synthetic placeholder bytes only.
/// No Booking / Pricing / Departure inserts.
/// </summary>
internal static class TourDemoSeed
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
        var tour = sp.GetRequiredService<TourDbContext>();
        var media = sp.GetRequiredService<MediaDbContext>();

        Console.WriteLine("Migrating ReferenceData (owner migrator)...");
        await ReferenceDataMigrator.MigrateAsync(referenceData, ct);
        Console.WriteLine("Migrating Destination (owner migrator)...");
        await DestinationMigrator.MigrateAsync(destination, ct);
        Console.WriteLine("Migrating Place (owner migrator)...");
        await PlaceMigrator.MigrateAsync(place, ct);
        Console.WriteLine("Migrating Tour (owner migrator)...");
        await TourMigrator.MigrateAsync(tour, ct);
        Console.WriteLine("Migrating Media (owner migrator)...");
        await MediaMigrator.MigrateAsync(media, ct);
        Console.WriteLine("Owner schemas ready (ReferenceData · Destination · Place · Tour · Media).");
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
        var tours = sp.GetRequiredService<TourDbContext>();
        var tourApp = sp.GetRequiredService<ITourProductService>();
        var semantic = sp.GetRequiredService<ITourProductSemanticLinkService>();
        var tourMedia = sp.GetRequiredService<ITourProductMediaService>();
        var mediaUpload = sp.GetRequiredService<IMediaUploadService>();

        var tehranId = await RequireDestinationIdAsync(destinations, "demofeed-ir-teh", ct);
        var istanbulId = await RequireDestinationIdAsync(destinations, "demofeed-tr-ist", ct);

        await EnsureTourAsync(
            tours,
            tourApp,
            semantic,
            tourMedia,
            mediaUpload,
            kind: "Package",
            code: "demofeed-tour-teh-1",
            englishName: "DEMOFEED Sample Tour Tehran 1",
            destinationId: tehranId,
            faTitle: "تور نمونه دمو تهران ۱ (DEMOFEED)",
            enTitle: "DEMOFEED Sample Tour Tehran 1",
            faSlug: "demofeed-tour-tehran-1",
            enSlug: "demofeed-tour-tehran-1",
            ct);

        await EnsureTourAsync(
            tours,
            tourApp,
            semantic,
            tourMedia,
            mediaUpload,
            kind: "Package",
            code: "demofeed-tour-ist-1",
            englishName: "DEMOFEED Sample Tour Istanbul 1",
            destinationId: istanbulId,
            faTitle: "تور نمونه دمو استانبول ۱ (DEMOFEED)",
            enTitle: "DEMOFEED Sample Tour Istanbul 1",
            faSlug: "demofeed-tour-istanbul-1",
            enSlug: "demofeed-tour-istanbul-1",
            ct);

        var count = await tours.TourProducts.AsNoTracking()
            .CountAsync(x => x.Code.StartsWith(DemoFeedHost.DemoCodePrefix), ct);

        Console.WriteLine($"Tour DEMOFEED seed complete. Rows with prefix '{DemoFeedHost.DemoCodePrefix}': {count}");
        return 0;
    }

    public static async Task<int> ListAsync(IServiceProvider root, CancellationToken ct)
    {
        await using var scope = root.CreateAsyncScope();
        var tours = scope.ServiceProvider.GetRequiredService<TourDbContext>();
        var rows = await tours.TourProducts.AsNoTracking()
            .Where(x => x.Code.StartsWith(DemoFeedHost.DemoCodePrefix))
            .OrderBy(x => x.Code)
            .ToListAsync(ct);

        Console.WriteLine($"DEMOFEED tours ({rows.Count}):");
        foreach (var row in rows)
        {
            Console.WriteLine(
                $"- {row.Code} | {row.Kind} | {row.EnglishName} | status={row.CatalogStatus} | id={row.Id.Value}");
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

    private static async Task EnsureTourAsync(
        TourDbContext db,
        ITourProductService tourApp,
        ITourProductSemanticLinkService semantic,
        ITourProductMediaService tourMedia,
        IMediaUploadService mediaUpload,
        string kind,
        string code,
        string englishName,
        Guid destinationId,
        string faTitle,
        string enTitle,
        string faSlug,
        string enSlug,
        CancellationToken ct)
    {
        var existing = await db.TourProducts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code, ct);

        TourProductResponse response;
        if (existing is not null)
        {
            Console.WriteLine($"Exists: {code}");
            response = (await tourApp.GetByCodeAsync(code, cancellationToken: ct))!;
        }
        else
        {
            Console.WriteLine($"Create: {code}");
            response = await tourApp.CreateAsync(
                new CreateTourProductRequest(kind, code, englishName),
                ct);
        }

        await tourApp.UpsertTranslationAsync(
            response.Id,
            "fa",
            new UpsertTourProductTranslationRequest(faTitle, "داده نمونه DEMOFEED — غیرتولیدی."),
            ct);

        await tourApp.UpsertTranslationAsync(
            response.Id,
            "en",
            new UpsertTourProductTranslationRequest(enTitle, "DEMOFEED sample data — non-production."),
            ct);

        await tourApp.SetTranslationSlugAsync(
            response.Id,
            "fa",
            new SetTourProductTranslationSlugRequest(faSlug),
            ct);

        await tourApp.SetTranslationSlugAsync(
            response.Id,
            "en",
            new SetTourProductTranslationSlugRequest(enSlug),
            ct);

        var links = await semantic.GetAsync(response.Id, ct);
        if (links is null || !links.DestinationIds.Contains(destinationId))
        {
            await semantic.AssignDestinationAsync(response.Id, destinationId, ct);
            Console.WriteLine($"Destination linked: {code} → {destinationId}");
        }
        else
        {
            Console.WriteLine($"Destination already linked: {code}");
        }

        // Catalog-visible for browse demos; Published ≠ bookable ≠ priced.
        response = (await tourApp.GetByCodeAsync(code, cancellationToken: ct))!;
        if (!string.Equals(response.CatalogStatus, "Published", StringComparison.OrdinalIgnoreCase))
        {
            response = await tourApp.SetCatalogStatusAsync(
                response.Id,
                new SetTourCatalogStatusRequest("Published"),
                ct);
            Console.WriteLine($"CatalogStatus Published: {code}");
        }

        await EnsureCoverAsync(tourMedia, mediaUpload, response.Id, code, ct);
    }

    private static async Task EnsureCoverAsync(
        ITourProductMediaService tourMedia,
        IMediaUploadService mediaUpload,
        Guid tourProductId,
        string tourCode,
        CancellationToken ct)
    {
        var media = await tourMedia.GetAsync(tourProductId, ct);
        if (media?.Cover is not null)
        {
            Console.WriteLine($"Cover exists: {tourCode}");
            return;
        }

        await using var stream = new MemoryStream(SyntheticPngPlaceholder, writable: false);
        var asset = await mediaUpload.UploadAsync(
            stream,
            contentType: "image/png",
            fileName: $"{tourCode}-cover.png",
            contentLength: SyntheticPngPlaceholder.LongLength,
            cancellationToken: ct);

        await tourMedia.SetCoverAsync(tourProductId, asset.Id, ct);
        Console.WriteLine($"Cover attached (MediaAsset {asset.Id}): {tourCode}");
    }
}
