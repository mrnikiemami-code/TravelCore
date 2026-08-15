using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Party.Contracts;
using TravelCore.Modules.Party.Domain;

namespace TravelCore.Modules.Party.Infrastructure.Services;

public sealed class PartyReadQuery : IPartyReadQuery
{
    private readonly PartyDbContext _db;

    public PartyReadQuery(PartyDbContext db)
    {
        _db = db;
    }

    public async Task<PartyReadInfo?> GetAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        if (partyId == Guid.Empty)
        {
            return null;
        }

        var id = PartyId.From(partyId);
        var party = await _db.Parties.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (party is null)
        {
            return null;
        }

        return new PartyReadInfo
        {
            Id = party.Id.Value,
            Kind = party.Kind.ToString(),
            DisplayName = party.DisplayName
        };
    }
}
