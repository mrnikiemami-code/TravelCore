namespace TravelCore.Modules.ReferenceData.Contracts;

public sealed record CurrencyCatalogItem(string Code, string EnglishName, int MinorUnits, string? Symbol);

public sealed record LocaleCatalogItem(string Code, string EnglishName);

public sealed record CountryCatalogItem(string Alpha2Code, string Alpha3Code, string? NumericCode, string EnglishName);

public sealed record TimeZoneCatalogItem(string Id, string EnglishName);

public interface IReferenceDataCatalogQuery
{
    Task<IReadOnlyList<CurrencyCatalogItem>> ListCurrenciesAsync(CancellationToken cancellationToken = default);

    Task<CurrencyCatalogItem?> GetCurrencyAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LocaleCatalogItem>> ListLocalesAsync(CancellationToken cancellationToken = default);

    Task<LocaleCatalogItem?> GetLocaleAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CountryCatalogItem>> ListCountriesAsync(CancellationToken cancellationToken = default);

    Task<CountryCatalogItem?> GetCountryAsync(string alpha2Code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimeZoneCatalogItem>> ListTimeZonesAsync(CancellationToken cancellationToken = default);

    Task<TimeZoneCatalogItem?> GetTimeZoneAsync(string id, CancellationToken cancellationToken = default);
}
