using NodaTime;
using TravelCore.Modules.HotelBooking.Domain;
using Xunit;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.UnitTests;

public sealed class HotelBookingStayTests
{
    private static readonly HotelPlaceReference Place =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000021"));

    private static HotelBookingContactSnapshot Contact() =>
        HotelBookingContactSnapshot.Create(email: "lead@example.com");

    private static HotelBookingGuestSpecification Adult(string given, string family, bool lead) =>
        new(given, family, HotelGuestCategory.Adult, lead);

    private static HotelBookingGuestSpecification Child(string given, string family, int age, bool lead) =>
        new(given, family, HotelGuestCategory.Child, lead, age);

    private static Stay OneRoomBooking(LocalDate checkIn, LocalDate checkOut) =>
        Stay.Create(
            Place,
            checkIn,
            checkOut,
            Contact(),
            [new RoomReservationSpecification([Adult("Ada", "Lovelace", lead: true)])]);

    [Fact]
    public void Valid_One_Night_Stay_Has_Nights_1()
    {
        var booking = OneRoomBooking(new LocalDate(2026, 8, 18), new LocalDate(2026, 8, 19));
        Assert.Equal(1, booking.Nights);
        Assert.Equal(1, booking.RoomCount);
        Assert.Equal(1, booking.GuestCount);
        Assert.Equal(1, booking.AdultCount);
        Assert.Equal(0, booking.ChildCount);
    }

    [Fact]
    public void Valid_Multi_Night_Stay_Derives_Nights()
    {
        var booking = OneRoomBooking(new LocalDate(2026, 8, 18), new LocalDate(2026, 8, 21));
        Assert.Equal(3, booking.Nights);
    }

    [Fact]
    public void Same_Day_Stay_Is_Rejected()
    {
        var day = new LocalDate(2026, 8, 18);
        var ex = Assert.Throws<ArgumentException>(() => OneRoomBooking(day, day));
        Assert.Contains("CheckOutDate", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Checkout_Before_Checkin_Is_Rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            OneRoomBooking(new LocalDate(2026, 8, 19), new LocalDate(2026, 8, 18)));
    }

    [Fact]
    public void Multiple_Rooms_Are_Accepted_With_Room_Level_Occupancy()
    {
        var booking = Stay.Create(
            Place,
            new LocalDate(2026, 8, 18),
            new LocalDate(2026, 8, 20),
            Contact(),
            [
                new RoomReservationSpecification(
                [
                    Adult("Ada", "Lovelace", lead: true),
                    Child("Ann", "Lovelace", age: 8, lead: false),
                ]),
                new RoomReservationSpecification([Adult("Alan", "Turing", lead: false)]),
            ]);

        Assert.Equal(2, booking.RoomCount);
        Assert.Equal(3, booking.GuestCount);
        Assert.Equal(2, booking.AdultCount);
        Assert.Equal(1, booking.ChildCount);
        Assert.Equal(2, booking.Rooms[0].GuestCount);
        Assert.Equal(1, booking.Rooms[0].AdultCount);
        Assert.Equal(1, booking.Rooms[0].ChildCount);
        Assert.Equal(1, booking.Rooms[1].AdultCount);
        Assert.Equal(booking.Rooms[0].Guests.Single(g => g.IsLeadGuest).Id, booking.LeadGuest.Id);
    }

