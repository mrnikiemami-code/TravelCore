using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Modules.Booking.UnitTests;

public sealed class BookingPeopleTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 9, 0);
    private static readonly TourDepartureReference Departure =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000401"));

    [Fact]
    public void Contact_Snapshot_Requires_Name_Email_Or_Phone_And_Is_Not_Party()
    {
        var contact = BookingContactSnapshot.Create("علی رضایی", "Ali@Example.com", "09120000000");
        Assert.Equal("علی رضایی", contact.DisplayName);
        Assert.Equal("ALI@EXAMPLE.COM", contact.NormalizedEmail);
        Assert.Throws<ArgumentException>(() => BookingContactSnapshot.Create(null, null, null));
        Assert.Equal("BookingContactSnapshot != Party", BookingPeopleBoundary.BookingContactSnapshotIsNotParty);
        Assert.Equal("BookingContactSnapshot != Identity Account", BookingPeopleBoundary.BookingContactSnapshotIsNotIdentityAccount);
    }

    [Fact]
    public void Passenger_Requires_Names_And_Controlled_Category()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        var passenger = booking.AddPassenger("فاطمه", "محمدی", TravelerCategory.Adult, activeHeldSeatCount: null);
        Assert.Equal("فاطمه", passenger.GivenName);
        Assert.Equal(TravelerCategory.Adult, passenger.Category);
        Assert.Throws<ArgumentException>(() => booking.AddPassenger(" ", "محمدی", TravelerCategory.Child, null));
        Assert.Equal(
            new[] { "Adult", "Child", "Infant" },
            Enum.GetNames<TravelerCategory>());
        Assert.False(BookingPeopleBoundary.BirthDateImplemented);
        Assert.Equal("PlannerTravelerComposition != BookingPassenger", BookingPeopleBoundary.PlannerTravelerCompositionIsNotBookingPassenger);
        Assert.Equal("BookingPassenger != Party Person Master", BookingPeopleBoundary.BookingPassengerIsNotPartyPersonMaster);
    }

    [Fact]
    public void Multiple_Passengers_Are_Booking_Owned_Not_Party_Records()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        booking.AddPassenger("A", "One", TravelerCategory.Adult, 4);
        booking.AddPassenger("B", "Two", TravelerCategory.Child, 4);
        Assert.Equal(2, booking.PassengerCount);
        Assert.Null(booking.PartyReference);
        Assert.Equal("BookingPassenger != Party Person Master", BookingPeopleBoundary.BookingPassengerIsNotPartyPersonMaster);
    }

    [Fact]
    public void Passenger_Count_Cannot_Exceed_Active_Held_Seats()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        booking.AddPassenger("A", "One", TravelerCategory.Adult, 1);
        Assert.Throws<InvalidOperationException>(
            () => booking.AddPassenger("B", "Two", TravelerCategory.Adult, 1));
        booking.RemovePassenger(booking.Passengers[0].Id);
        Assert.Equal(0, booking.PassengerCount);
    }

    [Fact]
    public void BookingStatus_Is_Unchanged_By_People_Facts()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        booking.SetContact(BookingContactSnapshot.Create("Booker", "b@example.com"));
        booking.AddPassenger("A", "One", TravelerCategory.Adult, null);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Equal(new[] { "Pending", "Confirmed", "Cancelled" }, Enum.GetNames<BookingStatus>());
        Assert.Null(typeof(BookingAggregate).GetMethod("Confirm"));
    }
}
