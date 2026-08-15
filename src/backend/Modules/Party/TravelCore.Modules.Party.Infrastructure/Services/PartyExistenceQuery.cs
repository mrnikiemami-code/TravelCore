using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Party.Contracts;
using TravelCore.Modules.Party.Domain;

namespace TravelCore.Modules.Party.Infrastructure.Services;

/// <summary>
/// Party-owned existence query used by Identity association commands.
/// </summary>
public sealed class PartyExistenceQuery : IPartyExistenceQuery
{
    private readonly PartyDbContext _db;

    public PartyExistenceQuery(PartyDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        if (partyId == Guid.Empty)
        {
            return Task.FromResult(false);
        }

        var id = PartyId.From(partyId);
        return _db.Parties.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken);
    }
}
