using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Place.Domain;
using TravelCore.Modules.Place.Infrastructure;
using TravelCore.Modules.Place.Infrastructure.Services;
using Xunit;
using PlaceAggregate = TravelCore.Modules.Place.Domain.Place;

namespace TravelCore.Modules.Place.UnitTests;

/// <summary>
/// Public hotel browse query (TC-HOTIDX-T002).
/// </summary>
public sealed class PlacePublicQueryTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 20, 12, 0);
    private static readonly Instant Later = Instant.FromUtc(2026, 8, 20, 12, 30);

    [Fact]
    public async Task ListByLocale_Returns_Active_Hotels_With_Slug_Only()
    {
        await using var db = CreateDb();
        var active = PlaceAggregate.CreateHotel("HTL-A", "Active Hotel", Now, 5);
        active.SetCatalogStatus(PlaceCatalogStatus.Active, Later);
        active.UpsertTranslation("fa", "هتل فعال", "توضیح", Later, slug: "active-hotel", setSlug: true);
        var draft = PlaceAggregate.CreateHotel("HTL-D", "Draft Hotel", Now, 4);
        draft.UpsertTranslation("fa", "پیش‌نویس", null, Later, slug: "draft-hotel", setSlug: true);
        var noSlug = PlaceAggregate.CreateHotel("HTL-N", "No Slug Hotel", Now, 3);
        noSlug.SetCatalogStatus(PlaceCatalogStatus.Active, Later);
        noSlug.UpsertTranslation("fa", "بدون اسلاگ", null, Later);
        db.Places.AddRange(active, draft, noSlug);
        await db.SaveChangesAsync();

        var query = new PlacePublicQuery(db);
        var items = await query.ListByLocaleAsync("fa", 50);

        Assert.Single(items);
        Assert.Equal("active-hotel", items[0].Slug);
        Assert.Equal("هتل فعال", items[0].Name);
        Assert.Equal((short?)5, items[0].StarRating);
    }

    private static PlaceDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<PlaceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new PlaceDbContext(options);
    }
}
