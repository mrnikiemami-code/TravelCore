using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Pricing.Domain;
using TravelCore.Modules.Pricing.Infrastructure;
using TravelCore.Modules.Pricing.Infrastructure.Persistence;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;
using Xunit;

namespace TravelCore.Modules.Pricing.UnitTests;

/// <summary>
/// Persistence model shape for Quote + PriceSnapshot (TC-P12-T004) and requested display currency (TC-P12-T007).
/// </summary>
public sealed class QuotePersistenceModelTests
{
    [Fact]
    public void PricingModel_Maps_Quotes_And_Snapshot_Components_With_Owned_Money_No_Peer_Fk()
    {
        using var db = new PricingDbContext(
            new DbContextOptionsBuilder<PricingDbContext>()
                .UseNpgsql(
                    "Host=127.0.0.1;Database=travelcore_pricing_quote_model_probe;Username=x;Password=x",
                    npgsql => npgsql.UseNodaTime())
                .Options);

        var model = db.Model;
        var quoteType = model.FindEntityType(typeof(Quote));
        var snapshotType = model.FindEntityType(typeof(QuoteSnapshotComponent));
        Assert.NotNull(quoteType);
        Assert.NotNull(snapshotType);

        Assert.Equal("quotes", quoteType.GetTableName());
        Assert.Equal(PricingDbContext.SchemaName, quoteType.GetSchema());
        Assert.Equal("quote_snapshot_components", snapshotType.GetTableName());
        Assert.Equal(PricingDbContext.SchemaName, snapshotType.GetSchema());

        Assert.NotNull(quoteType.FindProperty(nameof(Quote.SourcePriceId)));
        Assert.NotNull(quoteType.FindProperty(nameof(Quote.ExpiresAt)));
        Assert.NotNull(quoteType.FindProperty(nameof(Quote.CreatedAt)));
        Assert.NotNull(quoteType.FindProperty(nameof(Quote.SnapshotTargetType)));
        Assert.NotNull(quoteType.FindProperty(nameof(Quote.SnapshotTargetId)));

        var displayCurrency = quoteType.FindProperty(nameof(Quote.RequestedDisplayCurrency));
        Assert.NotNull(displayCurrency);
        Assert.True(displayCurrency.IsNullable);
        Assert.Equal("requested_display_currency", displayCurrency.GetColumnName());
        Assert.Null(quoteType.FindNavigation("RequestedDisplayMoney"));
        Assert.Null(quoteType.FindProperty("ConvertedAmount"));
        Assert.Null(quoteType.FindProperty("DisplayAmount"));
        Assert.Null(quoteType.FindProperty("ExchangeRate"));

        // SourcePriceId is logical provenance — must not FK to prices (snapshot independence).
        Assert.DoesNotContain(
            quoteType.GetForeignKeys(),
            f => f.PrincipalEntityType.ClrType == typeof(Price));

        var moneyNav = snapshotType.FindNavigation(nameof(QuoteSnapshotComponent.Money));
        Assert.NotNull(moneyNav);
        Assert.True(moneyNav.ForeignKey.IsRequired);

        var moneyType = moneyNav.TargetEntityType;
        Assert.Equal("amount", moneyType.FindProperty(nameof(MoneyValue.Amount))!.GetColumnName());
        Assert.Equal(MoneyOwnedMapping.DefaultAmountColumnType, moneyType.FindProperty(nameof(MoneyValue.Amount))!.GetColumnType());
        Assert.Equal("currency_code", moneyType.FindProperty(nameof(MoneyValue.Currency))!.GetColumnName());

        var fk = snapshotType.GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(Quote));
        Assert.Equal(PricingDbContext.SchemaName, fk.PrincipalEntityType.GetSchema());

        Assert.DoesNotContain(
            model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()),
            f => string.Equals(f.PrincipalEntityType.GetSchema(), "tour", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(f.PrincipalEntityType.GetSchema(), "booking", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(f.PrincipalEntityType.GetSchema(), "payment", StringComparison.OrdinalIgnoreCase)
                 || (f.PrincipalEntityType.ClrType.Namespace ?? string.Empty)
                     .Contains(".Tour.", StringComparison.Ordinal)
                 || (f.PrincipalEntityType.ClrType.Namespace ?? string.Empty)
                     .Contains(".Booking.", StringComparison.Ordinal)
                 || (f.PrincipalEntityType.ClrType.Namespace ?? string.Empty)
                     .Contains(".Payment.", StringComparison.Ordinal));

        Assert.Null(quoteType.FindProperty("CustomerId"));
        Assert.Null(quoteType.FindProperty("PassengerId"));
        Assert.Null(quoteType.FindProperty("PaymentId"));
        Assert.Null(quoteType.FindProperty("BookingId"));
        Assert.Null(quoteType.FindProperty("SettlementId"));

        Assert.DoesNotContain(
            model.GetEntityTypes(),
            e => e.GetTableName() is "exchange_rates" or "fx_rates" or "payments" or "settlements"
                 || string.Equals(e.ClrType.Name, "ExchangeRate", StringComparison.Ordinal)
                 || string.Equals(e.ClrType.Name, "FxRate", StringComparison.Ordinal));
    }
}
