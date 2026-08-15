using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.ReferenceData.Domain;

namespace TravelCore.Modules.ReferenceData.Infrastructure.Seeding;

/// <summary>
/// Idempotent baseline seeds for stable ReferenceData catalogs (TC-P04-T002).
/// </summary>
public static class ReferenceDataCatalogSeeder
{
    public static async Task EnsureSeededAsync(ReferenceDataDbContext db, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);

        await EnsureCurrenciesAsync(db, cancellationToken);
        await EnsureLocalesAsync(db, cancellationToken);
        await EnsureCountriesAsync(db, cancellationToken);
        await EnsureTimeZonesAsync(db, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureCurrenciesAsync(ReferenceDataDbContext db, CancellationToken cancellationToken)
    {
        var seeds = new[]
        {
            CurrencyCatalogEntry.Create("USD", "US Dollar", 2, "$"),
            CurrencyCatalogEntry.Create("EUR", "Euro", 2, "€"),
            CurrencyCatalogEntry.Create("IRR", "Iranian Rial", 0, "﷼"),
        };

        foreach (var seed in seeds)
        {
            if (!await db.Currencies.AnyAsync(x => x.Code == seed.Code, cancellationToken))
            {
                db.Currencies.Add(seed);
            }
        }
    }

    private static async Task EnsureLocalesAsync(ReferenceDataDbContext db, CancellationToken cancellationToken)
    {
        var seeds = new[]
        {
            LocaleCatalogEntry.Create("en", "English"),
            LocaleCatalogEntry.Create("fa", "Persian"),
            LocaleCatalogEntry.Create("en-US", "English (United States)"),
            LocaleCatalogEntry.Create("fa-IR", "Persian (Iran)"),
        };

        foreach (var seed in seeds)
        {
            if (!await db.Locales.AnyAsync(x => x.Code == seed.Code, cancellationToken))
            {
                db.Locales.Add(seed);
            }
        }
    }

    private static async Task EnsureCountriesAsync(ReferenceDataDbContext db, CancellationToken cancellationToken)
    {
        var seeds = new[]
        {
            CountryCatalogEntry.Create("IR", "IRN", "Iran", "364"),
            CountryCatalogEntry.Create("TR", "TUR", "Türkiye", "792"),
            CountryCatalogEntry.Create("US", "USA", "United States of America", "840"),
        };

        foreach (var seed in seeds)
        {
            if (!await db.Countries.AnyAsync(x => x.Alpha2Code == seed.Alpha2Code, cancellationToken))
            {
                db.Countries.Add(seed);
            }
        }
    }

    private static async Task EnsureTimeZonesAsync(ReferenceDataDbContext db, CancellationToken cancellationToken)
    {
        var seeds = new[]
        {
            TimeZoneCatalogEntry.Create("UTC", "Coordinated Universal Time"),
            TimeZoneCatalogEntry.Create("Asia/Tehran", "Iran Standard Time"),
            TimeZoneCatalogEntry.Create("Europe/Istanbul", "Turkey Time"),
        };

        foreach (var seed in seeds)
        {
            if (!await db.TimeZones.AnyAsync(x => x.Id == seed.Id, cancellationToken))
            {
                db.TimeZones.Add(seed);
            }
        }
    }
}
