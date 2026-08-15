using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Identity.Domain;

namespace TravelCore.Modules.Identity.Infrastructure.Services;

public sealed class AccountAssociationQuery : IAccountAssociationQuery
{
    private readonly IdentityDbContext _db;

    public AccountAssociationQuery(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<Guid?> GetAssociatedPartyIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
        {
            return null;
        }

        var id = AccountId.From(accountId);
        var account = await _db.Accounts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return account?.AssociatedPartyId;
    }
}
