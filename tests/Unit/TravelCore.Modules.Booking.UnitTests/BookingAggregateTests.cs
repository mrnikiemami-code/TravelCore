using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Modules.Booking.UnitTests;

public sealed class BookingAggregateTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 1, 0);

    [Fact]
    public void Create_Starts_Pending_With_TourDeparture_And_Uuidv7_Id()
    {
        var departure = new TourDepartureReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000101"));
        var booking = BookingAggregate.Create(departure, Now);

        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Equal(departure, booking.TourDeparture);
        Assert.Equal(Now, booking.CreatedAt);
        Assert.Equal(Now, booking.StatusChangedAt);
        Assert.NotEqual(Guid.Empty, booking.Id.Value);
        Assert.Equal(7, booking.Id.Value.Version);
        var methodNames = typeof(BookingAggregate).GetMethods(
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(m => m.Name)
            .ToArray();
        Assert.DoesNotContain("Confirm", methodNames);
        Assert.DoesNotContain("SetStatus", methodNames);
        Assert.False(BookingLifecycleBoundary.UnrestrictedConfirmationImplemented);
    }

    [Fact]
    public void Create_Requires_NonEmpty_TourDepartureReference()
    {
        Assert.Throws<ArgumentException>(() => BookingAggregate.Create(new TourDepartureReference(Guid.Empty), Now));
    }

    [Fact]
    public void CancelPending_Moves_Pending_To_Cancelled()
    {
        var booking = BookingAggregate.Create(new TourDepartureReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000102")), Now);
        var cancelledAt = Instant.FromUtc(2026, 8, 18, 2, 0);
        booking.CancelPending(cancelledAt);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal(cancelledAt, booking.StatusChangedAt);
    }

    [Fact]
    public void Cancelled_Cannot_Reopen_Or_Cancel_Again()
    {
        var booking = BookingAggregate.Create(new TourDepartureReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000103")), Now);
        booking.CancelPending(Instant.FromUtc(2026, 8, 18, 2, 0));

        Assert.Throws<InvalidOperationException>(
            () => booking.CancelPending(Instant.FromUtc(2026, 8, 18, 3, 0)));
    }

    [Fact]
    public void Lifecycle_Does_Not_Include_Payment_Or_Capacity_Statuses()
    {
        var names = Enum.GetNames<BookingStatus>();
        Assert.Equal(new[] { "Pending", "Confirmed", "Cancelled" }, names);
        Assert.DoesNotContain("Expired", names);
        Assert.DoesNotContain("AwaitingPayment", names);
        Assert.DoesNotContain("Paid", names);
        Assert.DoesNotContain("Refunded", names);
        Assert.DoesNotContain("Held", names);
        Assert.DoesNotContain("Reserved", names);
        Assert.False(BookingLifecycleBoundary.ExpiredStatusImplemented);
        Assert.False(BookingLifecycleBoundary.AwaitingPaymentStatusImplemented);
        Assert.Equal("Confirmed != PaymentSucceeded", BookingLifecycleBoundary.ConfirmedIsNotPaymentSucceeded);
        Assert.Equal("Cancelled != Refunded", BookingLifecycleBoundary.CancelledIsNotRefunded);
        Assert.Equal("BookingStatus != PaymentStatus", BookingLifecycleBoundary.BookingStatusIsNotPaymentStatus);
    }
}
