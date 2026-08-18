using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// Occupancy/structure rate request derived from HotelBooking. No guest PII.
/// </summary>
public sealed class HotelRateOfferRoomRequest
{
    public HotelRateOfferRoomRequest(
        Guid roomReservationId,
        int adultCount,
        IReadOnlyList<int> childAgesAtCheckIn,
        string? availabilitySelectionReference = null)
    {
        if (roomReservationId == Guid.Empty)
        {
            throw new ArgumentException("RoomReservationId is required.", nameof(roomReservationId));
        }

        ArgumentNullException.ThrowIfNull(childAgesAtCheckIn);
        RoomReservationId = roomReservationId;
        AdultCount = adultCount;
        ChildAgesAtCheckIn = childAgesAtCheckIn;
        AvailabilitySelectionReference = string.IsNullOrWhiteSpace(availabilitySelectionReference)
            ? null
            : availabilitySelectionReference.Trim();
    }

    public Guid RoomReservationId { get; }

    public int AdultCount { get; }

    public IReadOnlyList<int> ChildAgesAtCheckIn { get; }

    public string? AvailabilitySelectionReference { get; }
}

public sealed class HotelRateOfferRequest
{
    public HotelRateOfferRequest(
        Guid hotelBookingId,
        Guid placeId,
        LocalDate checkInDate,
        LocalDate checkOutDate,
        IReadOnlyList<HotelRateOfferRoomRequest> rooms)
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

    public IReadOnlyList<HotelRateOfferRoomRequest> Rooms { get; }
}

public sealed class HotelRateOfferRoomLine
{
    public HotelRateOfferRoomLine(
        Guid roomReservationId,
        MoneyValue? amount,
        string? availabilitySelectionReference = null,
        string? sourceRateReference = null,
        string? boardBasisCode = null)
    {
        if (roomReservationId == Guid.Empty)
        {
            throw new ArgumentException("RoomReservationId is required.", nameof(roomReservationId));
        }

        RoomReservationId = roomReservationId;
        Amount = amount;
        AvailabilitySelectionReference = NormalizeOptional(availabilitySelectionReference);
        SourceRateReference = NormalizeOptional(sourceRateReference);
        BoardBasisCode = NormalizeOptional(boardBasisCode);
    }

    public Guid RoomReservationId { get; }

    public MoneyValue? Amount { get; }

    public string? AvailabilitySelectionReference { get; }

    public string? SourceRateReference { get; }

    public string? BoardBasisCode { get; }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class HotelRateOfferPenaltyRule
{
    public HotelRateOfferPenaltyRule(Instant effectiveFrom, Instant? effectiveUntil, MoneyValue penalty)
    {
        ArgumentNullException.ThrowIfNull(penalty);
        if (effectiveFrom == default)
        {
            throw new ArgumentException("EffectiveFrom cannot be default.", nameof(effectiveFrom));
        }

        EffectiveFrom = effectiveFrom;
        EffectiveUntil = effectiveUntil;
        Penalty = penalty;
    }

    public Instant EffectiveFrom { get; }

    public Instant? EffectiveUntil { get; }

    public MoneyValue Penalty { get; }
}

public sealed class HotelRateOfferChargeComponent
{
    public HotelRateOfferChargeComponent(string code, MoneyValue amount)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Charge component code is required.", nameof(code));
        }

        ArgumentNullException.ThrowIfNull(amount);
        Code = code.Trim();
        Amount = amount;
    }

    public string Code { get; }

    public MoneyValue Amount { get; }
}

/// <summary>
/// Authoritative source-authored commercial offer. HotelBooking must not invent totals.
/// </summary>
public sealed class HotelRateOfferSourceResult
{
    public HotelRateOfferSourceResult(
        string sourceOfferReference,
        Instant quotedAt,
        Instant? offerExpiresAt,
        MoneyValue total,
        IReadOnlyList<HotelRateOfferRoomLine> rooms,
        IReadOnlyList<HotelRateOfferPenaltyRule> penaltyRules,
        MoneyValue? payableNow = null,
        MoneyValue? payableAtProperty = null,
        IReadOnlyList<HotelRateOfferChargeComponent>? charges = null,
        string? propertyTimeZoneId = null,
        string? publicExplanation = null)
    {
        if (string.IsNullOrWhiteSpace(sourceOfferReference))
        {
            throw new ArgumentException("SourceOfferReference is required.", nameof(sourceOfferReference));
        }

        ArgumentNullException.ThrowIfNull(total);
        ArgumentNullException.ThrowIfNull(rooms);
        ArgumentNullException.ThrowIfNull(penaltyRules);
        SourceOfferReference = sourceOfferReference.Trim();
        QuotedAt = quotedAt;
        OfferExpiresAt = offerExpiresAt;
        Total = total;
        Rooms = rooms;
        PenaltyRules = penaltyRules;
        PayableNow = payableNow;
        PayableAtProperty = payableAtProperty;
        Charges = charges;
        PropertyTimeZoneId = string.IsNullOrWhiteSpace(propertyTimeZoneId) ? null : propertyTimeZoneId.Trim();
        PublicExplanation = string.IsNullOrWhiteSpace(publicExplanation) ? null : publicExplanation.Trim();
    }

    public string SourceOfferReference { get; }

    public Instant QuotedAt { get; }

    public Instant? OfferExpiresAt { get; }

    public MoneyValue Total { get; }

    public IReadOnlyList<HotelRateOfferRoomLine> Rooms { get; }

    public IReadOnlyList<HotelRateOfferPenaltyRule> PenaltyRules { get; }

    public MoneyValue? PayableNow { get; }

    public MoneyValue? PayableAtProperty { get; }

    public IReadOnlyList<HotelRateOfferChargeComponent>? Charges { get; }

    public string? PropertyTimeZoneId { get; }

    public string? PublicExplanation { get; }
}
