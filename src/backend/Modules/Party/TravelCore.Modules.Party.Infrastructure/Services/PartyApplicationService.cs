using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Party.Contracts;
using TravelCore.Modules.Party.Domain;
using PartyAggregate = TravelCore.Modules.Party.Domain.Party;

namespace TravelCore.Modules.Party.Infrastructure.Services;

/// <summary>
/// Party application service stubs for create/get/search (server-owned persistence).
/// </summary>
public sealed class PartyApplicationService
{
    private readonly PartyDbContext _db;
    private readonly IClock _clock;

    public PartyApplicationService(PartyDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<PartyResponse> CreateAsync(CreatePartyRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var now = _clock.GetCurrentInstant();
        var kind = ParseKind(request.Kind);
        PartyAggregate party = kind switch
        {
            PartyKind.Person => PartyAggregate.CreatePerson(
                request.DisplayName,
                Require(request.GivenName, nameof(request.GivenName)),
                Require(request.FamilyName, nameof(request.FamilyName)),
                now,
                request.PrimaryEmail,
                request.PrimaryPhone),
            PartyKind.Organization => PartyAggregate.CreateOrganization(
                request.DisplayName,
                Require(request.LegalName, nameof(request.LegalName)),
                now,
                request.TradeName,
                request.PrimaryEmail,
                request.PrimaryPhone),
            PartyKind.Agency => PartyAggregate.CreateAgency(
                request.DisplayName,
                Require(request.TradingName, nameof(request.TradingName)),
                now,
                request.LicenseCode,
                request.PrimaryEmail,
                request.PrimaryPhone),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Kind), request.Kind, "Unsupported party kind.")
        };

        _db.Parties.Add(party);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(party);
    }

    public async Task<PartyResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var partyId = PartyId.From(id);
        var party = await _db.Parties.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == partyId, cancellationToken);
        return party is null ? null : Map(party);
    }

    public async Task<SearchPartiesResponse> SearchAsync(SearchPartiesRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var take = request.Take <= 0 ? 20 : Math.Min(request.Take, 100);
        var skip = Math.Max(request.Skip, 0);

        IQueryable<PartyAggregate> query = _db.Parties.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Kind))
        {
            var kind = ParseKind(request.Kind);
            query = query.Where(x => x.Kind == kind);
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var term = request.Query.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.DisplayName, $"%{term}%")
                || (x.PrimaryEmail != null && EF.Functions.ILike(x.PrimaryEmail, $"%{term}%")));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.DisplayName)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .Select(x => new PartySummaryResponse
            {
                Id = x.Id.Value,
                Kind = x.Kind.ToString(),
                DisplayName = x.DisplayName,
                Status = x.Status.ToString(),
                PrimaryEmail = x.PrimaryEmail
            })
            .ToListAsync(cancellationToken);

        return new SearchPartiesResponse
        {
            Items = items,
            TotalCount = total
        };
    }

    private static PartyKind ParseKind(string kind)
    {
        if (Enum.TryParse<PartyKind>(kind, ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException(
            "Kind must be one of: Person, Organization, Agency.",
            nameof(kind));
    }

    private static string Require(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{name} is required for this party kind.", name);
        }

        return value;
    }

    private static PartyResponse Map(PartyAggregate party) => new()
    {
        Id = party.Id.Value,
        Kind = party.Kind.ToString(),
        DisplayName = party.DisplayName,
        Status = party.Status.ToString(),
        PrimaryEmail = party.PrimaryEmail,
        PrimaryPhone = party.PrimaryPhone,
        CreatedAt = party.CreatedAt,
        UpdatedAt = party.UpdatedAt,
        Person = party.Person is null
            ? null
            : new PersonPartyResponse
            {
                GivenName = party.Person.GivenName,
                FamilyName = party.Person.FamilyName
            },
        Organization = party.Organization is null
            ? null
            : new OrganizationPartyResponse
            {
                LegalName = party.Organization.LegalName,
                TradeName = party.Organization.TradeName
            },
        Agency = party.Agency is null
            ? null
            : new AgencyPartyResponse
            {
                TradingName = party.Agency.TradingName,
                LicenseCode = party.Agency.LicenseCode
            }
    };
}
