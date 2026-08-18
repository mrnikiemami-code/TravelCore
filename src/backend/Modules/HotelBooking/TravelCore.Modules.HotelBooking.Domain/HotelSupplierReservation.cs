using NodaTime;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// HotelBooking-owned durable correlation to a final hotel reservation source (TC-P21-T005 / P21-R5).
/// Not HotelBooking itself, not an availability hold, and not a named supplier SDK.
/// </summary>
public sealed class HotelSupplierReservation
{
    public const int SourceKeyMaxLength = 64;
    public const int SourceReservationReferenceMaxLength = 128;
    public const int ConfirmationCodeMaxLength = 64;

    private readonly List<HotelSupplierReservationAttempt> _attempts = [];

    private HotelSupplierReservation()
    {
        SourceKey = string.Empty;
    }

    private HotelSupplierReservation(
        HotelSupplierReservationId id,
        HotelBookingId hotelBookingId,
        string sourceKey,
        Instant createdAt)
    {
        Id = id;
        HotelBookingId = hotelBookingId;
        SourceKey = sourceKey;
        Status = HotelSupplierReservationStatus.Pending;
        CreatedAt = createdAt;
        Version = 0;
    }

    public HotelSupplierReservationId Id { get; private set; }

    public HotelBookingId HotelBookingId { get; private set; }

    public string SourceKey { get; private set; }

    public HotelSupplierReservationStatus Status { get; private set; }

    public string? SourceReservationReference { get; private set; }

    public string? SupplierConfirmationCode { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant? ConfirmedAt { get; private set; }

    public Instant? CancelledAt { get; private set; }

    public long Version { get; private set; }

    public IReadOnlyList<HotelSupplierReservationAttempt> Attempts => _attempts;

    public bool HasUnresolvedAttempt => _attempts.Any(a => a.IsUnresolved);

    public static HotelSupplierReservation StartPending(
        HotelBookingId hotelBookingId,
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

        return new HotelSupplierReservation(
            HotelSupplierReservationId.New(),
            hotelBookingId,
            normalized,
            createdAt);
    }

    public HotelSupplierReservationAttempt StartAttempt(Instant now)
    {
        EnsureClock(now);
        if (Status == HotelSupplierReservationStatus.Confirmed)
        {
            throw new InvalidOperationException("Confirmed reservation cannot start another attempt.");
        }

        if (Status == HotelSupplierReservationStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled reservation cannot start another attempt.");
        }

        if (HasUnresolvedAttempt)
        {
            throw new InvalidOperationException(
                "An unresolved Created/Initiated attempt blocks another reservation attempt.");
        }

        var attempt = new HotelSupplierReservationAttempt(
            HotelSupplierReservationAttemptId.New(),
            Id,
            now);
        _attempts.Add(attempt);
        IncrementVersion();
        return attempt;
    }

    public void MarkAttemptInitiated(HotelSupplierReservationAttemptId attemptId, Instant now)
    {
        var attempt = RequireAttempt(attemptId);
        attempt.MarkInitiated(now);
        IncrementVersion();
    }

    public void RecordSourceCorrelation(string sourceReservationReference, string? supplierConfirmationCode = null)
    {
        var trimmedRef = NormalizeRequired(sourceReservationReference, SourceReservationReferenceMaxLength);
        if (trimmedRef == Id.Value.ToString("D") || trimmedRef == HotelBookingId.Value.ToString("D"))
        {
            throw new ArgumentException(
                "SourceReservationReference must not equal internal reservation or booking identity.",
                nameof(sourceReservationReference));
        }

        if (Status == HotelSupplierReservationStatus.Confirmed
            && !string.Equals(SourceReservationReference, trimmedRef, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Confirmed reservation cannot bind a different source reservation reference.");
        }

        SourceReservationReference = trimmedRef;
        if (!string.IsNullOrWhiteSpace(supplierConfirmationCode))
        {
            var code = supplierConfirmationCode.Trim();
            if (code.Length > ConfirmationCodeMaxLength)
            {
                throw new ArgumentException(
                    $"Confirmation code max length is {ConfirmationCodeMaxLength}.",
                    nameof(supplierConfirmationCode));
            }

            SupplierConfirmationCode = code;
        }

        IncrementVersion();
    }

    public void ConfirmAttempt(
        HotelSupplierReservationAttemptId attemptId,
        Instant now,
        string sourceReservationReference,
        string? supplierConfirmationCode,
        IReadOnlyCollection<RoomReservationId> confirmedRooms,
        IReadOnlyCollection<RoomReservationId> requestedRooms)
    {
        ArgumentNullException.ThrowIfNull(confirmedRooms);
        ArgumentNullException.ThrowIfNull(requestedRooms);
        var attempt = RequireAttempt(attemptId);
        if (requestedRooms.Count == 0)
        {
            throw new ArgumentException("Requested rooms are required.", nameof(requestedRooms));
        }

        if (confirmedRooms.Count != requestedRooms.Count
            || requestedRooms.Any(id => !confirmedRooms.Contains(id)))
        {
            throw new InvalidOperationException(
                "Partial room confirmation cannot confirm HotelSupplierReservation.");
        }

        if (Status == HotelSupplierReservationStatus.Confirmed
            && string.Equals(SourceReservationReference, sourceReservationReference.Trim(), StringComparison.Ordinal))
        {
            attempt.MarkConfirmed(now);
            IncrementVersion();
            return;
        }

        var trimmedRef = NormalizeRequired(sourceReservationReference, SourceReservationReferenceMaxLength);
        if (trimmedRef == Id.Value.ToString("D") || trimmedRef == HotelBookingId.Value.ToString("D"))
        {
            throw new ArgumentException(
                "SourceReservationReference must not equal internal reservation or booking identity.",
                nameof(sourceReservationReference));
        }

        string? code = null;
        if (!string.IsNullOrWhiteSpace(supplierConfirmationCode))
        {
            code = supplierConfirmationCode.Trim();
            if (code.Length > ConfirmationCodeMaxLength)
            {
                throw new ArgumentException(
                    $"Confirmation code max length is {ConfirmationCodeMaxLength}.",
                    nameof(supplierConfirmationCode));
            }
        }

        attempt.MarkConfirmed(now);
        SourceReservationReference = trimmedRef;
        SupplierConfirmationCode = code;
        Status = HotelSupplierReservationStatus.Confirmed;
        ConfirmedAt = now;
        IncrementVersion();
    }

    public void FailAttempt(HotelSupplierReservationAttemptId attemptId, Instant now)
    {
        var attempt = RequireAttempt(attemptId);
        attempt.MarkFailed(now);
        IncrementVersion();
    }

    public void CancelFromSource(Instant now)
    {
        EnsureClock(now);
        if (Status == HotelSupplierReservationStatus.Cancelled)
        {
            return;
        }

        if (Status == HotelSupplierReservationStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "Confirmed reservation cannot silently become Cancelled from contradictory evidence.");
        }

        Status = HotelSupplierReservationStatus.Cancelled;
        CancelledAt = now;
        IncrementVersion();
    }

    private HotelSupplierReservationAttempt RequireAttempt(HotelSupplierReservationAttemptId attemptId) =>
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
