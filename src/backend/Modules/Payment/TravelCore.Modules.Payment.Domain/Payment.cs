using NodaTime;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Logical monetary collection for exactly one supported target (Tour Booking or HotelBooking).
/// Not an open TargetType platform (P21-R6). Owns PaymentAttempt children.
/// </summary>
public sealed class Payment
{
    private readonly List<PaymentAttempt> _attempts = [];

    private Payment()
    {
    }

    private Payment(
        PaymentId id,
        BookingReference? booking,
        HotelBookingPaymentReference? hotelBooking,
        Instant createdAt)
    {
        if ((booking is null) == (hotelBooking is null))
        {
            throw new ArgumentException("A Payment must belong to exactly one supported target.");
        }

        Id = id;
        Booking = booking;
        HotelBooking = hotelBooking;
        Status = PaymentStatus.Pending;
        CreatedAt = createdAt;
        StatusChangedAt = createdAt;
        Version = 0;
    }

    public PaymentId Id { get; private set; }

    public BookingReference? Booking { get; private set; }

    public HotelBookingPaymentReference? HotelBooking { get; private set; }

    public PaymentTargetKind TargetKind =>
        HotelBooking is not null ? PaymentTargetKind.HotelBooking : PaymentTargetKind.TourBooking;

    public Guid TargetReferenceId =>
        HotelBooking?.HotelBookingId
        ?? Booking?.BookingId
        ?? throw new InvalidOperationException("Payment has no target.");

    public PaymentStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant StatusChangedAt { get; private set; }

    public Instant? SucceededAt { get; private set; }

    public long Version { get; private set; }

    public PaymentExecutionSnapshot? ExecutionSnapshot { get; private set; }

    public IReadOnlyList<PaymentAttempt> Attempts => _attempts;

    public static Payment Create(BookingReference booking, Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new Payment(PaymentId.New(), booking, hotelBooking: null, now);
    }

    public static Payment CreateForHotel(HotelBookingPaymentReference hotelBooking, Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new Payment(PaymentId.New(), booking: null, hotelBooking, now);
    }

    public PaymentAttempt CreateAttempt(Instant now)
    {
        EnsureClock(now);
        EnsurePending();
        if (_attempts.Any(attempt => attempt.IsActive))
        {
            throw new InvalidOperationException(
                "A Payment may have at most one non-terminal PaymentAttempt at a time.");
        }

        var attempt = new PaymentAttempt(PaymentAttemptId.New(), now);
        _attempts.Add(attempt);
        IncrementVersion();
        return attempt;
    }

    public void BindExecutionSnapshot(Guid bookingSnapshotId, global::TravelCore.Money.Money amount, Instant now)
    {
        EnsureClock(now);
        ArgumentNullException.ThrowIfNull(amount);

        if (ExecutionSnapshot is null)
        {
            ExecutionSnapshot = PaymentExecutionSnapshot.Create(bookingSnapshotId, amount, now);
            IncrementVersion();
            return;
        }

        if (!ExecutionSnapshot.Matches(bookingSnapshotId, amount))
        {
            throw new InvalidOperationException(
                "PaymentExecutionSnapshot is already bound; overwrite with different obligation is forbidden.");
        }
    }

    public void InitiateAttempt(PaymentAttemptId attemptId, Instant now)
    {
        EnsurePending();
        FindAttempt(attemptId).MarkInitiated(now);
        IncrementVersion();
    }

    public void RecordProviderInitiation(
        PaymentAttemptId attemptId,
        Instant now,
        ProviderKey providerKey,
        ProviderRequestReference? requestReference,
        ProviderTransactionReference? transactionReference)
    {
        EnsureClock(now);
        EnsurePending();
        var attempt = FindAttempt(attemptId);
        attempt.AttachProviderCorrelation(providerKey, requestReference, transactionReference);
        attempt.MarkInitiated(now);
        IncrementVersion();
    }

    public void RecordAmbiguousProviderInitiation(
        PaymentAttemptId attemptId,
        Instant now,
        ProviderKey providerKey,
        ProviderRequestReference? requestReference,
        ProviderTransactionReference? transactionReference)
    {
        EnsureClock(now);
        EnsurePending();
        var attempt = FindAttempt(attemptId);
        attempt.AttachProviderCorrelation(providerKey, requestReference, transactionReference);
        if (requestReference is not null || transactionReference is not null)
        {
            attempt.MarkInitiated(now);
        }

        IncrementVersion();
    }

    public void RecordAttemptFailure(PaymentAttemptId attemptId, Instant now)
    {
        EnsurePending();
        var attempt = FindAttempt(attemptId);
        if (attempt.Status == PaymentAttemptStatus.Failed)
        {
            return;
        }

        attempt.RecordFailure(now);
        IncrementVersion();
    }

    /// <summary>
    /// Trusted-evidence boundary for authoritative collection success.
    /// Not client/browser/unverified-callback-controlled. Duplicate success is idempotent.
    /// </summary>
    internal void RecordAuthoritativeCollectionSuccess(PaymentAttemptId attemptId, Instant now)
    {
        EnsureClock(now);
        var attempt = FindAttempt(attemptId);
        if (Status == PaymentStatus.Succeeded)
        {
            if (attempt.Status == PaymentAttemptStatus.Succeeded)
            {
                return;
            }

            throw new InvalidOperationException("A Payment may have at most one successful PaymentAttempt.");
        }

        EnsurePending();
        if (_attempts.Any(item =>
                item.Status == PaymentAttemptStatus.Succeeded && !item.Id.Equals(attemptId)))
        {
            throw new InvalidOperationException("A Payment may have at most one successful PaymentAttempt.");
        }

        attempt.RecordAuthoritativeSuccess(now);
        Status = PaymentStatus.Succeeded;
        StatusChangedAt = now;
        SucceededAt = now;
        IncrementVersion();
    }

    private PaymentAttempt FindAttempt(PaymentAttemptId attemptId)
    {
        var attempt = _attempts.SingleOrDefault(item => item.Id.Equals(attemptId));
        if (attempt is null)
        {
            throw new InvalidOperationException("PaymentAttempt does not belong to this Payment.");
        }

        return attempt;
    }

    private void EnsurePending()
    {
        if (Status == PaymentStatus.Succeeded)
        {
            throw new InvalidOperationException(
                "Succeeded Payment cannot accept further collection attempts or status reversal.");
        }

        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("Payment is not Pending.");
        }
    }

    private void IncrementVersion() => Version++;


    private static void EnsureClock(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("Timestamp cannot be default.", nameof(now));
        }
    }
}
