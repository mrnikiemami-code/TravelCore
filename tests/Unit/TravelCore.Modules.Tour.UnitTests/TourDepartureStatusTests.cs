using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class TourDepartureStatusTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 5, 0);

    private static TourDeparture NewDraft()
    {
        var product = TourProduct.CreatePackage("PKG-ST-001", "Status Package", Now);
        return TourDeparture.Create(product, Now);
    }

    [Fact]
    public void Create_Starts_As_Draft()
    {
        Assert.Equal(TourDepartureStatus.Draft, NewDraft().Status);
    }

    [Fact]
    public void Allowed_Happy_Path_Draft_Published_Closed_Completed()
    {
        var departure = NewDraft();
        departure.SetStatus(TourDepartureStatus.Published, Now);
        Assert.Equal(TourDepartureStatus.Published, departure.Status);

        departure.SetStatus(TourDepartureStatus.Closed, Now);
        Assert.Equal(TourDepartureStatus.Closed, departure.Status);

        departure.SetStatus(TourDepartureStatus.Completed, Now);
        Assert.Equal(TourDepartureStatus.Completed, departure.Status);
    }

    [Fact]
    public void Allowed_Published_To_Cancelled()
    {
        var departure = NewDraft();
        departure.SetStatus(TourDepartureStatus.Published, Now);
        departure.SetStatus(TourDepartureStatus.Cancelled, Now);
        Assert.Equal(TourDepartureStatus.Cancelled, departure.Status);
    }

    [Fact]
    public void Forbidden_Cancelled_To_Published()
    {
        var departure = NewDraft();
        departure.SetStatus(TourDepartureStatus.Published, Now);
        departure.SetStatus(TourDepartureStatus.Cancelled, Now);
        Assert.Throws<InvalidOperationException>(() =>
            departure.SetStatus(TourDepartureStatus.Published, Now));
    }

    [Fact]
    public void Forbidden_Completed_To_Published()
    {
        var departure = NewDraft();
        departure.SetStatus(TourDepartureStatus.Published, Now);
        departure.SetStatus(TourDepartureStatus.Closed, Now);
        departure.SetStatus(TourDepartureStatus.Completed, Now);
        Assert.Throws<InvalidOperationException>(() =>
            departure.SetStatus(TourDepartureStatus.Published, Now));
    }

    [Fact]
    public void Forbidden_Draft_To_Closed()
    {
        Assert.Throws<InvalidOperationException>(() =>
            NewDraft().SetStatus(TourDepartureStatus.Closed, Now));
    }

    [Fact]
    public void Idempotent_Same_Status_Is_Allowed()
    {
        var departure = NewDraft();
        departure.SetStatus(TourDepartureStatus.Draft, Now);
        Assert.Equal(TourDepartureStatus.Draft, departure.Status);
    }
}
