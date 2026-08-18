using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.HotelBooking.Contracts;

public sealed class HotelReservationGuestFact
{
    public HotelReservationGuestFact(string givenName, string familyName, bool isLeadGuest, int adultOrChild)
    {
        if (string.IsNullOrWhiteSpace(givenName))
        {
            throw new ArgumentException("GivenName is required.", nameof(givenName));
        }

        if (string.IsNullOrWhiteSpace(familyName))
        {
            throw new ArgumentException("FamilyName is required.", nameof(familyName));
        }

        GivenName = givenName.Trim();
        FamilyName = familyName.Trim();
        IsLeadGuest = isLeadGuest;
        AdultOrChild = adultOrChild;
    }

    public string GivenName { get; }

    public string FamilyName { get; }

    public bool IsLeadGuest { get; }

    public int AdultOrChild { get; }
}

public sealed class HotelReservationRoomRequest
{
    public HotelReservationRoomRequest(
        Guid roomReservationId,
        int adultCount,
        IReadOnlyList<int> childAgesAtCheckIn,
        IReadOnlyList<HotelReservationGuestFact> guests)
    {
        if (roomReservationId == Guid.Empty)
        {
            throw new ArgumentException("RoomReservationId is required.", nameof(roomReservationId));
        }

        ArgumentNullException.ThrowIfNull(childAgesAtCheckIn);
        ArgumentNullException.ThrowIfNull(guests);
        RoomReservationId = roomReservationId;
        AdultCount = adultCount;
        ChildAgesAtCheckIn = childAgesAtCheckIn;
        Guests = guests;
    }

    public Guid RoomReservationId { get; }

    public int AdultCount { get; }

    public IReadOnlyList<int> ChildAgesAtCheckIn { get; }

    public IReadOnlyList<HotelReservationGuestFact> Guests { get; }
}

public sealed class HotelReservationRequest
{
    public HotelReservationRequest(
        Guid hotelBookingId,
        Guid placeId,
        LocalDate checkInDate,
        LocalDate checkOutDate,
        IReadOnlyList<HotelReservationRoomRequest> rooms,
        Guid rateOfferSnapshotId,
        MoneyValue total,
        string? availabilityHoldReference,
        string idempotencyKey)
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
        ArgumentNullException.ThrowIfNull(total);
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("IdempotencyKey is required.", nameof(idempotencyKey));
        }

        HotelBookingId = hotelBookingId;
        PlaceId = placeId;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        Rooms = rooms;
        RateOfferSnapshotId = rateOfferSnapshotId;
        Total = total;
        AvailabilityHoldReference = availabilityHoldReference;
        IdempotencyKey = idempotencyKey.Trim();
    }

    public Guid HotelBookingId { get; }

    public Guid PlaceId { get; }

    public LocalDate CheckInDate { get; }

    public LocalDate CheckOutDate { get; }

    public IReadOnlyList<HotelReservationRoomRequest> Rooms { get; }

    public Guid RateOfferSnapshotId { get; }

    public MoneyValue Total { get; }

    public string? AvailabilityHoldReference { get; }

    public string IdempotencyKey { get; }
}

public enum HotelReservationSourceOutcome
{
    Complete = 1,
    Failed = 2,
    Partial = 3,
    Timeout = 4,
    Unknown = 5,
}

public sealed class HotelReservationSourceResult
{
    public HotelReservationSourceResult(
        HotelReservationSourceOutcome outcome,
        string? sourceReservationReference,
        string? supplierConfirmationCode,
        IReadOnlyList<Guid> confirmedRoomReservationIds,
        MoneyValue? reportedTotal,
        bool? cancellationTermsMatch)
    {
        ArgumentNullException.ThrowIfNull(confirmedRoomReservationIds);
        Outcome = outcome;
        SourceReservationReference = sourceReservationReference;
        SupplierConfirmationCode = supplierConfirmationCode;
        ConfirmedRoomReservationIds = confirmedRoomReservationIds;
        ReportedTotal = reportedTotal;
        CancellationTermsMatch = cancellationTermsMatch;
    }

    public HotelReservationSourceOutcome Outcome { get; }

    public string? SourceReservationReference { get; }

    public string? SupplierConfirmationCode { get; }

    public IReadOnlyList<Guid> ConfirmedRoomReservationIds { get; }

    public MoneyValue? ReportedTotal { get; }

    public bool? CancellationTermsMatch { get; }
}

public enum HotelReservationQueryStatus
{
    Confirmed = 1,
    NotCreated = 2,
    Cancelled = 3,
    PendingUnknown = 4,
    NotFound = 5,
}

public sealed class HotelReservationQueryResult
{
    public HotelReservationQueryResult(
        HotelReservationQueryStatus status,
        string? sourceReservationReference,
        IReadOnlyList<Guid> confirmedRoomReservationIds,
        MoneyValue? reportedTotal)
    {
        ArgumentNullException.ThrowIfNull(confirmedRoomReservationIds);
        Status = status;
        SourceReservationReference = sourceReservationReference;
        ConfirmedRoomReservationIds = confirmedRoomReservationIds;
        ReportedTotal = reportedTotal;
    }

    public HotelReservationQueryStatus Status { get; }

    public string? SourceReservationReference { get; }

    public IReadOnlyList<Guid> ConfirmedRoomReservationIds { get; }

    public MoneyValue? ReportedTotal { get; }
}
