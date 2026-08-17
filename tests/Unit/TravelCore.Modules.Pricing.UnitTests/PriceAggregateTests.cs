using TravelCore.Modules.Pricing.Domain;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;
using Xunit;

namespace TravelCore.Modules.Pricing.UnitTests;

/// <summary>
/// Price + PriceComponent domain invariants (TC-P12-T003 / P12-R3).
/// </summary>
public sealed class PriceAggregateTests
{
    private static Guid NonEmptyTargetId => Guid.Parse("01900000-0000-7000-8000-000000000001");

    [Fact]
    public void Create_Accepts_TourDeparture_Target_With_Base_And_Fee_Tax()
    {
        var price = Price.Create(
            PriceTargetType.TourDepartureValue,
            NonEmptyTargetId,
            [
                new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(1000m, "USD"), SortOrder: 0, Code: "BASE"),
                new PriceComponentDefinition(PriceComponentKind.Fee, PricingMoney.Create(50m, "USD"), SortOrder: 1, Code: "SVC", Label: "Service"),
                new PriceComponentDefinition(PriceComponentKind.Tax, PricingMoney.Create(90m, "USD"), SortOrder: 2, Code: "VAT")
            ]);

        Assert.Equal(PriceTargetType.TourDepartureValue, price.TargetType.Value);
        Assert.Equal(NonEmptyTargetId, price.TargetId);
        Assert.Equal(3, price.Components.Count);
        Assert.Equal("USD", price.Currency.Value);
        Assert.Contains(price.Components, c => c.Kind == PriceComponentKind.Base);
        Assert.Contains(price.Components, c => c.Kind == PriceComponentKind.Fee);
        Assert.Contains(price.Components, c => c.Kind == PriceComponentKind.Tax);
        Assert.NotEqual(Guid.Empty, price.Id.Value);
    }

    [Fact]
    public void Create_Rejects_Empty_TargetId()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Price.Create(
                PriceTargetType.TourDepartureValue,
                Guid.Empty,
                [new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(1m, "IRR"))]));

        Assert.Equal("targetId", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("TourProduct")]
    [InlineData("Booking")]
    public void Create_Rejects_Unknown_Or_Missing_TargetType(string? targetType)
    {
        Assert.Throws<ArgumentException>(() =>
            Price.Create(
                targetType!,
                NonEmptyTargetId,
                [new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(1m, "USD"))]));
    }

    [Fact]
    public void Create_Requires_At_Least_One_Base_Component()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Price.Create(
                PriceTargetType.TourDepartureValue,
                NonEmptyTargetId,
                [
                    new PriceComponentDefinition(PriceComponentKind.Fee, PricingMoney.Create(10m, "USD"), SortOrder: 0),
                    new PriceComponentDefinition(PriceComponentKind.Tax, PricingMoney.Create(2m, "USD"), SortOrder: 1)
                ]));

        Assert.Equal("components", ex.ParamName);
        Assert.Contains("Base", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_Rejects_Empty_Component_List()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Price.Create(PriceTargetType.TourDepartureValue, NonEmptyTargetId, []));

        Assert.Equal("components", ex.ParamName);
    }

    [Fact]
    public void Create_Rejects_Mixed_Currencies_Within_One_Price()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Price.Create(
                PriceTargetType.TourDepartureValue,
                NonEmptyTargetId,
                [
                    new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(100m, "USD"), SortOrder: 0),
                    new PriceComponentDefinition(PriceComponentKind.Fee, PricingMoney.Create(1000m, "IRR"), SortOrder: 1)
                ]));

        Assert.Contains("same currency", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_Rejects_Toman_Via_PricingMoney()
    {
        Assert.Throws<ArgumentException>(() =>
            Price.Create(
                PriceTargetType.TourDepartureValue,
                NonEmptyTargetId,
                [new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(1000m, "TOMAN"))]));
    }

    [Fact]
    public void Create_Rejects_Duplicate_SortOrder()
    {
        Assert.Throws<ArgumentException>(() =>
            Price.Create(
                PriceTargetType.TourDepartureValue,
                NonEmptyTargetId,
                [
                    new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(1m, "EUR"), SortOrder: 1),
                    new PriceComponentDefinition(PriceComponentKind.Fee, PricingMoney.Create(2m, "EUR"), SortOrder: 1)
                ]));
    }

    [Fact]
    public void Create_Rejects_Duplicate_Code()
    {
        Assert.Throws<ArgumentException>(() =>
            Price.Create(
                PriceTargetType.TourDepartureValue,
                NonEmptyTargetId,
                [
                    new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(1m, "EUR"), SortOrder: 0, Code: "X"),
                    new PriceComponentDefinition(PriceComponentKind.Fee, PricingMoney.Create(2m, "EUR"), SortOrder: 1, Code: "X")
                ]));
    }

    [Fact]
    public void AddComponent_Rejects_Cross_Currency()
    {
        var price = Price.Create(
            PriceTargetType.TourDepartureValue,
            NonEmptyTargetId,
            [new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(10m, "USD"))]);

        Assert.Throws<ArgumentException>(() =>
            price.AddComponent(PriceComponentKind.Fee, PricingMoney.Create(100m, "IRR"), sortOrder: 1));
    }

    [Fact]
    public void AddComponent_Accepts_Same_Currency_Fee()
    {
        var price = Price.Create(
            PriceTargetType.TourDepartureValue,
            NonEmptyTargetId,
            [new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(10m, "USD"))]);

        var fee = price.AddComponent(PriceComponentKind.Fee, PricingMoney.Create(1.5m, "USD"), sortOrder: 5, code: "BOOKING");

        Assert.Equal(PriceComponentKind.Fee, fee.Kind);
        Assert.Equal(2, price.Components.Count);
        Assert.Equal(1.5m, fee.Money.Amount);
    }

    [Fact]
    public void Price_Reuses_Platform_Money_On_Components()
    {
        var money = PricingMoney.Create(42m, "GBP");
        var price = Price.Create(
            PriceTargetType.TourDepartureValue,
            NonEmptyTargetId,
            [new PriceComponentDefinition(PriceComponentKind.Base, money)]);

        var component = Assert.Single(price.Components);
        Assert.IsType<MoneyValue>(component.Money);
        Assert.Equal("TravelCore.Money", typeof(MoneyValue).Assembly.GetName().Name);
    }

    [Fact]
    public void PriceTargetType_TourDeparture_Is_Only_Allowed_Value()
    {
        Assert.Equal("TourDeparture", PriceTargetType.Parse("TourDeparture").Value);
        Assert.Same(PriceTargetType.TourDeparture, PriceTargetType.Parse("TourDeparture"));
    }

    [Fact]
    public void Create_Accepts_Occupancy_Rules_For_Adult_And_Child_Categories()
    {
        var price = Price.Create(
            PriceTargetType.TourDepartureValue,
            NonEmptyTargetId,
            [new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(100m, "USD"))],
            [
                new PriceOccupancyRuleDefinition(
                    TourMarketPriceType.Public,
                    PassengerCategory.Adult,
                    OccupancyCategory.SingleRoom,
                    PricingMoney.Create(120m, "USD"),
                    SortOrder: 0),
                new PriceOccupancyRuleDefinition(
                    TourMarketPriceType.Public,
                    PassengerCategory.ChildWithBed,
                    OccupancyCategory.DoubleRoom,
                    PricingMoney.Create(90m, "USD"),
                    SortOrder: 1),
                new PriceOccupancyRuleDefinition(
                    TourMarketPriceType.Public,
                    PassengerCategory.ChildWithoutBed,
                    OccupancyCategory.DoubleRoom,
                    PricingMoney.Create(60m, "USD"),
                    SortOrder: 2)
            ]);

        Assert.Equal(3, price.OccupancyRules.Count);
        Assert.Contains(price.OccupancyRules, r => r.PassengerCategory == PassengerCategory.Adult);
        Assert.Contains(price.OccupancyRules, r => r.PassengerCategory == PassengerCategory.ChildWithBed);
        Assert.Contains(price.OccupancyRules, r => r.PassengerCategory == PassengerCategory.ChildWithoutBed);
        Assert.Contains(price.OccupancyRules, r => r.OccupancyCategory == OccupancyCategory.SingleRoom);
    }

    [Fact]
    public void Create_Rejects_Occupancy_Rule_When_Currency_Differs_From_Price()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Price.Create(
                PriceTargetType.TourDepartureValue,
                NonEmptyTargetId,
                [new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(100m, "USD"))],
                [
                    new PriceOccupancyRuleDefinition(
                        TourMarketPriceType.Public,
                        PassengerCategory.Adult,
                        OccupancyCategory.SingleRoom,
                        PricingMoney.Create(1000000m, "IRR"))
                ]));

        Assert.Equal("rules", ex.ParamName);
    }

    [Fact]
    public void AddOccupancyRule_Rejects_Duplicate_MarketPassengerOccupancy_Tuple()
    {
        var price = Price.Create(
            PriceTargetType.TourDepartureValue,
            NonEmptyTargetId,
            [new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(10m, "USD"))]);

        price.AddOccupancyRule(
            TourMarketPriceType.Public,
            PassengerCategory.Adult,
            OccupancyCategory.SingleRoom,
            PricingMoney.Create(12m, "USD"),
            sortOrder: 1);

        Assert.Throws<InvalidOperationException>(() =>
            price.AddOccupancyRule(
                TourMarketPriceType.Public,
                PassengerCategory.Adult,
                OccupancyCategory.SingleRoom,
                PricingMoney.Create(13m, "USD"),
                sortOrder: 2));
    }
}
