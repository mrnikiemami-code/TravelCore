using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Destination.Domain;
using TravelCore.Modules.Destination.Infrastructure;
using TravelCore.Modules.Destination.Infrastructure.Services;
using DestinationAggregate = TravelCore.Modules.Destination.Domain.Destination;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Hierarchy path/ancestors/descendants query proofs (TC-P04-T005).
/// </summary>
[Collection(nameof(DestinationMigrationLifecycleCollection))]
public sealed class DestinationHierarchyQueryTests
{
    private readonly DestinationMigrationLifecycleContainerFixture _postgres;

    public DestinationHierarchyQueryTests(DestinationMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task HierarchyQueries_AncestorsPathAndDepthLimitedDescendants()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var db = _postgres.CreateDbContext())
        {
            await DestinationMigrator.MigrateAsync(db, ct);
        }

        var now = Instant.FromUtc(2026, 8, 15, 23, 45);
        DestinationId countryId;
        DestinationId regionId;
        DestinationId cityId;
        DestinationId areaId;

        await using (var db = _postgres.CreateDbContext())
        {
            // Isolate codes from other lifecycle tests that may share the fixture DB.
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var country = DestinationAggregate.Create(
                DestinationKind.Country,
                $"IR-{suffix}",
                "Iran",
                now,
                isoCountryCode: "IR");
            var region = DestinationAggregate.Create(
                DestinationKind.Region,
                $"IR-THR-{suffix}",
                "Tehran Province",
                now,
                parentId: country.Id,
                parent: country);
            var city = DestinationAggregate.Create(
                DestinationKind.City,
                $"IR-THR-TEH-{suffix}",
                "Tehran",
                now,
                parentId: region.Id,
                parent: region);
            var area = DestinationAggregate.Create(
                DestinationKind.Area,
                $"IR-THR-TEH-VAL-{suffix}",
                "Valiasr",
                now,
                parentId: city.Id,
                parent: city);

            db.Destinations.AddRange(country, region, city, area);
            await db.SaveChangesAsync(ct);
            countryId = country.Id;
            regionId = region.Id;
            cityId = city.Id;
            areaId = area.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var query = new DestinationReadQuery(db);
            var path = await query.GetPathAsync(areaId.Value, ct);
            Assert.NotNull(path);
            Assert.Equal(3, path!.AncestorsRootFirst.Count);
            Assert.Equal(countryId.Value, path.AncestorsRootFirst[0].Id);
            Assert.Equal(regionId.Value, path.AncestorsRootFirst[1].Id);
            Assert.Equal(cityId.Value, path.AncestorsRootFirst[2].Id);
            Assert.Equal(areaId.Value, path.Self.Id);
            Assert.Equal(4, path.BreadcrumbRootFirst.Count);
            Assert.Equal(0, path.BreadcrumbRootFirst[0].DepthFromRoot);
            Assert.Equal(3, path.Self.DepthFromRoot);

            var ancestors = await query.ListAncestorsAsync(areaId.Value, ct);
            Assert.Equal(path.AncestorsRootFirst.Select(x => x.Id), ancestors.Select(x => x.Id));

            var depth1 = await query.ListDescendantsAsync(countryId.Value, maxDepth: 1, ct);
            Assert.NotNull(depth1);
            Assert.Contains(depth1!.Nodes, x => x.Id == regionId.Value);
            Assert.DoesNotContain(depth1.Nodes, x => x.Id == cityId.Value);

            var depth2 = await query.ListDescendantsAsync(countryId.Value, maxDepth: 2, ct);
            Assert.Contains(depth2!.Nodes, x => x.Id == cityId.Value);
            Assert.DoesNotContain(depth2.Nodes, x => x.Id == areaId.Value);

            var depth3 = await query.ListDescendantsAsync(countryId.Value, maxDepth: 3, ct);
            Assert.Contains(depth3!.Nodes, x => x.Id == areaId.Value);
        }
    }
}
