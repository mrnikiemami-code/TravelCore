using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Destination.Domain;

namespace TravelCore.Modules.Destination.Infrastructure.Services;

/// <summary>
/// Destination-owned existence query used by Place DestinationId association (P07-R2).
/// </summary>
public sealed class DestinationExistenceQuery : IDestinationExistenceQuery
{
    private readonly DestinationDbContext _db;

    public DestinationExistenceQuery(DestinationDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsAsync(Guid destinationId, CancellationToken cancellationToken = default)
    {
        if (destinationId == Guid.Empty)
        {
            return Task.FromResult(false);
        }

        var id = DestinationId.From(destinationId);
        return _db.Destinations.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken);
    }
}
