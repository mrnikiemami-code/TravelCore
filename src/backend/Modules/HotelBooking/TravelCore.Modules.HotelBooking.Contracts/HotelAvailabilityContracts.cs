using NodaTime;

namespace TravelCore.Modules.HotelBooking.Contracts;

public sealed class HotelAvailabilityRoomRequest
{
    public HotelAvailabilityRoomRequest(
        Guid roomReservationId,
        int adultCount,
        IReadOnlyList<int> childAgesAtCheckIn)
    {
        if (roomReservationId == Guid.Empty)
        {
            throw new ArgumentException("RoomReservationId is required.", nameof(roomReservationId));
        }

        ArgumentNullException.ThrowIfNull(childAgesAtCheckIn);
        RoomReservationId = roomReservationId;
        AdultCount = adultCount;
        ChildAgesAtCheckIn = childAgesAtCheckIn;
    }

    public Guid RoomReservationId { get; }

    public int AdultCount { get; }

    public IReadOnlyList<int> ChildAgesAtCheckIn { get; }
}

public sealed class HotelAvailabilityRequest
{
    public HotelAvailabilityRequest(
        Guid hotelBookingId,
        Guid placeId,
        LocalDate checkInDate,
        LocalDate checkOutDate,
        IReadOnlyList<HotelAvailabilityRoomRequest> rooms)
    {
        if (hotelBookingId == Guid.Empty)
        {
            throw new ArgumentException("HotelBookingId is required.", nameof(hotelBookingId));
        }

        if (placeId == Guid.Empty)
        {
            throw new ArgumentException("PlaceId is required.", nameof(placeId));
        }

        ArgumentNullException.ThrowIfNull(rooms);
        HotelBookingId = hotelBookingId;
        PlaceId = placeId;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        Rooms = rooms;
    }

    public Guid HotelBookingId { get; }

    public Guid PlaceId { get; }

    public LocalDate CheckInDate { get; }

    public LocalDate CheckOutDate { get; }

    public IReadOnlyList<HotelAvailabilityRoomRequest> Rooms { get; }
}

public enum HotelAvailabilitySourceOutcome
{
    Complete = 1,
    Unavailable = 2,
    Partial = 3,
    Timeout = 4,
    Unknown = 5,
}

public sealed class HotelAvailabilityRoomHoldResult
{
    public HotelAvailabilityRoomHoldResult(Guid roomReservationId, string selectionReference)
    {
        if (roomReservationId == Guid.Empty)
        {
            throw new ArgumentException("RoomReservationId is required.", nameof(roomReservationId));
        }

        if (string.IsNullOrWhiteSpace(selectionReference))
        {
            throw new ArgumentException("Selection reference is required.", nameof(selectionReference));
        }

        RoomReservationId = roomReservationId;
        SelectionReference = selectionReference.Trim();
    }

    public Guid RoomReservationId { get; }

    public string SelectionReference { get; }
}

public sealed class HotelAvailabilityHoldSourceResult
{
    public HotelAvailabilityHoldSourceResult(
        HotelAvailabilitySourceOutcome outcome,
        string? sourceHoldReference,
        Instant? expiresAt,
        IReadOnlyList<HotelAvailabilityRoomHoldResult> rooms)
    {
        ArgumentNullException.ThrowIfNull(rooms);
        Outcome = outcome;
        SourceHoldReference = sourceHoldReference;
        ExpiresAt = expiresAt;
        Rooms = rooms;
    }

    public HotelAvailabilitySourceOutcome Outcome { get; }

    public string? SourceHoldReference { get; }

    public Instant? ExpiresAt { get; }

    public IReadOnlyList<HotelAvailabilityRoomHoldResult> Rooms { get; }
}

public enum HotelAvailabilityHoldQueryStatus
{
    Active = 1,
    Released = 2,
    Expired = 3,
    PendingUnknown = 4,
    NotFound = 5,
}

public sealed class HotelAvailabilityHoldQueryResult
{
    public HotelAvailabilityHoldQueryResult(HotelAvailabilityHoldQueryStatus status, Instant? expiresAt)
    {
        Status = status;
        ExpiresAt = expiresAt;
    }

    public HotelAvailabilityHoldQueryStatus Status { get; }

    public Instant? ExpiresAt { get; }
}
