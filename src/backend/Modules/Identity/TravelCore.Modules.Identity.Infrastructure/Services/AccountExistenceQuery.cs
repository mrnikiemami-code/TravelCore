using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Identity.Domain;

namespace TravelCore.Modules.Identity.Infrastructure.Services;

public sealed class AccountExistenceQuery : IAccountExistenceQuery
{
    private readonly IdentityDbContext _db;

    public AccountExistenceQuery(IdentityDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
        {
            return Task.FromResult(false);
        }

        var id = AccountId.From(accountId);
        return _db.Accounts.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken);
    }
}
