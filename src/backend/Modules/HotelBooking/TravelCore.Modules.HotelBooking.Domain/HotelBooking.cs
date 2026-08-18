using NodaTime;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Hotel stay transaction aggregate (TC-P21-T002 / P21-R2).
/// One HotelPlaceReference, LocalDate stay, one or more RoomReservations, room-assigned guests.
/// No HotelBookingStatus / availability / supplier / rate / payment in T002.
/// </summary>
public sealed class HotelBooking
{
    private readonly List<RoomReservation> _rooms = [];

    private HotelBooking()
    {
        Place = default;
        Contact = null!;
    }

    private HotelBooking(
        HotelBookingId id,
        HotelPlaceReference place,
        LocalDate checkInDate,
        LocalDate checkOutDate,
        HotelBookingContactSnapshot contact)
    {
        Id = id;
        Place = place;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        Contact = contact;
    }

    public HotelBookingId Id { get; private set; }

    public HotelPlaceReference Place { get; private set; }

    public LocalDate CheckInDate { get; private set; }

    public LocalDate CheckOutDate { get; private set; }

    public int Nights => Period.Between(CheckInDate, CheckOutDate, PeriodUnits.Days).Days;

    public HotelBookingContactSnapshot Contact { get; private set; }

    public IReadOnlyList<RoomReservation> Rooms => _rooms;

    public int RoomCount => _rooms.Count;

    public IEnumerable<HotelBookingGuest> Guests => _rooms.SelectMany(r => r.Guests);

    public int GuestCount => _rooms.Sum(r => r.GuestCount);

    public int AdultCount => _rooms.Sum(r => r.AdultCount);

    public int ChildCount => _rooms.Sum(r => r.ChildCount);

    public HotelBookingGuest LeadGuest =>
        Guests.Single(g => g.IsLeadGuest);

    public static HotelBooking Create(
        HotelPlaceReference place,
        LocalDate checkInDate,
        LocalDate checkOutDate,
        HotelBookingContactSnapshot contact,
        IReadOnlyList<RoomReservationSpecification> rooms)
    {
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentNullException.ThrowIfNull(rooms);

        if (checkOutDate <= checkInDate)
        {
            throw new ArgumentException(
                "CheckOutDate must be later than CheckInDate.",
                nameof(checkOutDate));
        }

        if (rooms.Count == 0)
        {
            throw new ArgumentException("HotelBooking requires at least one RoomReservation.", nameof(rooms));
        }

        var booking = new HotelBooking(
            HotelBookingId.New(),
            place,
            checkInDate,
            checkOutDate,
            contact);

        var ordinal = 1;
        foreach (var roomSpec in rooms)
        {
            ArgumentNullException.ThrowIfNull(roomSpec);
            booking._rooms.Add(RoomReservation.Create(booking.Id, ordinal, roomSpec.Guests));
            ordinal++;
        }

        var leadCount = booking.Guests.Count(g => g.IsLeadGuest);
        if (leadCount != 1)
        {
            throw new ArgumentException(
                "HotelBooking requires exactly one LeadGuest.",
                nameof(rooms));
        }

        return booking;
    }
}
