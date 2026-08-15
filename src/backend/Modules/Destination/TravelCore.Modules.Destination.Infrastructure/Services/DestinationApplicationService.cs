using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Destination.Domain;
using TravelCore.Modules.ReferenceData.Contracts;
using DestinationAggregate = TravelCore.Modules.Destination.Domain.Destination;

namespace TravelCore.Modules.Destination.Infrastructure.Services;

/// <summary>
/// Destination application service for create/get/children (server-owned persistence).
/// </summary>
public sealed class DestinationApplicationService
{
    private readonly DestinationDbContext _db;
    private readonly IClock _clock;
    private readonly IReferenceDataCatalogQuery _referenceData;

    public DestinationApplicationService(
        DestinationDbContext db,
        IClock clock,
        IReferenceDataCatalogQuery referenceData)
    {
        _db = db;
        _clock = clock;
        _referenceData = referenceData;
    }

    public async Task<DestinationResponse> CreateAsync(
        CreateDestinationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var kind = ParseKind(request.Kind);
        DestinationId? parentId = request.ParentId is null
            ? null
            : DestinationId.From(request.ParentId.Value);

        DestinationAggregate? parent = null;
        if (parentId is not null)
        {
            parent = await _db.Destinations
                .FirstOrDefaultAsync(x => x.Id == parentId.Value, cancellationToken)
                ?? throw new ArgumentException("Parent destination was not found.", nameof(request.ParentId));
        }

        if (kind == DestinationKind.Country)
        {
            var iso = request.IsoCountryCode?.Trim();
            if (string.IsNullOrWhiteSpace(iso))
            {
                throw new ArgumentException(
                    "Country destinations require IsoCountryCode (ReferenceData alpha-2).",
                    nameof(request.IsoCountryCode));
            }

            var country = await _referenceData.GetCountryAsync(iso, cancellationToken);
            if (country is null)
            {
                throw new ArgumentException(
                    $"IsoCountryCode '{iso}' was not found in ReferenceData country catalog.",
                    nameof(request.IsoCountryCode));
            }
        }

        var now = _clock.GetCurrentInstant();
        var destination = DestinationAggregate.Create(
            kind,
            request.Code,
            request.EnglishName,
            now,
            parentId,
            request.IsoCountryCode,
            parent);

        _db.Destinations.Add(destination);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(destination);
    }

    public async Task<DestinationResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var destinationId = DestinationId.From(id);
        var destination = await _db.Destinations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == destinationId, cancellationToken);
        return destination is null ? null : Map(destination);
    }

    public async Task<IReadOnlyList<DestinationResponse>> ListChildrenAsync(
        Guid parentId,
        CancellationToken cancellationToken)
    {
        var id = DestinationId.From(parentId);
        var children = await _db.Destinations.AsNoTracking()
            .Where(x => x.ParentId == id)
            .OrderBy(x => x.EnglishName)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return children.Select(Map).ToList();
    }

    private static DestinationKind ParseKind(string kind)
    {
        if (Enum.TryParse<DestinationKind>(kind, ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException(
            "Kind must be one of: Country, Region, City, Area.",
            nameof(kind));
    }

    private static DestinationResponse Map(DestinationAggregate destination) =>
        new(
            destination.Id.Value,
            destination.Kind.ToString(),
            destination.Code,
            destination.EnglishName,
            destination.ParentId?.Value,
            destination.IsoCountryCode);
}
