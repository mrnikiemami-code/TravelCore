using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.Contracts;

public enum FlightOfferOutcome : short
{
    Available = 1,
    Unavailable = 2,
    Changed = 3,
    Unknown = 4,
}

/// <summary>
/// Structural itinerary identity used to bind a commercial offer to a persisted FlightBooking.
/// Derived from FlightBooking — not a client-reconstructed itinerary.
/// </summary>
public sealed class FlightOfferSegmentIdentity : IEquatable<FlightOfferSegmentIdentity>
{
    public FlightOfferSegmentIdentity(
        int journeyOrdinal,
        int segmentOrdinal,
        AirportReference origin,
        AirportReference destination,
        Instant departureAt,
        Instant arrivalAt,
        AirlineReference marketingCarrier,
        AirlineReference? operatingCarrier,
        string? flightNumber)
    {
        if (journeyOrdinal < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(journeyOrdinal), journeyOrdinal, "Journey ordinal must be >= 1.");
        }

        if (segmentOrdinal < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(segmentOrdinal), segmentOrdinal, "Segment ordinal must be >= 1.");
        }

        if (origin.IataCode == destination.IataCode)
        {
            throw new ArgumentException("Segment origin and destination airports must differ.", nameof(destination));
        }

        if (arrivalAt <= departureAt)
        {
            throw new ArgumentException("ArrivalAt must be later than DepartureAt.", nameof(arrivalAt));
        }

        JourneyOrdinal = journeyOrdinal;
        SegmentOrdinal = segmentOrdinal;
        Origin = origin;
        Destination = destination;
        DepartureAt = departureAt;
        ArrivalAt = arrivalAt;
        MarketingCarrier = marketingCarrier;
        OperatingCarrier = operatingCarrier;
        FlightNumber = string.IsNullOrWhiteSpace(flightNumber) ? null : flightNumber.Trim().ToUpperInvariant();
    }

    public int JourneyOrdinal { get; }

    public int SegmentOrdinal { get; }

    public AirportReference Origin { get; }

    public AirportReference Destination { get; }

    public Instant DepartureAt { get; }

    public Instant ArrivalAt { get; }

    public AirlineReference MarketingCarrier { get; }

    public AirlineReference? OperatingCarrier { get; }

    public string? FlightNumber { get; }

    public bool Equals(FlightOfferSegmentIdentity? other)
    {
        if (other is null)
        {
            return false;
        }

        return JourneyOrdinal == other.JourneyOrdinal
            && SegmentOrdinal == other.SegmentOrdinal
            && Origin.IataCode == other.Origin.IataCode
            && Destination.IataCode == other.Destination.IataCode
            && DepartureAt == other.DepartureAt
            && ArrivalAt == other.ArrivalAt
            && MarketingCarrier.IataCode == other.MarketingCarrier.IataCode
            && OperatingCarrierCode(OperatingCarrier) == OperatingCarrierCode(other.OperatingCarrier)
            && FlightNumber == other.FlightNumber;
    }

    public override bool Equals(object? obj) => Equals(obj as FlightOfferSegmentIdentity);

    public override int GetHashCode() =>
        HashCode.Combine(
            HashCode.Combine(
                JourneyOrdinal,
                SegmentOrdinal,
                Origin.IataCode,
                Destination.IataCode,
                DepartureAt,
                ArrivalAt),
            MarketingCarrier.IataCode,
            OperatingCarrierCode(OperatingCarrier),
            FlightNumber);

    private static string? OperatingCarrierCode(AirlineReference? carrier) =>
        carrier?.IataCode;
}

public sealed class FlightPassengerCategoryFare
{
    public FlightPassengerCategoryFare(FlightPassengerCategory category, int passengerCount, MoneyValue amount)
    {
        if (!Enum.IsDefined(category))
        {
            throw new ArgumentOutOfRangeException(nameof(category), category, "FlightPassengerCategory is not controlled.");
        }

        if (passengerCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(passengerCount), passengerCount, "Passenger count must be >= 1.");
        }

        ArgumentNullException.ThrowIfNull(amount);
        Category = category;
        PassengerCount = passengerCount;
        Amount = amount;
    }

    public FlightPassengerCategory Category { get; }

    public int PassengerCount { get; }

    public MoneyValue Amount { get; }
}

public sealed class FlightBaggageAllowanceFact
{
    public const int UnitMaxLength = 8;
    public const int CategoryMaxLength = 32;

