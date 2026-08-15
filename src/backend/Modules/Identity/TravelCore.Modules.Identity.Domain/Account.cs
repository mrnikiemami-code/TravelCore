using NodaTime;

namespace TravelCore.Modules.Identity.Domain;

/// <summary>
/// Authentication Account aggregate. Owns credentials; does not own Party profile or Access roles.
/// </summary>
public sealed class Account
{
    public const int EmailMaxLength = 320;
    public const int PasswordHashMaxLength = 512;

    private Account()
    {
        Email = null!;
        NormalizedEmail = null!;
        PasswordHash = null!;
    }

    private Account(
        AccountId id,
        string email,
        string normalizedEmail,
        string passwordHash,
        Guid? associatedPartyId,
        Instant createdAt)
    {
        Id = id;
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        AssociatedPartyId = associatedPartyId;
        Status = AccountStatus.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public AccountId Id { get; private set; }

    public string Email { get; private set; }

    public string NormalizedEmail { get; private set; }

    /// <summary>
    /// One-way password hash only. Never expose via API contracts.
    /// </summary>
    public string PasswordHash { get; private set; }

    public AccountStatus Status { get; private set; }

    /// <summary>
    /// Opaque Party reference (Identity-owned column). Not a cross-schema FK and not Party profile data.
    /// Association workflow (link/unlink/replace) is owned by T004.
    /// </summary>
    public Guid? AssociatedPartyId { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static Account Create(
        string email,
        string passwordHash,
        Instant now,
        Guid? associatedPartyId = null,
        AccountId? id = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        if (passwordHash.Length > PasswordHashMaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(passwordHash));
        }

        if (associatedPartyId == Guid.Empty)
        {
            throw new ArgumentException("AssociatedPartyId cannot be empty Guid.", nameof(associatedPartyId));
        }

        var (raw, normalized) = NormalizeEmail(email);
        return new Account(id ?? AccountId.New(), raw, normalized, passwordHash, associatedPartyId, now);
    }

    public void Disable(Instant now)
    {
        if (Status == AccountStatus.Disabled)
        {
            return;
        }

        Status = AccountStatus.Disabled;
        UpdatedAt = now;
    }

    public void Enable(Instant now)
    {
        if (Status == AccountStatus.Active)
        {
            return;
        }

        Status = AccountStatus.Active;
        UpdatedAt = now;
    }

    public void ReplacePasswordHash(string passwordHash, Instant now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        if (passwordHash.Length > PasswordHashMaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(passwordHash));
        }

        PasswordHash = passwordHash;
        UpdatedAt = now;
    }

    public void LinkAssociatedParty(Guid partyId, Instant now)
    {
        ValidatePartyId(partyId);
        if (AssociatedPartyId is not null)
        {
            throw new InvalidOperationException(
                "Account already has an associated Party. Use replace instead of link.");
        }

        AssociatedPartyId = partyId;
        UpdatedAt = now;
    }

    public void ReplaceAssociatedParty(Guid partyId, Instant now)
    {
        ValidatePartyId(partyId);
        AssociatedPartyId = partyId;
        UpdatedAt = now;
    }

    public void UnlinkAssociatedParty(Instant now)
    {
        if (AssociatedPartyId is null)
        {
            return;
        }

        AssociatedPartyId = null;
        UpdatedAt = now;
    }

    private static void ValidatePartyId(Guid partyId)
    {
        if (partyId == Guid.Empty)
        {
            throw new ArgumentException("Party id cannot be empty.", nameof(partyId));
        }
    }

    public static (string Email, string NormalizedEmail) NormalizeEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var trimmed = email.Trim();
        if (trimmed.Length > EmailMaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(email), $"Email length must be <= {EmailMaxLength}.");
        }

        if (!trimmed.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("Email must contain '@'.", nameof(email));
        }

        return (trimmed, trimmed.ToUpperInvariant());
    }
}
