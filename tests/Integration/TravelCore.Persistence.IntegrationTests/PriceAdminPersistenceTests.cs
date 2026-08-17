using TravelCore.Modules.Pricing.Contracts;
using TravelCore.Modules.Pricing.Infrastructure;
using TravelCore.Modules.Pricing.Infrastructure.Services;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Admin Price persistence round-trip (TC-P12-T006).
/// </summary>
[Collection(nameof(PricingMigrationLifecycleCollection))]
public sealed class PriceAdminPersistenceTests
{
    private readonly PricingMigrationLifecycleContainerFixture _postgres;

    public PriceAdminPersistenceTests(PricingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task PriceAdmin_Create_Update_Components_And_Occupancy_Rules_RoundTrip()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PricingMigrator.MigrateAsync(migrate, ct);
        }

        var targetId = Guid.Parse("01900000-0000-7000-8000-0000000000aa");
        await using var db = _postgres.CreateDbContext();
        var service = new PriceAdminService(db);

        var created = await service.CreateAsync(
            new CreatePriceRequest(
                TargetType: "TourDeparture",
                TargetId: targetId,
                Components:
                [
                    new PriceComponentInput("Base", new MoneyInput(100m, "USD"), SortOrder: 0, Code: "BASE")
                ],
                OccupancyRules:
                [
                    new PriceOccupancyRuleInput(
                        "Public",
                        "Adult",
                        "SingleRoom",
                        new MoneyInput(120m, "USD"),
                        SortOrder: 0)
                ]),
            ct);

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("TourDeparture", created.TargetType);
        Assert.Equal(targetId, created.TargetId);
        Assert.Equal("USD", created.CurrencyCode);
        Assert.Single(created.Components);
        Assert.Single(created.OccupancyRules);

        var listed = await service.ListAsync("TourDeparture", targetId, take: 10, ct);
        Assert.Contains(listed, x => x.Id == created.Id);

        var updated = await service.UpdateAsync(
            created.Id,
            new UpdatePriceRequest(
                Components:
                [
                    new PriceComponentInput("Base", new MoneyInput(200m, "EUR"), SortOrder: 0, Code: "BASE"),
                    new PriceComponentInput("Fee", new MoneyInput(15m, "EUR"), SortOrder: 1, Code: "SVC")
                ],
                OccupancyRules:
                [
                    new PriceOccupancyRuleInput(
                        "Agency",
                        "ChildWithBed",
                        "DoubleRoom",
                        new MoneyInput(90m, "EUR"),
                        SortOrder: 0)
                ]),
            ct);

        Assert.Equal("EUR", updated.CurrencyCode);
        Assert.Equal(2, updated.Components.Count);
        var rule = Assert.Single(updated.OccupancyRules);
        Assert.Equal("Agency", rule.MarketPriceType);
        Assert.Equal("ChildWithBed", rule.PassengerCategory);
        Assert.Equal("DoubleRoom", rule.OccupancyCategory);

        var added = await service.AddComponentAsync(
            created.Id,
            new PriceComponentInput("Tax", new MoneyInput(8m, "EUR"), SortOrder: 2, Code: "VAT"),
            ct);
        Assert.Equal(3, added.Components.Count);

        var withRule = await service.AddOccupancyRuleAsync(
            created.Id,
            new PriceOccupancyRuleInput(
                "Agency",
                "Adult",
                "TwinRoom",
                new MoneyInput(110m, "EUR"),
                SortOrder: 1),
            ct);
        Assert.Equal(2, withRule.OccupancyRules.Count);

        await using var reloadDb = _postgres.CreateDbContext();
        var reloadService = new PriceAdminService(reloadDb);
        var reloaded = await reloadService.GetAsync(created.Id, ct);
        Assert.NotNull(reloaded);
        Assert.Equal(3, reloaded.Components.Count);
        Assert.Equal(2, reloaded.OccupancyRules.Count);
        Assert.Equal("EUR", reloaded.CurrencyCode);
        Assert.Contains(reloaded.Components, c => c.Kind == "Tax" && c.Code == "VAT");
    }
}
