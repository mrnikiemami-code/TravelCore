using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Identity.Domain;
using TravelCore.Modules.Identity.Infrastructure.Security;
using TravelCore.Modules.Party.Contracts;
using AccountAggregate = TravelCore.Modules.Identity.Domain.Account;

namespace TravelCore.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Identity application service: account create/status + association + credential authentication.
/// Ticket issuance is owned by login endpoints (cookie transport; R1).
/// </summary>
public sealed class IdentityApplicationService
{
    private readonly IdentityDbContext _db;
    private readonly IIdentityPasswordHasher _passwordHasher;
    private readonly IPartyExistenceQuery _partyExistence;
    private readonly IClock _clock;

    public IdentityApplicationService(
        IdentityDbContext db,
        IIdentityPasswordHasher passwordHasher,
        IPartyExistenceQuery partyExistence,
        IClock clock)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _partyExistence = partyExistence;
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

        if (request.AssociatedPartyId is Guid partyId)
        {
            await EnsurePartyExistsAsync(partyId, cancellationToken);
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

    public async Task<AccountStatusResponse> LinkPartyAsync(
        Guid accountId,
        Guid partyId,
        CancellationToken cancellationToken)
    {
        await EnsurePartyExistsAsync(partyId, cancellationToken);
        var account = await LoadTrackedAsync(accountId, cancellationToken);
        account.LinkAssociatedParty(partyId, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(account);
    }

    public async Task<AccountStatusResponse> ReplacePartyAsync(
        Guid accountId,
        Guid partyId,
        CancellationToken cancellationToken)
    {
        await EnsurePartyExistsAsync(partyId, cancellationToken);
        var account = await LoadTrackedAsync(accountId, cancellationToken);
        account.ReplaceAssociatedParty(partyId, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(account);
    }

    public async Task<AccountStatusResponse> UnlinkPartyAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var account = await LoadTrackedAsync(accountId, cancellationToken);
        account.UnlinkAssociatedParty(_clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(account);
    }

    /// <summary>
    /// Verifies credentials for an Active account. Returns principal projection or null (no existence leakage).
    /// Does not issue tickets — callers use cookie SignIn.
    /// </summary>
    public async Task<AuthenticatedPrincipalResponse?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var (_, normalized) = AccountAggregate.NormalizeEmail(email);
        var account = await _db.Accounts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalized, cancellationToken);

        if (account is null || account.Status != AccountStatus.Active)
        {
            return null;
        }

        if (!_passwordHasher.VerifyHashedPassword(account.PasswordHash, password))
        {
            return null;
        }

        return new AuthenticatedPrincipalResponse
        {
            AccountId = account.Id.Value,
            Email = account.Email,
            Status = account.Status.ToString(),
            AssociatedPartyId = account.AssociatedPartyId,
            AuthenticatedAt = _clock.GetCurrentInstant()
        };
    }

    /// <summary>
    /// Credential verification hook. Prefer <see cref="AuthenticateAsync"/> for login flows.
    /// </summary>
    public async Task<bool> VerifyCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        return await AuthenticateAsync(email, password, cancellationToken) is not null;
    }

    private async Task EnsurePartyExistsAsync(Guid partyId, CancellationToken cancellationToken)
    {
        if (!await _partyExistence.ExistsAsync(partyId, cancellationToken))
        {
            throw new InvalidOperationException("Party does not exist.");
        }
    }

    private async Task<AccountAggregate> LoadTrackedAsync(Guid id, CancellationToken cancellationToken)
    {
        var accountId = AccountId.From(id);
        var account = await _db.Accounts.FirstOrDefaultAsync(x => x.Id == accountId, cancellationToken);
        if (account is null)
        {
            throw new KeyNotFoundException("Account was not found.");
        }

        return account;
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
