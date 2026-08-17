using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.AgencyMarketplace.Domain;
using TravelCore.Modules.AgencyMarketplace.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Xunit;

namespace TravelCore.Modules.AgencyMarketplace.UnitTests;

/// <summary>
/// Persistence model shape for AgencyProfile (TC-P13-T002). No Party/Tour/Pricing FK.
/// </summary>
public sealed class AgencyProfilePersistenceModelTests
{
    [Fact]
    public void AgencyMarketplaceModel_Maps_AgencyProfile_With_Owned_Settings_No_Peer_Fk()
    {
        using var db = new AgencyMarketplaceDbContext(
            new DbContextOptionsBuilder<AgencyMarketplaceDbContext>()
                .UseTravelCorePostgreSql(
                    "Host=127.0.0.1;Database=travelcore_agency_marketplace_model_probe;Username=x;Password=x",
                    migrationsHistorySchema: AgencyMarketplaceDbContext.SchemaName)
                .Options);

        var model = db.Model;
        var profileType = model.FindEntityType(typeof(AgencyProfile));
        Assert.NotNull(profileType);
        Assert.Equal("agency_profiles", profileType.GetTableName());
        Assert.Equal(AgencyMarketplaceDbContext.SchemaName, profileType.GetSchema());

        var partyId = profileType.FindProperty(nameof(AgencyProfile.PartyId));
        Assert.NotNull(partyId);
        Assert.Equal("party_id", partyId.GetColumnName());

        var uniqueParty = profileType.GetIndexes().Single(i => i.IsUnique);
        Assert.Equal("ux_agency_profiles_party_id", uniqueParty.GetDatabaseName());

        Assert.DoesNotContain(
            model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()),
            f => string.Equals(f.PrincipalEntityType.GetSchema(), "party", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(f.PrincipalEntityType.GetSchema(), "tour", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(f.PrincipalEntityType.GetSchema(), "pricing", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(f.PrincipalEntityType.GetSchema(), "media", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(model.FindEntityType(typeof(AgencyOffer)));
        Assert.False(db.Database.HasPendingModelChanges());
    }
}
