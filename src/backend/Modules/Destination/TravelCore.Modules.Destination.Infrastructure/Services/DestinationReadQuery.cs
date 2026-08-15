using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Destination.Domain;

namespace TravelCore.Modules.Destination.Infrastructure.Services;

public sealed class DestinationReadQuery : IDestinationReadQuery
{
    private readonly DestinationDbContext _db;

    public DestinationReadQuery(DestinationDbContext db)
    {
        _db = db;
    }

    public async Task<DestinationResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var destinationId = DestinationId.From(id);
        var destination = await _db.Destinations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == destinationId, cancellationToken);
        return destination is null ? null : Map(destination);
    }

    public async Task<IReadOnlyList<DestinationResponse>> ListChildrenAsync(
        Guid parentId,
        CancellationToken cancellationToken = default)
    {
        var id = DestinationId.From(parentId);
        var children = await _db.Destinations.AsNoTracking()
            .Where(x => x.ParentId == id)
            .OrderBy(x => x.EnglishName)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return children.Select(Map).ToList();
    }

    private static DestinationResponse Map(Domain.Destination destination) =>
        new(
            destination.Id.Value,
            destination.Kind.ToString(),
            destination.Code,
            destination.EnglishName,
            destination.ParentId?.Value,
            destination.IsoCountryCode);
}
