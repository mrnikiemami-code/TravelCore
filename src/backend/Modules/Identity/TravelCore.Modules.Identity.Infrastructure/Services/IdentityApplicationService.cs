using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Identity.Domain;
using TravelCore.Modules.Identity.Infrastructure.Security;
using AccountAggregate = TravelCore.Modules.Identity.Domain.Account;

namespace TravelCore.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Identity application service: account create/status (+ internal credential verify). No session/ticket issuance.
/// </summary>
public sealed class IdentityApplicationService
{
    private readonly IdentityDbContext _db;
    private readonly IIdentityPasswordHasher _passwordHasher;
    private readonly IClock _clock;

    public IdentityApplicationService(
        IdentityDbContext db,
        IIdentityPasswordHasher passwordHasher,
        IClock clock)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task<AccountStatusResponse> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (_, normalized) = AccountAggregate.NormalizeEmail(request.Email);
        var exists = await _db.Accounts.AsNoTracking()
            .AnyAsync(x => x.NormalizedEmail == normalized, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("An account with this email already exists.");
        }

        var hash = _passwordHasher.HashPassword(request.Password);
        var now = _clock.GetCurrentInstant();
        var account = AccountAggregate.Create(
            request.Email,
            hash,
            now,
            request.AssociatedPartyId);

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(account);
    }

    public async Task<AccountStatusResponse?> GetStatusByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var accountId = AccountId.From(id);
        var account = await _db.Accounts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == accountId, cancellationToken);
        return account is null ? null : Map(account);
    }

    /// <summary>
    /// Credential verification hook for later auth flows. Does not issue tickets (R1 deferred).
    /// </summary>
    public async Task<bool> VerifyCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        var (_, normalized) = AccountAggregate.NormalizeEmail(email);
        var account = await _db.Accounts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalized, cancellationToken);

        if (account is null || account.Status != AccountStatus.Active)
        {
            return false;
        }

        return _passwordHasher.VerifyHashedPassword(account.PasswordHash, password);
    }

    private static AccountStatusResponse Map(AccountAggregate account) => new()
    {
        Id = account.Id.Value,
        Email = account.Email,
        Status = account.Status.ToString(),
        AssociatedPartyId = account.AssociatedPartyId,
        CreatedAt = account.CreatedAt,
        UpdatedAt = account.UpdatedAt
    };
}
