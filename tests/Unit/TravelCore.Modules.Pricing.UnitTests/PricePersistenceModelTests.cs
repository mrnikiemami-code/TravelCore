using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Pricing.Domain;
using TravelCore.Modules.Pricing.Infrastructure;
using TravelCore.Modules.Pricing.Infrastructure.Persistence;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;
using Xunit;

namespace TravelCore.Modules.Pricing.UnitTests;

/// <summary>
/// Persistence model shape for Price + PriceComponent (TC-P12-T003).
/// </summary>
public sealed class PricePersistenceModelTests
{
    [Fact]
    public void PricingModel_Maps_Prices_And_Components_With_Owned_Money_No_Tour_Fk()
    {
        using var db = new PricingDbContext(
            new DbContextOptionsBuilder<PricingDbContext>()
                .UseNpgsql(
                    "Host=127.0.0.1;Database=travelcore_pricing_model_probe;Username=x;Password=x",
                    npgsql => npgsql.UseNodaTime())
                .Options);

        var model = db.Model;
        var priceType = model.FindEntityType(typeof(Price));
        var componentType = model.FindEntityType(typeof(PriceComponent));
        var occupancyRuleType = model.FindEntityType(typeof(PriceOccupancyRule));
        Assert.NotNull(priceType);
        Assert.NotNull(componentType);
        Assert.NotNull(occupancyRuleType);

        Assert.Equal("prices", priceType.GetTableName());
        Assert.Equal(PricingDbContext.SchemaName, priceType.GetSchema());
        Assert.Equal("price_components", componentType.GetTableName());
        Assert.Equal(PricingDbContext.SchemaName, componentType.GetSchema());
        Assert.Equal("price_occupancy_rules", occupancyRuleType.GetTableName());
        Assert.Equal(PricingDbContext.SchemaName, occupancyRuleType.GetSchema());

        var targetType = priceType.FindProperty(nameof(Price.TargetType));
        var targetId = priceType.FindProperty(nameof(Price.TargetId));
        Assert.NotNull(targetType);
        Assert.NotNull(targetId);
        Assert.Equal("target_type", targetType.GetColumnName());
        Assert.Equal("target_id", targetId.GetColumnName());

        var moneyNav = componentType.FindNavigation(nameof(PriceComponent.Money));
        Assert.NotNull(moneyNav);
        Assert.False(moneyNav.IsCollection);
        Assert.True(moneyNav.ForeignKey.IsRequired);

        var moneyType = moneyNav.TargetEntityType;
        var amount = moneyType.FindProperty(nameof(MoneyValue.Amount));
        var currency = moneyType.FindProperty(nameof(MoneyValue.Currency));
        Assert.NotNull(amount);
        Assert.NotNull(currency);
        Assert.Equal("amount", amount.GetColumnName());
        Assert.Equal(MoneyOwnedMapping.DefaultAmountColumnType, amount.GetColumnType());
        Assert.Equal("currency_code", currency.GetColumnName());

        // FK must stay inside pricing schema — never tour.
        var fk = componentType.GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(Price));
        Assert.Equal(PricingDbContext.SchemaName, fk.PrincipalEntityType.GetSchema());
        var occupancyFk = occupancyRuleType.GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(Price));
        Assert.Equal(PricingDbContext.SchemaName, occupancyFk.PrincipalEntityType.GetSchema());

        var marketPriceType = occupancyRuleType.FindProperty(nameof(PriceOccupancyRule.MarketPriceType));
        var passengerCategory = occupancyRuleType.FindProperty(nameof(PriceOccupancyRule.PassengerCategory));
        var occupancyCategory = occupancyRuleType.FindProperty(nameof(PriceOccupancyRule.OccupancyCategory));
        Assert.NotNull(marketPriceType);
        Assert.NotNull(passengerCategory);
        Assert.NotNull(occupancyCategory);
        Assert.Equal("market_price_type", marketPriceType.GetColumnName());
        Assert.Equal("passenger_category", passengerCategory.GetColumnName());
        Assert.Equal("occupancy_category", occupancyCategory.GetColumnName());

        Assert.DoesNotContain(
            model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()),
            f => string.Equals(f.PrincipalEntityType.GetSchema(), "tour", StringComparison.OrdinalIgnoreCase)
                 || (f.PrincipalEntityType.ClrType.Namespace ?? string.Empty)
                     .Contains(".Tour.", StringComparison.Ordinal));
    }
}