    public FlightBaggageAllowanceFact(
        int? quantity = null,
        decimal? weight = null,
        string? unit = null,
        string? category = null,
        FlightPassengerCategory? passengerCategory = null)
    {
        if (quantity is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Baggage quantity cannot be negative.");
        }

        if (weight is < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), weight, "Baggage weight cannot be negative.");
        }

        if (quantity is null && weight is null)
        {
            throw new ArgumentException("Baggage allowance requires quantity or weight.");
        }

        Quantity = quantity;
        Weight = weight;
        Unit = NormalizeOptional(unit, UnitMaxLength);
        Category = NormalizeOptional(category, CategoryMaxLength);
        PassengerCategory = passengerCategory;
    }

    public int? Quantity { get; }

    public decimal? Weight { get; }

    public string? Unit { get; }

    public string? Category { get; }

    public FlightPassengerCategory? PassengerCategory { get; }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Max length is {maxLength}.");
        }

        return trimmed;
    }
}

public sealed class FlightFareRulesFact
{
    public FlightFareRulesFact(
        bool refundable,
        bool changeable,
        Instant? ticketingDeadline = null,
        MoneyValue? cancelPenalty = null,
        MoneyValue? changePenalty = null,
        bool partialRefundRequired = false)
    {
        Refundable = refundable;
        Changeable = changeable;
        TicketingDeadline = ticketingDeadline;
        CancelPenalty = cancelPenalty;
        ChangePenalty = changePenalty;
        PartialRefundRequired = partialRefundRequired;
    }

    public bool Refundable { get; }

    public bool Changeable { get; }

    public Instant? TicketingDeadline { get; }

    public MoneyValue? CancelPenalty { get; }

    public MoneyValue? ChangePenalty { get; }

    public bool PartialRefundRequired { get; }
}

/// <summary>
/// Commercial revalidation request derived from persisted FlightBooking. No passenger PII.
/// </summary>
public sealed class FlightOfferRequest
{
    public FlightOfferRequest(
        Guid flightBookingId,
        FlightTripType tripType,
        IReadOnlyList<FlightOfferSegmentIdentity> segments,
        FlightPassengerCount passengers,
        string? sourceOfferReference = null,
        MoneyValue? previouslyObservedTotal = null)
    {
        if (flightBookingId == Guid.Empty)
        {
            throw new ArgumentException("FlightBookingId is required.", nameof(flightBookingId));
        }

        if (!Enum.IsDefined(tripType))
        {
            throw new ArgumentOutOfRangeException(nameof(tripType), tripType, "FlightTripType is not controlled.");
        }

        ArgumentNullException.ThrowIfNull(segments);
        ArgumentNullException.ThrowIfNull(passengers);
        if (segments.Count == 0)
        {
            throw new ArgumentException("Offer request requires at least one segment.", nameof(segments));
        }

        FlightBookingId = flightBookingId;
        TripType = tripType;
        Segments = segments;
        Passengers = passengers;
        SourceOfferReference = string.IsNullOrWhiteSpace(sourceOfferReference) ? null : sourceOfferReference.Trim();
        PreviouslyObservedTotal = previouslyObservedTotal;
    }

    public Guid FlightBookingId { get; }

    public FlightTripType TripType { get; }

    public IReadOnlyList<FlightOfferSegmentIdentity> Segments { get; }

    public FlightPassengerCount Passengers { get; }

    public string? SourceOfferReference { get; }

    public MoneyValue? PreviouslyObservedTotal { get; }
}

/// <summary>
/// Authoritative source-authored commercial offer. Flight must not invent totals.
/// </summary>
public sealed class FlightOfferSourceResult
{
    public const int SourceOfferReferenceMaxLength = 128;
    public const int CabinMaxLength = 32;
    public const int BookingClassMaxLength = 8;
    public const int FareBasisMaxLength = 16;
    public const int FareFamilyMaxLength = 64;

    private FlightOfferSourceResult(
        FlightOfferOutcome outcome,
        FlightSourceKey sourceKey,
        Instant observedAt,
        string? sourceOfferReference = null,
        Instant? quotedAt = null,
        Instant? offerExpiresAt = null,
        MoneyValue? baseFare = null,
        MoneyValue? taxes = null,
        MoneyValue? fees = null,
        MoneyValue? totalAmount = null,
        IReadOnlyList<FlightOfferSegmentIdentity>? segments = null,
        FlightPassengerCount? passengers = null,
        FlightFareRulesFact? fareRules = null,
        IReadOnlyList<FlightPassengerCategoryFare>? categoryFares = null,
        IReadOnlyList<FlightBaggageAllowanceFact>? baggage = null,
        string? cabin = null,
        string? bookingClass = null,
        string? fareBasis = null,
        string? fareFamily = null)
    {
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Offer outcome is not controlled.");
        }

