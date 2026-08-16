using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Party.Contracts;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Services;

/// <summary>
/// TourProduct classification / Origin / Destination / Agency link mutations with Contracts validation.
/// </summary>
public sealed class TourProductSemanticLinkService : ITourProductSemanticLinkService
{
    private readonly TourDbContext _db;
    private readonly IDestinationExistenceQuery _destinations;
    private readonly IPartyReadQuery _parties;
    private readonly IClock _clock;

    public TourProductSemanticLinkService(
        TourDbContext db,
        IDestinationExistenceQuery destinations,
        IPartyReadQuery parties,
        IClock clock)
    {
        _db = db;
        _destinations = destinations;
        _parties = parties;
        _clock = clock;
    }

    public async Task<TourProductSemanticLinksResponse?> GetAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default)
    {
        var product = await FindAsync(tourProductId, cancellationToken);
        return product is null ? null : Map(product);
    }

    public async Task<TourProductSemanticLinksResponse> SetClassificationAsync(
        Guid tourProductId,
        SetTourClassificationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.SetClassificationCode(request.ClassificationCode, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<TourProductSemanticLinksResponse> SetOriginAsync(
        Guid tourProductId,
        SetTourOriginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.OriginDestinationId is Guid originId)
        {
            await EnsureDestinationExistsAsync(originId, cancellationToken);
        }

        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.SetOriginLink(request.OriginDestinationId, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<TourProductSemanticLinksResponse> SetAgencyAsync(
        Guid tourProductId,
        SetTourAgencyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.AgencyId is Guid agencyId)
        {
            await EnsureAgencyExistsAsync(agencyId, cancellationToken);
        }

        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.SetAgencyLink(request.AgencyId, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<TourProductSemanticLinksResponse> AssignDestinationAsync(
        Guid tourProductId,
        Guid destinationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureDestinationExistsAsync(destinationId, cancellationToken);
        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.AssignDestination(destinationId, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<TourProductSemanticLinksResponse> RemoveDestinationAsync(
        Guid tourProductId,
        Guid destinationId,
        CancellationToken cancellationToken = default)
    {
        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.RemoveDestination(destinationId, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    private async Task EnsureDestinationExistsAsync(
        Guid destinationId,
        CancellationToken cancellationToken)
    {
        if (destinationId == Guid.Empty)
        {
            throw new ArgumentException("DestinationId cannot be empty.", nameof(destinationId));
        }

        if (!await _destinations.ExistsAsync(destinationId, cancellationToken))
        {
            throw new ArgumentException(
                $"Destination '{destinationId}' was not found.",
                nameof(destinationId));
        }
    }

    private async Task EnsureAgencyExistsAsync(
        Guid agencyId,
        CancellationToken cancellationToken)
    {
        if (agencyId == Guid.Empty)
        {
            throw new ArgumentException("AgencyId cannot be empty.", nameof(agencyId));
        }

        // Agency is PartyKind.Agency under Party module (no separate Agency.Contracts assembly).
        var party = await _parties.GetAsync(agencyId, cancellationToken);
        if (party is null)
        {
            throw new ArgumentException($"Agency '{agencyId}' was not found.", nameof(agencyId));
        }

        if (!string.Equals(party.Kind, "Agency", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Party '{agencyId}' kind '{party.Kind}' is not Agency.",
                nameof(agencyId));
        }
    }

    private async Task<TourProduct?> FindAsync(Guid tourProductId, CancellationToken cancellationToken)
    {
        var id = TourProductId.From(tourProductId);
        return await _db.TourProducts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    private async Task<TourProduct> LoadTrackedAsync(Guid tourProductId, CancellationToken cancellationToken)
    {
        return await FindAsync(tourProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"TourProduct '{tourProductId}' was not found.");
    }

    private static TourProductSemanticLinksResponse Map(TourProduct product) =>
        new(
            product.Id.Value,
            product.Code,
            product.ClassificationCode,
            product.OriginDestinationId,
            product.AgencyId,
            product.Destinations
                .Select(x => x.DestinationId)
                .OrderBy(x => x)
                .ToArray());
}
