using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Pricing.Contracts;
using TravelCore.Modules.Pricing.Domain;
using TravelCore.Modules.Pricing.Infrastructure;
using TravelCore.Modules.Pricing.Infrastructure.Services;
using Xunit;

namespace TravelCore.Modules.Pricing.UnitTests;

/// <summary>
/// Public read-only price summary (TC-P12-T008 / P12-R8).
/// </summary>
public sealed class PublicPricingQueryTests
{
    private static Guid NonEmptyTargetId => Guid.Parse("01900000-0000-7000-8000-0000000000c8");

    [Fact]
    public void Map_Exposes_Currency_Components_And_Public_Occupancy_Only()
    {
        var price = Price.Create(
            PriceTargetType.TourDepartureValue,
            NonEmptyTargetId,
            [
                new PriceComponentDefinition(PriceComponentKind.Base, PricingMoney.Create(1290m, "USD"), SortOrder: 0, Code: "BASE"),
                new PriceComponentDefinition(PriceComponentKind.Tax, PricingMoney.Create(90m, "USD"), SortOrder: 1, Code: "VAT")
            ],
            [
                new PriceOccupancyRuleDefinition(
                    TourMarketPriceType.Public,
                    PassengerCategory.Adult,
                    OccupancyCategory.SingleRoom,
                    PricingMoney.Create(1290m, "USD"),
                    SortOrder: 0),
                new PriceOccupancyRuleDefinition(
                    TourMarketPriceType.Agency,
                    PassengerCategory.Adult,
                    OccupancyCategory.SingleRoom,
                    PricingMoney.Create(1100m, "USD"),
                    SortOrder: 1),
                new PriceOccupancyRuleDefinition(
                    TourMarketPriceType.Public,
                    PassengerCategory.ChildWithBed,
                    OccupancyCategory.DoubleRoom,
                    PricingMoney.Create(900m, "USD"),
                    SortOrder: 2)
            ]);

        var summary = PublicPricingQuery.Map(price);

        Assert.Equal(price.Id.Value, summary.PriceId);
        Assert.Equal(PublicPricingTargets.TourDeparture, summary.TargetType);
        Assert.Equal(NonEmptyTargetId, summary.TargetId);
        Assert.Equal("USD", summary.Currency);
        Assert.Equal(2, summary.Components.Count);
        Assert.Equal("Base", summary.Components[0].Kind);
        Assert.Equal(1290m, summary.Components[0].Money.Amount);
        Assert.Equal("USD", summary.Components[0].Money.CurrencyCode);
        Assert.Equal("Tax", summary.Components[1].Kind);

        Assert.Equal(2, summary.OccupancyPrices.Count);
        Assert.All(summary.OccupancyPrices, row => Assert.Equal("USD", row.Money.CurrencyCode));
        Assert.Contains(
            summary.OccupancyPrices,
            row => row.PassengerCategory == "Adult" && row.OccupancyCategory == "SingleRoom" && row.Money.Amount == 1290m);
        Assert.Contains(
            summary.OccupancyPrices,
            row => row.PassengerCategory == "ChildWithBed" && row.OccupancyCategory == "DoubleRoom");
        Assert.DoesNotContain(summary.OccupancyPrices, row => row.Money.Amount == 1100m);

        Assert.Null(typeof(PublicPriceSummary).GetProperty("ConvertedAmount"));
        Assert.Null(typeof(PublicPriceSummary).GetProperty("DisplayAmount"));
        Assert.Null(typeof(PublicPriceSummary).GetProperty("ExchangeRate"));
        Assert.Null(typeof(PublicOccupancyPriceSummary).GetProperty("ConvertedAmount"));
    }

    [Fact]
    public void GetSummaryAsync_Rejects_Empty_TargetId_Before_Query()
    {
        using var db = CreateProbeContext();
        var query = new PublicPricingQuery(db);

        var ex = Assert.Throws<ArgumentException>(() =>
            query.GetSummaryAsync(
                    PublicPricingTargets.TourDeparture,
                    Guid.Empty,
                    TestContext.Current.CancellationToken)
                .GetAwaiter()
                .GetResult());

        Assert.Equal("targetId", ex.ParamName);
    }

    [Fact]
    public void GetByTourDepartureIdAsync_Rejects_Empty_Id()
    {
        using var db = CreateProbeContext();
        var query = new PublicPricingQuery(db);

        var ex = Assert.Throws<ArgumentException>(() =>
            query.GetByTourDepartureIdAsync(Guid.Empty, TestContext.Current.CancellationToken)
                .GetAwaiter()
                .GetResult());

        Assert.Equal("targetId", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("TourProduct")]
    public void GetSummaryAsync_Rejects_Unknown_TargetType(string? targetType)
    {
        using var db = CreateProbeContext();
        var query = new PublicPricingQuery(db);

        Assert.Throws<ArgumentException>(() =>
            query.GetSummaryAsync(targetType!, NonEmptyTargetId, TestContext.Current.CancellationToken)
                .GetAwaiter()
                .GetResult());
    }

    [Fact]
    public void Public_Contracts_Are_ReadOnly_Query_Without_Mutation_Or_Fx()
    {
        Assert.NotNull(typeof(IPublicPricingQuery).GetMethod(nameof(IPublicPricingQuery.GetSummaryAsync)));
        Assert.NotNull(typeof(IPublicPricingQuery).GetMethod(nameof(IPublicPricingQuery.GetByTourDepartureIdAsync)));
        Assert.Null(typeof(IPublicPricingQuery).GetMethod("CreateAsync"));
        Assert.Null(typeof(IPublicPricingQuery).GetMethod("UpdateAsync"));
        Assert.Null(typeof(IPublicPricingQuery).GetMethod("RequestDisplayConversionAsync"));
        Assert.Equal("TourDeparture", PublicPricingTargets.TourDeparture);
    }

    private static PricingDbContext CreateProbeContext() =>
        new(
            new DbContextOptionsBuilder<PricingDbContext>()
                .UseNpgsql(
                    "Host=127.0.0.1;Database=travelcore_pricing_public_probe;Username=x;Password=x",
                    npgsql => npgsql.UseNodaTime())
                .Options);
}
