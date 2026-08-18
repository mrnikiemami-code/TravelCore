using NodaTime;
using TravelCore.Modules.Flight.Contracts;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Flight-owned durable correlation to a final supplier reservation / PNR locator (TC-P22-T005 / P22-R5).
/// Not FlightBooking itself, not a type named PNR, and not a named supplier SDK.
/// One FlightBooking has at most one logical FlightSupplierReservation covering the complete itinerary.
/// </summary>
public sealed class FlightSupplierReservation
{
    public const int SourceKeyMaxLength = 64;
    public const int SourceReservationReferenceMaxLength = 128;
    public const int ReservationLocatorMaxLength = 32;

    private readonly List<FlightSupplierReservationAttempt> _attempts = [];

    private FlightSupplierReservation()
    {
        SourceKey = string.Empty;
    }

    private FlightSupplierReservation(
        FlightSupplierReservationId id,
        FlightBookingId flightBookingId,
        string sourceKey,
        Instant createdAt)
    {
        Id = id;
        FlightBookingId = flightBookingId;
        SourceKey = sourceKey;
        Status = FlightSupplierReservationStatus.Pending;
        CreatedAt = createdAt;
        Version = 0;
    }

    public FlightSupplierReservationId Id { get; private set; }

    public FlightBookingId FlightBookingId { get; private set; }

    public string SourceKey { get; private set; }

    public FlightSupplierReservationStatus Status { get; private set; }

    public string? SourceReservationReference { get; private set; }

    public string? ReservationLocator { get; private set; }

    public Instant? ReservationExpiresAt { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant? ConfirmedAt { get; private set; }

    public Instant? ExpiredAt { get; private set; }

    public Instant? CancelledAt { get; private set; }

    public long Version { get; private set; }

    public IReadOnlyList<FlightSupplierReservationAttempt> Attempts => _attempts;

    public bool HasUnresolvedAttempt => _attempts.Any(a => a.IsUnresolved);

    public static FlightSupplierReservation StartPending(
        FlightBookingId flightBookingId,
        string sourceKey,
        Instant createdAt)
    {
        if (createdAt == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(createdAt));
        }

        if (string.IsNullOrWhiteSpace(sourceKey))
        {
            throw new ArgumentException("SourceKey is required.", nameof(sourceKey));
        }

        var normalized = sourceKey.Trim().ToLowerInvariant();
        if (normalized.Length > SourceKeyMaxLength)
        {
            throw new ArgumentException($"SourceKey max length is {SourceKeyMaxLength}.", nameof(sourceKey));
        }

        return new FlightSupplierReservation(
            FlightSupplierReservationId.New(),
            flightBookingId,
            normalized,
            createdAt);
    }

    public FlightSupplierReservationAttempt StartAttempt(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightSupplierReservationStatus.Confirmed)
        {
            throw new InvalidOperationException("Confirmed reservation cannot start another attempt.");
        }

        if (Status is FlightSupplierReservationStatus.Expired or FlightSupplierReservationStatus.Cancelled)
        {
            throw new InvalidOperationException($"{Status} reservation cannot start another attempt.");
        }

        if (HasUnresolvedAttempt)
        {
            throw new InvalidOperationException(
                "An unresolved Created/Initiated attempt blocks another reservation attempt.");
        }

