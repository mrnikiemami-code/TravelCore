namespace TravelCore.Modules.Destination.Contracts;

public sealed record DestinationResponse(
    Guid Id,
    string Kind,
    string Code,
    string EnglishName,
    Guid? ParentId,
    string? IsoCountryCode,
    decimal? Latitude,
    decimal? Longitude,
    string? LocalizedName = null,
    string? LocalizedDescription = null,
    string? Locale = null);

public sealed record CreateDestinationRequest(
    string Kind,
    string Code,
    string EnglishName,
    Guid? ParentId,
    string? IsoCountryCode);

public sealed record UpsertDestinationTranslationRequest(
    string Name,
    string? Description);

public sealed record DestinationTranslationResponse(
    Guid DestinationId,
    string LocaleCode,
    string Name,
    string? Description);

public sealed record SetDestinationGeoRequest(
    decimal? Latitude,
    decimal? Longitude);

public interface IDestinationReadQuery
{
    Task<DestinationResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DestinationResponse?> GetByIdAsync(Guid id, string? locale, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DestinationResponse>> ListChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DestinationTranslationResponse>> ListTranslationsAsync(
        Guid destinationId,
        CancellationToken cancellationToken = default);
}
