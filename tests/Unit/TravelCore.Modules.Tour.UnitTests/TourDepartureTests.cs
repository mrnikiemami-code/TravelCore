using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class TourDepartureTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 2, 0);

    [Fact]
    public void Create_Links_To_TourProduct_Without_Copying_Product_Fields()
    {
        var product = TourProduct.CreatePackage("PKG-DEP-001", "Europe Package", Now);
        var departure = TourDeparture.Create(product, Now);

        Assert.NotEqual(Guid.Empty, departure.Id.Value);
        Assert.Equal(product.Id, departure.TourProductId);
        Assert.Equal(Now, departure.CreatedAt);
        Assert.Equal(Now, departure.UpdatedAt);
        Assert.NotEqual(product.Id.Value, departure.Id.Value);
    }

    [Fact]
    public void Create_Allows_Multiple_Departures_Per_Product()
    {
        var product = TourProduct.CreateExperience("EXP-DEP-001", "Caspian Walk", Now);
        var a = TourDeparture.Create(product, Now);
        var b = TourDeparture.Create(product, Now);

        Assert.Equal(product.Id, a.TourProductId);
        Assert.Equal(product.Id, b.TourProductId);
        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void Create_Rejects_Null_Product()
    {
        Assert.Throws<ArgumentNullException>(() => TourDeparture.Create(null!, Now));
    }

    [Fact]
    public void TourDepartureId_Rejects_Empty()
    {
        Assert.Throws<ArgumentException>(() => TourDepartureId.From(Guid.Empty));
    }
}
