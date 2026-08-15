using NodaTime;

namespace TravelCore.Modules.Party.Contracts;

/// <summary>
/// Party read contract for Admin/API consumers.
/// </summary>
public sealed class PartyResponse
{
    public required Guid Id { get; init; }

    public required string Kind { get; init; }

    public required string DisplayName { get; init; }

    public required string Status { get; init; }

    public string? PrimaryEmail { get; init; }

    public string? PrimaryPhone { get; init; }

    public required Instant CreatedAt { get; init; }

    public required Instant UpdatedAt { get; init; }

    public PersonPartyResponse? Person { get; init; }

    public OrganizationPartyResponse? Organization { get; init; }

    public AgencyPartyResponse? Agency { get; init; }
}

public sealed class PersonPartyResponse
{
    public required string GivenName { get; init; }

    public required string FamilyName { get; init; }
}

public sealed class OrganizationPartyResponse
{
    public required string LegalName { get; init; }

    public string? TradeName { get; init; }
}

public sealed class AgencyPartyResponse
{
    public required string TradingName { get; init; }

    public string? LicenseCode { get; init; }
}
