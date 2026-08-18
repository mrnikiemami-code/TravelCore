using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Passenger facts allowed on the reservation port (T005). GivenName, FamilyName, Category only.
/// </summary>
public sealed class FlightReservationPassengerFact : IEquatable<FlightReservationPassengerFact>
{
    public FlightReservationPassengerFact(
        string givenName,
        string familyName,
        FlightPassengerCategory category)
    {
        if (string.IsNullOrWhiteSpace(givenName))
        {
            throw new ArgumentException("GivenName is required.", nameof(givenName));
        }

        if (string.IsNullOrWhiteSpace(familyName))
        {
            throw new ArgumentException("FamilyName is required.", nameof(familyName));
        }

        if (!Enum.IsDefined(category))
        {
            throw new ArgumentOutOfRangeException(nameof(category), category, "FlightPassengerCategory is not controlled.");
        }

        GivenName = givenName.Trim();
        FamilyName = familyName.Trim();
        Category = category;
    }

    public string GivenName { get; }

    public string FamilyName { get; }

    public FlightPassengerCategory Category { get; }

    public bool Equals(FlightReservationPassengerFact? other)
    {
        if (other is null)
        {
            return false;
        }

        return string.Equals(GivenName, other.GivenName, StringComparison.Ordinal)
            && string.Equals(FamilyName, other.FamilyName, StringComparison.Ordinal)
            && Category == other.Category;
    }

    public override bool Equals(object? obj) => Equals(obj as FlightReservationPassengerFact);

    public override int GetHashCode() => HashCode.Combine(GivenName, FamilyName, Category);
}

/// <summary>
/// Reservation request derived from persisted FlightBooking + accepted offer. Not client-reconstructed.
/// </summary>
public sealed class FlightReservationRequest
{
    public FlightReservationRequest(
        Guid flightBookingId,
        FlightTripType tripType,
        IReadOnlyList<FlightOfferSegmentIdentity> segments,
        IReadOnlyList<FlightReservationPassengerFact> passengers,
        Guid offerSnapshotId,
        string sourceOfferReference,
        MoneyValue total,
        string idempotencyKey)
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
        ArgumentNullException.ThrowIfNull(total);
        if (segments.Count == 0)
        {
            throw new ArgumentException("Reservation request requires at least one segment.", nameof(segments));
        }

        if (passengers.Count == 0)
        {
            throw new ArgumentException("Reservation request requires at least one passenger.", nameof(passengers));
        }

        if (offerSnapshotId == Guid.Empty)
        {
            throw new ArgumentException("OfferSnapshotId is required.", nameof(offerSnapshotId));
        }

        if (string.IsNullOrWhiteSpace(sourceOfferReference))
        {
            throw new ArgumentException("SourceOfferReference is required.", nameof(sourceOfferReference));
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("IdempotencyKey is required.", nameof(idempotencyKey));
        }

        FlightBookingId = flightBookingId;
        TripType = tripType;
        Segments = segments;
        Passengers = passengers;
        OfferSnapshotId = offerSnapshotId;
        SourceOfferReference = sourceOfferReference.Trim();
        Total = total;
        IdempotencyKey = idempotencyKey.Trim();
    }

    public Guid FlightBookingId { get; }

    public FlightTripType TripType { get; }

    public IReadOnlyList<FlightOfferSegmentIdentity> Segments { get; }

    public IReadOnlyList<FlightReservationPassengerFact> Passengers { get; }

    public Guid OfferSnapshotId { get; }

    public string SourceOfferReference { get; }

    public MoneyValue Total { get; }

    public string IdempotencyKey { get; }
}

public enum FlightReservationSourceOutcome : short
{
    Complete = 1,
    Failed = 2,
    Partial = 3,
    Timeout = 4,
    Unknown = 5,
}

public sealed class FlightReservationSourceResult
{
    public const int SourceReservationReferenceMaxLength = 128;
    public const int ReservationLocatorMaxLength = 32;
    public const int SourceOfferReferenceMaxLength = 128;

    public FlightReservationSourceResult(
        FlightReservationSourceOutcome outcome,
        string? sourceReservationReference = null,
        string? reservationLocator = null,
        IReadOnlyList<FlightOfferSegmentIdentity>? confirmedSegments = null,
        IReadOnlyList<FlightReservationPassengerFact>? confirmedPassengers = null,
        MoneyValue? reportedTotal = null,
        Instant? reservationExpiresAt = null,
        string? sourceOfferReference = null)
    {
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Reservation outcome is not controlled.");
        }

        Outcome = outcome;
        SourceReservationReference = NormalizeOptional(
            sourceReservationReference,
            SourceReservationReferenceMaxLength);
        ReservationLocator = NormalizeOptional(reservationLocator, ReservationLocatorMaxLength);
        ConfirmedSegments = confirmedSegments ?? [];
        ConfirmedPassengers = confirmedPassengers ?? [];
        ReportedTotal = reportedTotal;
        ReservationExpiresAt = reservationExpiresAt == default ? null : reservationExpiresAt;
        SourceOfferReference = NormalizeOptional(sourceOfferReference, SourceOfferReferenceMaxLength);
    }

    public FlightReservationSourceOutcome Outcome { get; }

    public string? SourceReservationReference { get; }

    public string? ReservationLocator { get; }

    public IReadOnlyList<FlightOfferSegmentIdentity> ConfirmedSegments { get; }

    public IReadOnlyList<FlightReservationPassengerFact> ConfirmedPassengers { get; }

    public MoneyValue? ReportedTotal { get; }

    public Instant? ReservationExpiresAt { get; }

    public string? SourceOfferReference { get; }

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

public enum FlightReservationQueryStatus : short
{
    Confirmed = 1,
    Expired = 2,
    Cancelled = 3,
    NotCreated = 4,
    PendingOrUnknown = 5,
}

public sealed class FlightReservationQueryResult
{
    public FlightReservationQueryResult(
        FlightReservationQueryStatus status,
        string? sourceReservationReference = null,
        string? reservationLocator = null,
        IReadOnlyList<FlightOfferSegmentIdentity>? confirmedSegments = null,
        IReadOnlyList<FlightReservationPassengerFact>? confirmedPassengers = null,
        MoneyValue? reportedTotal = null,
        Instant? reservationExpiresAt = null,
        string? sourceOfferReference = null)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Query status is not controlled.");
        }

        Status = status;
        SourceReservationReference = sourceReservationReference;
        ReservationLocator = reservationLocator;
        ConfirmedSegments = confirmedSegments ?? [];
        ConfirmedPassengers = confirmedPassengers ?? [];
        ReportedTotal = reportedTotal;
        ReservationExpiresAt = reservationExpiresAt == default ? null : reservationExpiresAt;
        SourceOfferReference = sourceOfferReference;
    }

    public FlightReservationQueryStatus Status { get; }

    public string? SourceReservationReference { get; }

    public string? ReservationLocator { get; }

    public IReadOnlyList<FlightOfferSegmentIdentity> ConfirmedSegments { get; }

    public IReadOnlyList<FlightReservationPassengerFact> ConfirmedPassengers { get; }

    public MoneyValue? ReportedTotal { get; }

    public Instant? ReservationExpiresAt { get; }

    public string? SourceOfferReference { get; }
}
