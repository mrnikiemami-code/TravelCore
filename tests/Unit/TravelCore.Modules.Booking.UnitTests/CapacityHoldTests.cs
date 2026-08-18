using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Modules.Booking.UnitTests;

public sealed class CapacityHoldTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 6, 0);
    private static readonly Instant Expires = Instant.FromUtc(2026, 8, 18, 7, 0);
    private static readonly BookingId BookingId = BookingId.From(Guid.Parse("0198b3e0-0000-7000-8000-000000000301"));
    private static readonly TourDepartureReference Departure =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000302"));

    [Fact]
    public void Create_Starts_Active_With_Positive_Seats_And_Expiry()
    {
        var hold = CapacityHold.Create(BookingId, Departure, 2, 10, Now, Expires, "hold-key-1");

        Assert.Equal(CapacityHoldStatus.Active, hold.Status);
        Assert.Equal(2, hold.SeatCount);
        Assert.Equal(10, hold.ObservedConfiguredCapacity);
        Assert.Equal(Expires, hold.ExpiresAt);
        Assert.Equal(7, hold.Id.Value.Version);
        Assert.Equal("NOT Tour Source of Truth", CapacityConsumptionBoundary.ObservedCapacityIsNotTourSourceOfTruth);
        Assert.False(CapacityConsumptionBoundary.HoldDurationHardcoded);
    }

    [Fact]
    public void Create_Rejects_NonPositive_Seats_And_NonPositive_Expiry()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CapacityHold.Create(BookingId, Departure, 0, 10, Now, Expires, "hold-key-2"));
        Assert.Throws<ArgumentException>(
            () => CapacityHold.Create(BookingId, Departure, 1, 10, Now, Now, "hold-key-3"));
    }

    [Fact]
    public void Release_Expire_Consume_Are_Terminal_And_Idempotent_For_Same_Status()
    {
        var released = CapacityHold.Create(BookingId, Departure, 1, 4, Now, Expires, "hold-key-4");
        released.Release(Expires);
        released.Release(Expires.Plus(Duration.FromMinutes(1)));
        Assert.Equal(CapacityHoldStatus.Released, released.Status);
        Assert.Throws<InvalidOperationException>(() => released.Consume(Expires));

        var expired = CapacityHold.Create(BookingId, Departure, 1, 4, Now, Expires, "hold-key-5");
        Assert.Throws<InvalidOperationException>(() => expired.Expire(Now));
        expired.Expire(Expires);
        expired.Expire(Expires.Plus(Duration.FromMinutes(1)));
        Assert.Equal(CapacityHoldStatus.Expired, expired.Status);

        var consumed = CapacityHold.Create(BookingId, Departure, 1, 4, Now, Expires, "hold-key-6");
        consumed.Consume(Now.Plus(Duration.FromMinutes(1)));
        consumed.Consume(Now.Plus(Duration.FromMinutes(2)));
        Assert.Equal(CapacityHoldStatus.Consumed, consumed.Status);
        Assert.Throws<InvalidOperationException>(() => consumed.Release(Expires));
    }

    [Fact]
    public void Account_Reserves_Until_Configured_Capacity_Then_Fails()
    {
        var account = DepartureCapacityAccount.Create(Departure);
        account.Reserve(3, 5);
        Assert.Equal(3, account.EffectiveSeats);
        var ex = Assert.Throws<InsufficientCapacityException>(() => account.Reserve(3, 5));
        Assert.Equal(3, ex.RequestedSeats);
        Assert.Equal(2, ex.AvailableSeats);

        account.ConsumeActive(3);
        Assert.Equal(0, account.ActiveSeats);
        Assert.Equal(3, account.ConsumedSeats);
        Assert.Throws<InsufficientCapacityException>(() => account.Reserve(3, 5));
    }

    [Fact]
    public void Account_Release_Frees_Active_But_Consumed_Remains()
    {
        var account = DepartureCapacityAccount.Create(Departure);
        account.Reserve(2, 2);
        account.ReleaseActive(2);
        Assert.Equal(0, account.EffectiveSeats);
        account.Reserve(2, 2);
        account.ConsumeActive(2);
        Assert.Equal(2, account.EffectiveSeats);
        Assert.Throws<InsufficientCapacityException>(() => account.Reserve(1, 2));
    }

    [Fact]
    public void BookingStatus_Still_Has_No_Hold_Or_Expired_Values()
    {
        Assert.Equal(new[] { "Pending", "Confirmed", "Cancelled" }, Enum.GetNames<BookingStatus>());
        var booking = BookingAggregate.Create(Departure, Now);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Equal("Pending != CapacityHeld", CapacityConsumptionBoundary.PendingIsNotCapacityHeld);
    }
}