        Outcome = outcome;
        SourceKey = sourceKey;
        ObservedAt = observedAt;
        SourceOfferReference = sourceOfferReference;
        QuotedAt = quotedAt;
        OfferExpiresAt = offerExpiresAt;
        BaseFare = baseFare;
        Taxes = taxes;
        Fees = fees;
        TotalAmount = totalAmount;
        Segments = segments;
        Passengers = passengers;
        FareRules = fareRules;
        CategoryFares = categoryFares;
        Baggage = baggage;
        Cabin = cabin;
        BookingClass = bookingClass;
        FareBasis = fareBasis;
        FareFamily = fareFamily;
    }

    public static FlightOfferSourceResult Available(
        FlightSourceKey sourceKey,
        string sourceOfferReference,
        Instant quotedAt,
        Instant offerExpiresAt,
        MoneyValue baseFare,
        MoneyValue taxes,
        MoneyValue fees,
        MoneyValue totalAmount,
        IReadOnlyList<FlightOfferSegmentIdentity> segments,
        FlightPassengerCount passengers,
        FlightFareRulesFact fareRules,
        Instant observedAt,
        IReadOnlyList<FlightPassengerCategoryFare>? categoryFares = null,
        IReadOnlyList<FlightBaggageAllowanceFact>? baggage = null,
        string? cabin = null,
        string? bookingClass = null,
        string? fareBasis = null,
        string? fareFamily = null)
    {
        if (string.IsNullOrWhiteSpace(sourceOfferReference))
        {
            throw new ArgumentException("SourceOfferReference is required.", nameof(sourceOfferReference));
        }

        var offerRef = sourceOfferReference.Trim();
        if (offerRef.Length > SourceOfferReferenceMaxLength)
        {
            throw new ArgumentException(
                $"SourceOfferReference max length is {SourceOfferReferenceMaxLength}.",
                nameof(sourceOfferReference));
        }

        ArgumentNullException.ThrowIfNull(baseFare);
        ArgumentNullException.ThrowIfNull(taxes);
        ArgumentNullException.ThrowIfNull(fees);
        ArgumentNullException.ThrowIfNull(totalAmount);
        ArgumentNullException.ThrowIfNull(segments);
        ArgumentNullException.ThrowIfNull(passengers);
        ArgumentNullException.ThrowIfNull(fareRules);
        if (quotedAt == default)
        {
            throw new ArgumentException("QuotedAt cannot be default.", nameof(quotedAt));
        }

        if (offerExpiresAt == default)
        {
            throw new ArgumentException("OfferExpiresAt cannot be default.", nameof(offerExpiresAt));
        }

        if (segments.Count == 0)
        {
            throw new ArgumentException("Available offer must cover at least one segment.", nameof(segments));
        }

        return new FlightOfferSourceResult(
            FlightOfferOutcome.Available,
            sourceKey,
            observedAt,
            offerRef,
            quotedAt,
            offerExpiresAt,
            baseFare,
            taxes,
            fees,
            totalAmount,
            segments,
            passengers,
            fareRules,
            categoryFares,
            baggage,
            NormalizeOptional(cabin, CabinMaxLength),
            NormalizeOptional(bookingClass, BookingClassMaxLength),
            NormalizeOptional(fareBasis, FareBasisMaxLength),
            NormalizeOptional(fareFamily, FareFamilyMaxLength));
    }

    public static FlightOfferSourceResult Unavailable(FlightSourceKey sourceKey, Instant observedAt) =>
        new(FlightOfferOutcome.Unavailable, sourceKey, observedAt);

    public static FlightOfferSourceResult Changed(FlightSourceKey sourceKey, Instant observedAt) =>
        new(FlightOfferOutcome.Changed, sourceKey, observedAt);

    public static FlightOfferSourceResult Unknown(FlightSourceKey sourceKey, Instant observedAt) =>
        new(FlightOfferOutcome.Unknown, sourceKey, observedAt);

    public FlightOfferOutcome Outcome { get; }

    public FlightSourceKey SourceKey { get; }

    public Instant ObservedAt { get; }

    public string? SourceOfferReference { get; }

    public Instant? QuotedAt { get; }

    public Instant? OfferExpiresAt { get; }

    public MoneyValue? BaseFare { get; }

    public MoneyValue? Taxes { get; }

    public MoneyValue? Fees { get; }

    public MoneyValue? TotalAmount { get; }

    public IReadOnlyList<FlightOfferSegmentIdentity>? Segments { get; }

    public FlightPassengerCount? Passengers { get; }

    public FlightFareRulesFact? FareRules { get; }

    public IReadOnlyList<FlightPassengerCategoryFare>? CategoryFares { get; }

    public IReadOnlyList<FlightBaggageAllowanceFact>? Baggage { get; }

    public string? Cabin { get; }

    public string? BookingClass { get; }

    public string? FareBasis { get; }

    public string? FareFamily { get; }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Max length is {maxLength}.");
        }

        return trimmed;
    }
}