        var attempt = new FlightSupplierReservationAttempt(
            FlightSupplierReservationAttemptId.New(),
            Id,
            now);
        _attempts.Add(attempt);
        IncrementVersion();
        return attempt;
    }

    public void MarkAttemptInitiated(FlightSupplierReservationAttemptId attemptId, Instant now)
    {
        var attempt = RequireAttempt(attemptId);
        attempt.MarkInitiated(now);
        IncrementVersion();
    }

    public void RecordSourceCorrelation(
        string sourceReservationReference,
        string? reservationLocator = null,
        Instant? reservationExpiresAt = null)
    {
        var trimmedRef = NormalizeRequired(sourceReservationReference, SourceReservationReferenceMaxLength);
        RejectIdentityCollision(trimmedRef, nameof(sourceReservationReference));

        if (Status == FlightSupplierReservationStatus.Confirmed
            && !string.Equals(SourceReservationReference, trimmedRef, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Confirmed reservation cannot bind a different source reservation reference.");
        }

        SourceReservationReference = trimmedRef;
        if (!string.IsNullOrWhiteSpace(reservationLocator))
        {
            var locator = NormalizeRequired(reservationLocator, ReservationLocatorMaxLength);
            RejectIdentityCollision(locator, nameof(reservationLocator));
            if (string.Equals(locator, trimmedRef, StringComparison.Ordinal)
                && locator == Id.Value.ToString("D"))
            {
                throw new ArgumentException(
                    "ReservationLocator must not equal internal reservation identity.",
                    nameof(reservationLocator));
            }

            ReservationLocator = locator;
        }

        if (reservationExpiresAt is { } expires && expires != default)
        {
            ReservationExpiresAt = expires;
        }

        IncrementVersion();
    }

    public void ConfirmAttempt(
        FlightSupplierReservationAttemptId attemptId,
        Instant now,
        string sourceReservationReference,
        string reservationLocator,
        Instant? reservationExpiresAt,
        IReadOnlyCollection<FlightOfferSegmentIdentity> confirmedSegments,
        IReadOnlyCollection<FlightOfferSegmentIdentity> expectedSegments,
        IReadOnlyCollection<FlightReservationPassengerFact> confirmedPassengers,
        IReadOnlyCollection<FlightReservationPassengerFact> expectedPassengers)
    {
        ArgumentNullException.ThrowIfNull(confirmedSegments);
        ArgumentNullException.ThrowIfNull(expectedSegments);
        ArgumentNullException.ThrowIfNull(confirmedPassengers);
        ArgumentNullException.ThrowIfNull(expectedPassengers);
        var attempt = RequireAttempt(attemptId);
        if (expectedSegments.Count == 0)
        {
            throw new ArgumentException("Expected segments are required.", nameof(expectedSegments));
        }

        if (expectedPassengers.Count == 0)
        {
            throw new ArgumentException("Expected passengers are required.", nameof(expectedPassengers));
        }

        if (!FlightReservationReconciliation.SegmentsMatch(confirmedSegments, expectedSegments))
        {
            throw new InvalidOperationException(
                "Partial itinerary confirmation cannot confirm FlightSupplierReservation.");
        }

        if (!FlightReservationReconciliation.PassengersMatch(confirmedPassengers, expectedPassengers))
        {
            throw new InvalidOperationException(
                "Partial passenger confirmation cannot confirm FlightSupplierReservation.");
        }

        if (Status == FlightSupplierReservationStatus.Confirmed
            && string.Equals(SourceReservationReference, sourceReservationReference.Trim(), StringComparison.Ordinal))
        {
            attempt.MarkConfirmed(now);
            IncrementVersion();
            return;
        }

        var trimmedRef = NormalizeRequired(sourceReservationReference, SourceReservationReferenceMaxLength);
        var locator = NormalizeRequired(reservationLocator, ReservationLocatorMaxLength);
        RejectIdentityCollision(trimmedRef, nameof(sourceReservationReference));
        RejectIdentityCollision(locator, nameof(reservationLocator));

        attempt.MarkConfirmed(now);
        SourceReservationReference = trimmedRef;
        ReservationLocator = locator;
        if (reservationExpiresAt is { } expires && expires != default)
        {
            ReservationExpiresAt = expires;
        }

        Status = FlightSupplierReservationStatus.Confirmed;
        ConfirmedAt = now;
        IncrementVersion();
    }

    public void FailAttempt(FlightSupplierReservationAttemptId attemptId, Instant now)
    {
        var attempt = RequireAttempt(attemptId);
        attempt.MarkFailed(now);
        IncrementVersion();
    }

    public void ExpireFromSource(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightSupplierReservationStatus.Expired)
        {
            return;
        }

        if (Status == FlightSupplierReservationStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Cancelled reservation cannot silently become Expired from contradictory evidence.");
        }

        if (Status is not FlightSupplierReservationStatus.Pending
            and not FlightSupplierReservationStatus.Confirmed)
        {
            throw new InvalidOperationException($"Reservation in status {Status} cannot become Expired.");
        }

        Status = FlightSupplierReservationStatus.Expired;
        ExpiredAt = now;
        IncrementVersion();
    }

    public void CancelFromAuthoritativeSource(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightSupplierReservationStatus.Cancelled)
        {
            return;
        }

        if (Status == FlightSupplierReservationStatus.Expired)
        {
            throw new InvalidOperationException(
                "Expired reservation cannot silently become Cancelled from contradictory evidence.");
        }

        if (Status is not FlightSupplierReservationStatus.Pending
            and not FlightSupplierReservationStatus.Confirmed)
        {
            throw new InvalidOperationException($"Reservation in status {Status} cannot become Cancelled.");
        }

        Status = FlightSupplierReservationStatus.Cancelled;
        CancelledAt = now;
        IncrementVersion();
    }

    private void RejectIdentityCollision(string value, string paramName)
    {
        if (value == Id.Value.ToString("D") || value == FlightBookingId.Value.ToString("D"))
        {
            throw new ArgumentException(
                "SourceReservationReference and ReservationLocator must not equal internal reservation or booking identity.",
                paramName);
        }
    }

    private FlightSupplierReservationAttempt RequireAttempt(FlightSupplierReservationAttemptId attemptId) =>
        _attempts.Single(a => a.Id.Equals(attemptId));

    private static string NormalizeRequired(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Max length is {maxLength}.");
        }

        return trimmed;
    }

    private static void EnsureClock(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("Instant cannot be default.", nameof(now));
        }
    }

    private void IncrementVersion() => Version++;
}
