using NodaTime;
using TravelCore.Modules.Booking.Contracts;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Transactional Tour Booking aggregate (TC-P19-T002 / P19-R2).
/// Targets one logical TourDeparture. Lifecycle is Pending/Confirmed/Cancelled.
/// Unrestricted confirmation is intentionally deferred (R3/R5/R6).
/// </summary>
public sealed class Booking
{
    private Booking()
    {
    }

    private Booking(BookingId id, TourDepartureReference tourDeparture, Instant createdAt)
    {
        Id = id;
        TourDeparture = tourDeparture;
        Status = BookingStatus.Pending;
        CreatedAt = createdAt;
        StatusChangedAt = createdAt;
    }

    public BookingId Id { get; private set; }

    public TourDepartureReference TourDeparture { get; private set; }

    public BookingStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant StatusChangedAt { get; private set; }

    public static Booking Create(TourDepartureReference tourDeparture, Instant now)
    {
        if (tourDeparture.LogicalId == Guid.Empty)
        {
            throw new ArgumentException("TourDeparture reference cannot be empty.", nameof(tourDeparture));
        }

        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new Booking(BookingId.New(), tourDeparture, now);
    }

    /// <summary>
    /// Pending → Cancelled only. Does not release capacity or refund payment (R3/R6 deferred).
    /// </summary>
    public void CancelPending(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("StatusChangedAt cannot be default.", nameof(now));
        }

        if (Status == BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled Booking cannot be reopened or cancelled again.");
        }

        if (Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException("Only Pending Booking can be cancelled in T002.");
        }

        Status = BookingStatus.Cancelled;
        StatusChangedAt = now;
    }
}
