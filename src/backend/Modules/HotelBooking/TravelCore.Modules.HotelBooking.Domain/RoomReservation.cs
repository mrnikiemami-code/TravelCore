namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// One booked room position in a HotelBooking. Not a hotel physical room number.
/// Guest assignment is the occupancy truth for T002, not availability/rate eligibility.
/// </summary>
public sealed class RoomReservation
{
    private readonly List<HotelBookingGuest> _guests = [];

    private RoomReservation()
    {
    }

    private RoomReservation(RoomReservationId id, HotelBookingId hotelBookingId, int ordinal)
    {
        Id = id;
        HotelBookingId = hotelBookingId;
        Ordinal = ordinal;
    }

    public RoomReservationId Id { get; private set; }

    public HotelBookingId HotelBookingId { get; private set; }

    public int Ordinal { get; private set; }

    public IReadOnlyList<HotelBookingGuest> Guests => _guests;

    public int GuestCount => _guests.Count;

    public int AdultCount => _guests.Count(g => g.Category == HotelGuestCategory.Adult);

    public int ChildCount => _guests.Count(g => g.Category == HotelGuestCategory.Child);

    internal static RoomReservation Create(
        HotelBookingId hotelBookingId,
        int ordinal,
        IReadOnlyList<HotelBookingGuestSpecification> guests)
    {
        ArgumentNullException.ThrowIfNull(guests);
        if (ordinal < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, "Room ordinal must be >= 1.");
        }

        if (guests.Count == 0)
        {
            throw new ArgumentException("RoomReservation requires at least one assigned guest.", nameof(guests));
        }

        var room = new RoomReservation(RoomReservationId.New(), hotelBookingId, ordinal);
        foreach (var spec in guests)
        {
            room._guests.Add(HotelBookingGuest.Create(hotelBookingId, room.Id, spec));
        }

        return room;
    }
}
