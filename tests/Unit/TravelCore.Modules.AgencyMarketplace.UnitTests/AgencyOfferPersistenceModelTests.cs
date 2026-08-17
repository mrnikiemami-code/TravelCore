using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.AgencyMarketplace.Domain;
using TravelCore.Modules.AgencyMarketplace.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Xunit;

namespace TravelCore.Modules.AgencyMarketplace.UnitTests;

/// <summary>
/// Persistence model shape for AgencyOffer (TC-P13-T003). Same-schema AgencyProfile FK; no Tour FK.
/// </summary>
public sealed class AgencyOfferPersistenceModelTests
{
    [Fact]
    public void AgencyMarketplaceModel_Maps_AgencyOffer_With_Logical_TourProduct_No_Tour_Fk()
    {
        using var db = new AgencyMarketplaceDbContext(
            new DbContextOptionsBuilder<AgencyMarketplaceDbContext>()
                .UseTravelCorePostgreSql(
                    "Host=127.0.0.1;Database=travelcore_agency_offer_model_probe;Username=x;Password=x",
                    migrationsHistorySchema: AgencyMarketplaceDbContext.SchemaName)
                .Options);

        var model = db.Model;
        var offerType = model.FindEntityType(typeof(AgencyOffer));
        Assert.NotNull(offerType);
        Assert.Equal("agency_offers", offerType.GetTableName());
        Assert.Equal(AgencyMarketplaceDbContext.SchemaName, offerType.GetSchema());

        var tourProductId = offerType.FindProperty(nameof(AgencyOffer.TourProductId));
        Assert.NotNull(tourProductId);
        Assert.Equal("tour_product_id", tourProductId.GetColumnName());

        var fk = offerType.GetForeignKeys().Single();
        Assert.Equal(typeof(AgencyProfile), fk.PrincipalEntityType.ClrType);
        Assert.Equal(AgencyMarketplaceDbContext.SchemaName, fk.PrincipalEntityType.GetSchema());

        Assert.DoesNotContain(
            model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()),
            f => string.Equals(f.PrincipalEntityType.GetSchema(), "tour", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(f.PrincipalEntityType.GetSchema(), "party", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(f.PrincipalEntityType.GetSchema(), "pricing", StringComparison.OrdinalIgnoreCase));

        Assert.False(db.Database.HasPendingModelChanges());
    }
}
