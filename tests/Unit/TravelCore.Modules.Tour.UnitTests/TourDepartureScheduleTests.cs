using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class TourDepartureScheduleTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 3, 0);
    private static readonly LocalDate Start = new(2027, 5, 1);
    private static readonly LocalDate End = new(2027, 5, 6);

    [Fact]
    public void SetSchedule_Attaches_LocalDates_And_Iana_Zone()
    {
        var product = TourProduct.CreatePackage("PKG-SCH-001", "Istanbul Package", Now);
        var departure = TourDeparture.Create(product, Now);

        departure.SetSchedule(Start, End, "Europe/Istanbul", Now);

        Assert.NotNull(departure.Schedule);
        Assert.Equal(Start, departure.Schedule!.StartDate);
        Assert.Equal(End, departure.Schedule.EndDate);
        Assert.Equal("Europe/Istanbul", departure.Schedule.TimeZoneId);
        Assert.Equal(Now, departure.UpdatedAt);
    }

    [Fact]
    public void Schedule_Allows_Same_Start_And_End()
    {
        var schedule = TourDepartureSchedule.Create(Start, Start, "Asia/Tehran");
        Assert.Equal(Start, schedule.StartDate);
        Assert.Equal(Start, schedule.EndDate);
    }

    [Fact]
    public void Schedule_Rejects_End_Before_Start()
    {
        Assert.Throws<ArgumentException>(() =>
            TourDepartureSchedule.Create(End, Start, "Asia/Tehran"));
    }

    [Fact]
    public void Schedule_Rejects_Unknown_Timezone()
    {
        Assert.Throws<ArgumentException>(() =>
            TourDepartureSchedule.Create(Start, End, "Not/AZone"));
    }

    [Fact]
    public void Schedule_Rejects_Blank_Timezone()
    {
        Assert.Throws<ArgumentException>(() =>
            TourDepartureSchedule.Create(Start, End, "  "));
    }
}
