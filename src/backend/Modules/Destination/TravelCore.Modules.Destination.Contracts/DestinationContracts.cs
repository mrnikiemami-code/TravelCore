namespace TravelCore.Modules.Destination.Contracts;

public sealed record DestinationResponse(
    Guid Id,
    string Kind,
    string Code,
    string EnglishName,
    Guid? ParentId,
    string? IsoCountryCode);

public sealed record CreateDestinationRequest(
    string Kind,
    string Code,
    string EnglishName,
    Guid? ParentId,
    string? IsoCountryCode);

public interface IDestinationReadQuery
{
    Task<DestinationResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DestinationResponse>> ListChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);
}