    [Fact]
    public void Zero_Rooms_Are_Rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            Stay.Create(
                Place,
                new LocalDate(2026, 8, 18),
                new LocalDate(2026, 8, 19),
                Contact(),
                []));
    }

    [Fact]
    public void Room_With_Zero_Guests_Is_Rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            Stay.Create(
                Place,
                new LocalDate(2026, 8, 18),
                new LocalDate(2026, 8, 19),
                Contact(),
                [new RoomReservationSpecification([])]));
    }

    [Fact]
    public void Guest_Ids_Are_Unique_Across_Rooms()
    {
        var booking = Stay.Create(
            Place,
            new LocalDate(2026, 8, 18),
            new LocalDate(2026, 8, 19),
            Contact(),
            [
                new RoomReservationSpecification([Adult("Ada", "Lovelace", lead: true)]),
                new RoomReservationSpecification([Adult("Alan", "Turing", lead: false)]),
            ]);

        var ids = booking.Guests.Select(g => g.Id).ToArray();
        Assert.Equal(ids.Length, ids.Distinct().Count());
        Assert.Equal(booking.Rooms[0].Id, booking.Rooms[0].Guests[0].RoomReservationId);
        Assert.NotEqual(booking.Rooms[0].Guests[0].RoomReservationId, booking.Rooms[1].Guests[0].RoomReservationId);
    }

    [Fact]
    public void Exactly_One_Lead_Guest_Is_Required()
    {
        Assert.Throws<ArgumentException>(() =>
            Stay.Create(
                Place,
                new LocalDate(2026, 8, 18),
                new LocalDate(2026, 8, 19),
                Contact(),
                [new RoomReservationSpecification([Adult("Ada", "Lovelace", lead: false)])]));

        Assert.Throws<ArgumentException>(() =>
            Stay.Create(
                Place,
                new LocalDate(2026, 8, 18),
                new LocalDate(2026, 8, 19),
                Contact(),
                [
                    new RoomReservationSpecification([Adult("Ada", "Lovelace", lead: true)]),
                    new RoomReservationSpecification([Adult("Alan", "Turing", lead: true)]),
                ]));
    }

    [Fact]
    public void Child_Requires_AgeAtCheckIn_And_Rejects_Impossible_Ages()
    {
        var withChild = Stay.Create(
            Place,
            new LocalDate(2026, 8, 18),
            new LocalDate(2026, 8, 19),
            Contact(),
            [
                new RoomReservationSpecification(
                [
                    Adult("Ada", "Lovelace", lead: true),
                    Child("Ann", "Lovelace", age: 8, lead: false),
                ]),
            ]);
        Assert.Equal(8, withChild.Rooms[0].Guests.Single(g => g.Category == HotelGuestCategory.Child).AgeAtCheckIn!.Value.Years);

        var missingAge = Assert.Throws<ArgumentException>(() =>
            Stay.Create(
                Place,
                new LocalDate(2026, 8, 18),
                new LocalDate(2026, 8, 19),
                Contact(),
                [
                    new RoomReservationSpecification(
                    [
                        Adult("Ada", "Lovelace", lead: true),
                        new HotelBookingGuestSpecification("Ann", "Lovelace", HotelGuestCategory.Child, false),
                    ]),
                ]));
        Assert.Contains("AgeAtCheckIn", missingAge.Message, StringComparison.Ordinal);

        Assert.Throws<ArgumentOutOfRangeException>(() => new HotelGuestAgeAtCheckIn(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HotelGuestAgeAtCheckIn(121));
    }

    [Fact]
    public void Adult_Does_Not_Require_AgeAtCheckIn()
    {
        var booking = OneRoomBooking(new LocalDate(2026, 8, 18), new LocalDate(2026, 8, 19));
        Assert.Null(booking.LeadGuest.AgeAtCheckIn);
        Assert.Equal(HotelGuestCategory.Adult, booking.LeadGuest.Category);
    }

    [Fact]
    public void Contact_Requires_Email_Or_Phone()
    {
        Assert.Throws<ArgumentException>(() => HotelBookingContactSnapshot.Create());
        var byPhone = HotelBookingContactSnapshot.Create(phone: "+989121234567");
        Assert.Equal("+989121234567", byPhone.Phone);
        Assert.Null(byPhone.Email);
    }

    [Fact]
    public void Guest_And_Contact_Have_No_Document_Fields()
    {
        var guestNames = typeof(HotelBookingGuest).GetProperties().Select(p => p.Name);
        var contactNames = typeof(HotelBookingContactSnapshot).GetProperties().Select(p => p.Name);
        string[] forbidden =
        [
            "Passport", "NationalId", "CardNumber", "CVV", "Health", "VisaDocument", "DocumentScan", "BirthDate",
        ];
        foreach (var name in forbidden)
        {
            Assert.DoesNotContain(name, guestNames, StringComparer.OrdinalIgnoreCase);
            Assert.DoesNotContain(name, contactNames, StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Duplicate_Guest_Names_Are_Allowed()
    {
        var booking = Stay.Create(
            Place,
            new LocalDate(2026, 8, 18),
            new LocalDate(2026, 8, 19),
            Contact(),
            [
                new RoomReservationSpecification(
                [
                    Adult("Ali", "Karimi", lead: true),
                    Adult("Ali", "Karimi", lead: false),
                ]),
            ]);
        Assert.Equal(2, booking.GuestCount);
    }

    [Fact]
    public void Child_May_Be_LeadGuest_Because_Adult_Lead_Is_Not_Invented()
    {
        var booking = Stay.Create(
            Place,
            new LocalDate(2026, 8, 18),
            new LocalDate(2026, 8, 19),
            Contact(),
            [
                new RoomReservationSpecification(
                [
                    Child("Ann", "Lovelace", age: 10, lead: true),
                    Adult("Ada", "Lovelace", lead: false),
                ]),
            ]);
        Assert.Equal(HotelGuestCategory.Child, booking.LeadGuest.Category);
        Assert.Equal(10, booking.LeadGuest.AgeAtCheckIn!.Value.Years);
    }

    [Fact]
    public void HotelBooking_Identities_Use_Uuidv7()
    {
        Assert.Equal(7, HotelBookingId.New().Value.Version);
        Assert.Equal(7, RoomReservationId.New().Value.Version);
        Assert.Equal(7, HotelBookingGuestId.New().Value.Version);
    }
}
