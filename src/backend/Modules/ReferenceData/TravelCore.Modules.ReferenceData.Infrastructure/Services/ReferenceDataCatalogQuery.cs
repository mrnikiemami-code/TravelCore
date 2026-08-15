using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.ReferenceData.Contracts;
using TravelCore.Money;

namespace TravelCore.Modules.ReferenceData.Infrastructure.Services;

public sealed class ReferenceDataCatalogQuery : IReferenceDataCatalogQuery
{
    private readonly ReferenceDataDbContext _db;

    public ReferenceDataCatalogQuery(ReferenceDataDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyList<CurrencyCatalogItem>> ListCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Currencies
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new CurrencyCatalogItem(x.Code, x.EnglishName, x.MinorUnits, x.Symbol))
            .ToListAsync(cancellationToken);
    }

    public async Task<CurrencyCatalogItem?> GetCurrencyAsync(string code, CancellationToken cancellationToken = default)
    {
        var parsed = CurrencyCode.Parse(code);
        var row = await _db.Currencies.AsNoTracking().FirstOrDefaultAsync(x => x.Code == parsed.Value, cancellationToken);
        return row is null ? null : new CurrencyCatalogItem(row.Code, row.EnglishName, row.MinorUnits, row.Symbol);
    }

    public async Task<IReadOnlyList<LocaleCatalogItem>> ListLocalesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Locales
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new LocaleCatalogItem(x.Code, x.EnglishName))
            .ToListAsync(cancellationToken);
    }

    public async Task<LocaleCatalogItem?> GetLocaleAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var normalized = code.Trim();
        var row = await _db.Locales.AsNoTracking().FirstOrDefaultAsync(x => x.Code == normalized, cancellationToken);
        return row is null ? null : new LocaleCatalogItem(row.Code, row.EnglishName);
    }

    public async Task<IReadOnlyList<CountryCatalogItem>> ListCountriesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Countries
            .AsNoTracking()
            .OrderBy(x => x.Alpha2Code)
            .Select(x => new CountryCatalogItem(x.Alpha2Code, x.Alpha3Code, x.NumericCode, x.EnglishName))
            .ToListAsync(cancellationToken);
    }

    public async Task<CountryCatalogItem?> GetCountryAsync(string alpha2Code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alpha2Code);
        var code = alpha2Code.Trim().ToUpperInvariant();
        var row = await _db.Countries.AsNoTracking().FirstOrDefaultAsync(x => x.Alpha2Code == code, cancellationToken);
        return row is null ? null : new CountryCatalogItem(row.Alpha2Code, row.Alpha3Code, row.NumericCode, row.EnglishName);
    }

    public async Task<IReadOnlyList<TimeZoneCatalogItem>> ListTimeZonesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.TimeZones
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new TimeZoneCatalogItem(x.Id, x.EnglishName))
            .ToListAsync(cancellationToken);
    }

    public async Task<TimeZoneCatalogItem?> GetTimeZoneAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var normalized = id.Trim();
        var row = await _db.TimeZones.AsNoTracking().FirstOrDefaultAsync(x => x.Id == normalized, cancellationToken);
        return row is null ? null : new TimeZoneCatalogItem(row.Id, row.EnglishName);
    }
}
