using NodaTime;

namespace TravelCore.Modules.Party.Domain;

/// <summary>
/// Party aggregate root — business who (Person / Organization / Agency).
/// Does not own credentials or Access roles.
/// </summary>
public sealed class Party
{
    public const int DisplayNameMaxLength = 200;
    public const int ContactMaxLength = 256;

    private Party()
    {
        DisplayName = null!;
    }

    private Party(
        PartyId id,
        PartyKind kind,
        string displayName,
        string? primaryEmail,
        string? primaryPhone,
        Instant createdAt)
    {
        Id = id;
        Kind = kind;
        DisplayName = NormalizeDisplayName(displayName);
        PrimaryEmail = NormalizeOptionalContact(primaryEmail, nameof(primaryEmail));
        PrimaryPhone = NormalizeOptionalContact(primaryPhone, nameof(primaryPhone));
        Status = PartyStatus.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public PartyId Id { get; private set; }

    public PartyKind Kind { get; private set; }

    public string DisplayName { get; private set; }

    /// <summary>
    /// Opaque contact essential until ReferenceData catalogs exist (P04).
    /// </summary>
    public string? PrimaryEmail { get; private set; }

    /// <summary>
    /// Opaque contact essential until ReferenceData catalogs exist (P04).
    /// </summary>
    public string? PrimaryPhone { get; private set; }

    public PartyStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public PersonParty? Person { get; private set; }

    public OrganizationParty? Organization { get; private set; }

    public AgencyParty? Agency { get; private set; }

    public static Party CreatePerson(
        string displayName,
        string givenName,
        string familyName,
        Instant now,
        string? primaryEmail = null,
        string? primaryPhone = null,
        PartyId? id = null)
    {
        var partyId = id ?? PartyId.New();
        var party = new Party(partyId, PartyKind.Person, displayName, primaryEmail, primaryPhone, now)
        {
            Person = new PersonParty(partyId, givenName, familyName)
        };
        return party;
    }

    public static Party CreateOrganization(
        string displayName,
        string legalName,
        Instant now,
        string? tradeName = null,
        string? primaryEmail = null,
        string? primaryPhone = null,
        PartyId? id = null)
    {
        var partyId = id ?? PartyId.New();
        var party = new Party(partyId, PartyKind.Organization, displayName, primaryEmail, primaryPhone, now)
        {
            Organization = new OrganizationParty(partyId, legalName, tradeName)
        };
        return party;
    }

    public static Party CreateAgency(
        string displayName,
        string tradingName,
        Instant now,
        string? licenseCode = null,
        string? primaryEmail = null,
        string? primaryPhone = null,
        PartyId? id = null)
    {
        var partyId = id ?? PartyId.New();
        var party = new Party(partyId, PartyKind.Agency, displayName, primaryEmail, primaryPhone, now)
        {
            Agency = new AgencyParty(partyId, tradingName, licenseCode)
        };
        return party;
    }

    public void Deactivate(Instant now)
    {
        if (Status == PartyStatus.Inactive)
        {
            return;
        }

        Status = PartyStatus.Inactive;
        Touch(now);
    }

    public void Activate(Instant now)
    {
        if (Status == PartyStatus.Active)
        {
            return;
        }

        Status = PartyStatus.Active;
        Touch(now);
    }

    public void Rename(string displayName, Instant now)
    {
        DisplayName = NormalizeDisplayName(displayName);
        Touch(now);
    }

    private void Touch(Instant now) => UpdatedAt = now;

    private static string NormalizeDisplayName(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        var trimmed = displayName.Trim();
        if (trimmed.Length > DisplayNameMaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(displayName),
                $"Display name length must be <= {DisplayNameMaxLength}.");
        }

        return trimmed;
    }

    private static string? NormalizeOptionalContact(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > ContactMaxLength)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Length must be <= {ContactMaxLength}.");
        }

        return trimmed;
    }
}
