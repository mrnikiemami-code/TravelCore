using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Destination.Infrastructure;
using TravelCore.Modules.Destination.Infrastructure.Services;
using TravelCore.Modules.ReferenceData.Infrastructure;

namespace TravelCore.Tools.DemoFeed;

/// <summary>
/// Deterministic Destination demo seed (TC-DEMOFEED-T003).
/// Identifiable via code/slug prefix demofeed-. Removable. Not production claims.
/// </summary>
internal static class DestinationDemoSeed
{
    public static async Task<int> EnsureSchemaAsync(IServiceProvider root, CancellationToken ct)
    {
        await using var scope = root.CreateAsyncScope();
        var referenceData = scope.ServiceProvider.GetRequiredService<ReferenceDataDbContext>();
        var destination = scope.ServiceProvider.GetRequiredService<DestinationDbContext>();

        Console.WriteLine("Migrating ReferenceData (owner migrator)...");
        await ReferenceDataMigrator.MigrateAsync(referenceData, ct);
        Console.WriteLine("Migrating Destination (owner migrator)...");
        await DestinationMigrator.MigrateAsync(destination, ct);
        Console.WriteLine("Owner schemas ready.");
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
        var app = sp.GetRequiredService<DestinationApplicationService>();

        // Explicitly labeled DEMOFEED sample geography — not live inventory / commercial facts.
        var iran = await EnsureDestinationAsync(
            destinations,
            app,
            kind: "Country",
            code: "demofeed-ir",
            englishName: "DEMOFEED Sample Country IR",
            parentId: null,
            isoCountryCode: "IR",
            faName: "نمونه دمو ایران (DEMOFEED)",
            enName: "DEMOFEED Sample Iran",
            faSlug: "demofeed-iran",
            enSlug: "demofeed-iran",
            ct);

        var tehranRegion = await EnsureDestinationAsync(
            destinations,
            app,
            kind: "Region",
            code: "demofeed-ir-thr",
            englishName: "DEMOFEED Sample Region Tehran",
            parentId: iran.Id,
            isoCountryCode: null,
            faName: "نمونه دمو استان تهران (DEMOFEED)",
            enName: "DEMOFEED Sample Tehran Region",
            faSlug: "demofeed-tehran-region",
            enSlug: "demofeed-tehran-region",
            ct);

        await EnsureDestinationAsync(
            destinations,
            app,
            kind: "City",
            code: "demofeed-ir-teh",
            englishName: "DEMOFEED Sample City Tehran",
            parentId: tehranRegion.Id,
            isoCountryCode: null,
            faName: "نمونه دمو شهر تهران (DEMOFEED)",
            enName: "DEMOFEED Sample Tehran City",
            faSlug: "demofeed-tehran",
            enSlug: "demofeed-tehran",
            ct);

        var turkey = await EnsureDestinationAsync(
            destinations,
            app,
            kind: "Country",
            code: "demofeed-tr",
            englishName: "DEMOFEED Sample Country TR",
            parentId: null,
            isoCountryCode: "TR",
            faName: "نمونه دمو ترکیه (DEMOFEED)",
            enName: "DEMOFEED Sample Turkey",
            faSlug: "demofeed-turkey",
            enSlug: "demofeed-turkey",
            ct);

        await EnsureDestinationAsync(
            destinations,
            app,
            kind: "City",
            code: "demofeed-tr-ist",
            englishName: "DEMOFEED Sample City Istanbul",
            parentId: turkey.Id,
            isoCountryCode: null,
            faName: "نمونه دمو استانبول (DEMOFEED)",
            enName: "DEMOFEED Sample Istanbul City",
            faSlug: "demofeed-istanbul",
            enSlug: "demofeed-istanbul",
            ct);

        var count = await destinations.Destinations.AsNoTracking()
            .CountAsync(x => x.Code.StartsWith(DemoFeedHost.DemoCodePrefix), ct);

        Console.WriteLine($"Destination DEMOFEED seed complete. Rows with prefix '{DemoFeedHost.DemoCodePrefix}': {count}");
        return 0;
    }

    public static async Task<int> ListAsync(IServiceProvider root, CancellationToken ct)
    {
        await using var scope = root.CreateAsyncScope();
        var destinations = scope.ServiceProvider.GetRequiredService<DestinationDbContext>();
        var rows = await destinations.Destinations.AsNoTracking()
            .Where(x => x.Code.StartsWith(DemoFeedHost.DemoCodePrefix))
            .OrderBy(x => x.Code)
            .ToListAsync(ct);

        Console.WriteLine($"DEMOFEED destinations ({rows.Count}):");
        foreach (var row in rows)
        {
            Console.WriteLine($"- {row.Code} | {row.Kind} | {row.EnglishName} | id={row.Id.Value}");
        }

        return 0;
    }

    private static async Task<DestinationResponse> EnsureDestinationAsync(
        DestinationDbContext db,
        DestinationApplicationService app,
        string kind,
        string code,
        string englishName,
        Guid? parentId,
        string? isoCountryCode,
        string faName,
        string enName,
        string faSlug,
        string enSlug,
        CancellationToken ct)
    {
        var existing = await db.Destinations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code, ct);

        DestinationResponse response;
        if (existing is not null)
        {
            Console.WriteLine($"Exists: {code}");
            response = new DestinationResponse(
                existing.Id.Value,
                existing.Kind.ToString(),
                existing.Code,
                existing.EnglishName,
                existing.ParentId?.Value,
                existing.IsoCountryCode,
                existing.Latitude,
                existing.Longitude);
        }
        else
        {
            Console.WriteLine($"Create: {code}");
            response = await app.CreateAsync(
                new CreateDestinationRequest(kind, code, englishName, parentId, isoCountryCode),
                ct);
        }

        await app.UpsertTranslationAsync(
            response.Id,
            "fa",
            new UpsertDestinationTranslationRequest(faName, "داده نمونه DEMOFEED — غیرتولیدی.", faSlug),
            ct);

        await app.UpsertTranslationAsync(
            response.Id,
            "en",
            new UpsertDestinationTranslationRequest(enName, "DEMOFEED sample data — non-production.", enSlug),
            ct);

        return response;
    }
}
