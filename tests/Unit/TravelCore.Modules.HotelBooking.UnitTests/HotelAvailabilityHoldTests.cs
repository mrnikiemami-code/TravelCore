using NodaTime;
using TravelCore.Modules.HotelBooking.Domain;
using Xunit;

namespace TravelCore.Modules.HotelBooking.UnitTests;

public sealed class HotelAvailabilityHoldTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly RoomReservationId Room1 = RoomReservationId.New();
    private static readonly RoomReservationId Room2 = RoomReservationId.New();
    private static readonly HotelBookingId BookingId = HotelBookingId.New();

    private static HotelAvailabilityHold RequestedTwoRooms() =>
        HotelAvailabilityHold.StartRequested(BookingId, "test-source", T0, [Room1, Room2]);

    [Fact]
    public void New_Hold_Starts_Requested()
    {
        var hold = RequestedTwoRooms();
        Assert.Equal(HotelAvailabilityHoldStatus.Requested, hold.Status);
        Assert.Equal(2, hold.Rooms.Count);
        Assert.Equal(7, hold.Id.Value.Version);
        Assert.True(hold.IsUnresolved);
    }

    [Fact]
    public void Authoritative_Activation_Requires_Expiry_And_All_Rooms()
    {
        var hold = RequestedTwoRooms();
        var expires = T0.Plus(Duration.FromHours(2));
        hold.Activate(
            T0.Plus(Duration.FromMinutes(1)),
            expires,
            "src-hold-1",
            new Dictionary<RoomReservationId, string>
            {
                [Room1] = "sel-1",
                [Room2] = "sel-2",
            });

        Assert.Equal(HotelAvailabilityHoldStatus.Active, hold.Status);
        Assert.Equal(expires, hold.ExpiresAt);
        Assert.Equal("src-hold-1", hold.SourceHoldReference);
    }

    [Fact]
    public void Partial_Source_Success_Cannot_Become_Active()
    {
        var hold = RequestedTwoRooms();
        Assert.Throws<InvalidOperationException>(() =>
            hold.Activate(
                T0.Plus(Duration.FromMinutes(1)),
                T0.Plus(Duration.FromHours(2)),
                "src-hold-1",
                new Dictionary<RoomReservationId, string> { [Room1] = "sel-1" }));
        Assert.Equal(HotelAvailabilityHoldStatus.Requested, hold.Status);
    }

    [Fact]
    public void Active_To_Released_And_Expired_Are_Terminal()
    {
        var hold = RequestedTwoRooms();
        hold.Activate(
            T0.Plus(Duration.FromMinutes(1)),
            T0.Plus(Duration.FromHours(2)),
            "src-hold-1",
            new Dictionary<RoomReservationId, string>
            {
                [Room1] = "sel-1",
                [Room2] = "sel-2",
            });

        hold.Release(T0.Plus(Duration.FromMinutes(5)));
        Assert.Equal(HotelAvailabilityHoldStatus.Released, hold.Status);
        hold.Release(T0.Plus(Duration.FromMinutes(6)));
        Assert.Throws<InvalidOperationException>(() =>
            hold.Activate(
                T0.Plus(Duration.FromMinutes(7)),
                T0.Plus(Duration.FromHours(3)),
                "src-hold-2",
                new Dictionary<RoomReservationId, string>
                {
                    [Room1] = "sel-1",
                    [Room2] = "sel-2",
                }));
    }

    [Fact]
    public void Active_To_Expired_Is_Terminal_And_Idempotent()
    {
        var hold = RequestedTwoRooms();
        hold.Activate(
            T0.Plus(Duration.FromMinutes(1)),
            T0.Plus(Duration.FromHours(2)),
            "src-hold-1",
            new Dictionary<RoomReservationId, string>
            {
                [Room1] = "sel-1",
                [Room2] = "sel-2",
            });

        hold.Expire(T0.Plus(Duration.FromHours(3)));
        hold.Expire(T0.Plus(Duration.FromHours(4)));
        Assert.Equal(HotelAvailabilityHoldStatus.Expired, hold.Status);
        Assert.Throws<InvalidOperationException>(() => hold.Release(T0.Plus(Duration.FromHours(5))));
    }

    [Fact]
    public void Local_Expiry_Uses_Source_ExpiresAt_Not_Hardcoded_Ttl()
    {
        var hold = RequestedTwoRooms();
        var expires = T0.Plus(Duration.FromMinutes(30));
        hold.Activate(
            T0.Plus(Duration.FromMinutes(1)),
            expires,
            "src-hold-1",
            new Dictionary<RoomReservationId, string>
            {
                [Room1] = "sel-1",
                [Room2] = "sel-2",
            });

        hold.ApplyLocalExpiryIfDue(expires.Plus(Duration.FromSeconds(-1)));
        Assert.Equal(HotelAvailabilityHoldStatus.Active, hold.Status);
        hold.ApplyLocalExpiryIfDue(expires);
        Assert.Equal(HotelAvailabilityHoldStatus.Expired, hold.Status);
    }

    [Fact]
    public void Hold_Status_Exact_Values_Are_Requested_Active_Released_Expired()
    {
        Assert.Equal(
            new[] { "Requested", "Active", "Released", "Expired" },
            Enum.GetNames<HotelAvailabilityHoldStatus>());
        Assert.DoesNotContain("Failed", Enum.GetNames<HotelAvailabilityHoldStatus>());
    }
}
