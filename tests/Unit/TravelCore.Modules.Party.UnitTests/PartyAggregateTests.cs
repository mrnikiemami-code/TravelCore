using NodaTime;
using TravelCore.Modules.Party.Domain;
using PartyAggregate = TravelCore.Modules.Party.Domain.Party;
using Xunit;

namespace TravelCore.Modules.Party.UnitTests;

public sealed class PartyAggregateTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 15, 12, 0);

    [Fact]
    public void CreatePerson_AssignsUuidV7_AndPersonSpecialization()
    {
        var party = PartyAggregate.CreatePerson("Ada Lovelace", "Ada", "Lovelace", Now, "ada@example.com", "+1000");

        Assert.NotEqual(Guid.Empty, party.Id.Value);
        Assert.Equal(7, party.Id.Value.Version);
        Assert.Equal(PartyKind.Person, party.Kind);
        Assert.Equal(PartyStatus.Active, party.Status);
        Assert.Equal("Ada Lovelace", party.DisplayName);
        Assert.NotNull(party.Person);
        Assert.Equal("Ada", party.Person!.GivenName);
        Assert.Null(party.Organization);
        Assert.Null(party.Agency);
    }

    [Fact]
    public void CreateOrganization_RequiresLegalName()
    {
        var party = PartyAggregate.CreateOrganization("Acme", "Acme Legal Ltd", Now, tradeName: "Acme");

        Assert.Equal(PartyKind.Organization, party.Kind);
        Assert.Equal("Acme Legal Ltd", party.Organization!.LegalName);
        Assert.Equal("Acme", party.Organization.TradeName);
        Assert.Null(party.Person);
        Assert.Null(party.Agency);
    }

    [Fact]
    public void CreateAgency_IsBusinessIdentity_NotAuthSilo()
    {
        var party = PartyAggregate.CreateAgency("Sky Travel", "Sky Travel Agency", Now, licenseCode: "LIC-1");

        Assert.Equal(PartyKind.Agency, party.Kind);
        Assert.Equal("Sky Travel Agency", party.Agency!.TradingName);
        Assert.Equal("LIC-1", party.Agency.LicenseCode);
        Assert.Null(party.Person);
        Assert.Null(party.Organization);
    }

    [Fact]
    public void CreatePerson_RejectsBlankDisplayName()
    {
        Assert.Throws<ArgumentException>(() =>
            PartyAggregate.CreatePerson("  ", "Ada", "Lovelace", Now));
    }

    [Fact]
    public void Deactivate_SetsInactiveLifecycle()
    {
        var party = PartyAggregate.CreatePerson("Ada Lovelace", "Ada", "Lovelace", Now);
        var later = Now.Plus(Duration.FromMinutes(5));

        party.Deactivate(later);

        Assert.Equal(PartyStatus.Inactive, party.Status);
        Assert.Equal(later, party.UpdatedAt);
    }

    [Fact]
    public void PartyId_RejectsEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() => PartyId.From(Guid.Empty));
    }
}
