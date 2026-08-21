using System.Reflection;
using NodaTime;
using TravelCore.Modules.Pricing.Domain;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;
using Xunit;

namespace TravelCore.Modules.Pricing.UnitTests;

/// <summary>
/// Quote + PriceSnapshot domain invariants (TC-P12-T004 / P12-R4 · TC-P12-T007 / P12-R7).
/// Price ≠ Quote ≠ Booking Amount; requested display currency is metadata only; no Customer/Passenger/Payment.
/// </summary>
public sealed class QuoteAggregateTests
{
    private static Guid NonEmptyTargetId => Guid.Parse("01900000-0000-7000-8000-000000000001");
    private static Instant CreatedAt => Instant.FromUtc(2026, 8, 17, 9, 0);
    private static Instant ExpiresAt => Instant.FromUtc(2026, 8, 17, 10, 0);

    private static Price SamplePrice() =>
        Price.Create(
            PriceTargetType.TourDepartureValue,
            NonEmptyTargetId,
            [
                new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(1000m, "USD"), SortOrder: 0, Code: "BASE"),
                new PriceComponentDefinition(PriceComponentKind.Fee, PricingMoney.Create(50m, "USD"), SortOrder: 1, Code: "SVC"),
                new PriceComponentDefinition(PriceComponentKind.Tax, PricingMoney.Create(90m, "USD"), SortOrder: 2, Code: "VAT")
            ]);

    [Fact]
    public void CreateFromPrice_Snapshots_Components_And_Target_Copy()
    {
        var price = SamplePrice();
        var quote = Quote.CreateFromPrice(price, CreatedAt, ExpiresAt);

        Assert.Equal(price.Id, quote.SourcePriceId);
        Assert.Equal(PriceTargetType.TourDepartureValue, quote.SnapshotTargetType!.Value);
        Assert.Equal(NonEmptyTargetId, quote.SnapshotTargetId);
        Assert.Equal(3, quote.SnapshotComponents.Count);
        Assert.Equal("USD", quote.Currency.Value);
        Assert.Equal(1140m, quote.Total.Amount);
        Assert.Equal(CreatedAt, quote.CreatedAt);
        Assert.Equal(ExpiresAt, quote.ExpiresAt);
        Assert.NotEqual(Guid.Empty, quote.Id.Value);
    }

    [Fact]
    public void Create_Rejects_Empty_Snapshot()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Quote.Create(PriceId.New(), [], CreatedAt, ExpiresAt));

        Assert.Equal("snapshotComponents", ex.ParamName);
        Assert.Contains("snapshot", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_Rejects_Expiration_Not_After_CreatedAt()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Quote.CreateFromPrice(SamplePrice(), CreatedAt, CreatedAt));

        Assert.Equal("expiresAt", ex.ParamName);
    }

    [Fact]
    public void Create_Rejects_Expiration_Before_CreatedAt()
    {
        var earlier = Instant.FromUtc(2026, 8, 17, 8, 0);
        Assert.Throws<ArgumentException>(() =>
            Quote.CreateFromPrice(SamplePrice(), CreatedAt, earlier));
    }

    [Fact]
    public void IsExpired_Is_True_At_And_After_ExpiresAt()
    {
        var quote = Quote.CreateFromPrice(SamplePrice(), CreatedAt, ExpiresAt);

        Assert.False(quote.IsExpired(CreatedAt));
        Assert.False(quote.IsExpired(Instant.FromUtc(2026, 8, 17, 9, 59)));
        Assert.True(quote.IsExpired(ExpiresAt));
        Assert.True(quote.IsExpired(Instant.FromUtc(2026, 8, 17, 11, 0)));
    }

    [Fact]
    public void Create_Rejects_Mixed_Currencies_In_Snapshot()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Quote.Create(
                PriceId.New(),
                [
                    new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(100m, "USD"), SortOrder: 0),
                    new PriceComponentDefinition(PriceComponentKind.Fee, PricingMoney.Create(1000m, "IRR"), SortOrder: 1)
                ],
                CreatedAt,
                ExpiresAt));

        Assert.Contains("same currency", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Snapshot_Is_Independent_Of_Later_Price_Mutation()
    {
        var price = SamplePrice();
        var quote = Quote.CreateFromPrice(price, CreatedAt, ExpiresAt);
        var snapTotal = quote.Total.Amount;

        price.AddComponent(PriceComponentKind.Fee, PricingMoney.Create(500m, "USD"), sortOrder: 9, code: "LATE");

        Assert.Equal(snapTotal, quote.Total.Amount);
        Assert.Equal(3, quote.SnapshotComponents.Count);
        Assert.Equal(4, price.Components.Count);
    }

    [Fact]
    public void SnapshotComponents_Have_No_Public_Mutators()
    {
        var mutators = typeof(QuoteSnapshotComponent)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName)
            .Select(m => m.Name)
            .ToList();

        Assert.Empty(mutators);
        Assert.Null(typeof(Quote).GetMethod("AddComponent", BindingFlags.Instance | BindingFlags.Public));
        Assert.Null(typeof(Quote).GetMethod("AddSnapshotComponent", BindingFlags.Instance | BindingFlags.Public));
    }

    [Fact]
    public void Quote_Has_No_Customer_Passenger_Payment_Or_Booking_Fields()
    {
        var names = typeof(Quote)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("CustomerId", names);
        Assert.DoesNotContain("PassengerId", names);
        Assert.DoesNotContain("Passenger", names);
        Assert.DoesNotContain("PaymentId", names);
        Assert.DoesNotContain("Payment", names);
        Assert.DoesNotContain("BookingId", names);
        Assert.DoesNotContain("Booking", names);
        Assert.DoesNotContain("ReservationId", names);
        Assert.DoesNotContain("Reservation", names);
        Assert.DoesNotContain("Checkout", names);

        Assert.Contains("SourcePriceId", names);
        Assert.Contains("ExpiresAt", names);
        Assert.Contains("SnapshotComponents", names);
        Assert.Contains("RequestedDisplayCurrency", names);
    }

    [Fact]
    public void Quote_Total_Reuses_Platform_Money()
    {
        var quote = Quote.CreateFromPrice(SamplePrice(), CreatedAt, ExpiresAt);
        Assert.IsType<MoneyValue>(quote.Total);
        Assert.Equal("TravelCore.Money", typeof(MoneyValue).Assembly.GetName().Name);
    }

    [Fact]
    public void CreateFromPrice_Stores_CommercialContextAgencyOfferId_As_Metadata_Only()
    {
        var offerId = Guid.Parse("0198b3e0-0000-7000-8000-000000000901");
        var quote = Quote.CreateFromPrice(
            SamplePrice(),
            CreatedAt,
            ExpiresAt,
            commercialContextAgencyOfferId: offerId);

        Assert.Equal(offerId, quote.CommercialContextAgencyOfferId);
        Assert.Equal("USD", quote.Currency.Value);
        Assert.Equal(1140m, quote.Total.Amount);
    }

    [Fact]
    public void CreateFromPrice_Rejects_Empty_CommercialContextAgencyOfferId()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Quote.CreateFromPrice(
                SamplePrice(),
                CreatedAt,
                ExpiresAt,
                commercialContextAgencyOfferId: Guid.Empty));

        Assert.Equal("commercialContextAgencyOfferId", ex.ParamName);
    }

    [Fact]
    public void CreateFromPrice_Without_RequestedDisplayCurrency_Leaves_Metadata_Null()
    {
        var quote = Quote.CreateFromPrice(SamplePrice(), CreatedAt, ExpiresAt);

        Assert.Null(quote.RequestedDisplayCurrency);
        Assert.Null(quote.CommercialContextAgencyOfferId);
        Assert.Equal("USD", quote.Currency.Value);
        Assert.Equal(1140m, quote.Total.Amount);
    }

    [Fact]
    public void CreateFromPrice_Stores_RequestedDisplayCurrency_As_Metadata_Only()
    {
        var quote = Quote.CreateFromPrice(SamplePrice(), CreatedAt, ExpiresAt, requestedDisplayCurrency: "irr");

        Assert.Equal("IRR", quote.RequestedDisplayCurrency!.Value);
        Assert.Equal("USD", quote.Currency.Value);
        Assert.Equal("USD", quote.Total.Currency.Value);
        Assert.Equal(1140m, quote.Total.Amount);
        Assert.All(quote.SnapshotComponents, c => Assert.Equal("USD", c.Money.Currency.Value));
    }

    [Fact]
    public void Create_Allows_Same_Code_RequestedDisplayCurrency()
    {
        var quote = Quote.CreateFromPrice(SamplePrice(), CreatedAt, ExpiresAt, requestedDisplayCurrency: "USD");

        Assert.Equal("USD", quote.RequestedDisplayCurrency!.Value);
        Assert.Equal("USD", quote.Currency.Value);
        Assert.Equal(1140m, quote.Total.Amount);
    }

    [Fact]
    public void Create_Rejects_Toman_RequestedDisplayCurrency()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Quote.CreateFromPrice(SamplePrice(), CreatedAt, ExpiresAt, requestedDisplayCurrency: "TOMAN"));

        Assert.Contains("TOMAN", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Quote_Has_No_Conversion_Methods()
    {
        var names = typeof(Quote)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName)
            .Select(m => m.Name)
            .ToList();

        Assert.DoesNotContain(names, n => n.Contains("Convert", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, n => n.Contains("Exchange", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain("ToDisplayMoney", names);
        Assert.DoesNotContain("ConvertTotal", names);
        Assert.Contains("CreateFromPrice", names);
        Assert.Contains(
            "RequestedDisplayCurrency",
            typeof(Quote).GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(p => p.Name));
    }
}
