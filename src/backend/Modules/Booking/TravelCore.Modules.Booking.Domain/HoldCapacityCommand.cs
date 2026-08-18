using NodaTime;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Application input for a temporary capacity hold.
/// <see cref="ConfiguredCapacity"/> must be the authoritative Tour/TourDeparture
/// definition read for this operation — not a client-invented catalog value.
/// Hold duration is supplied as <see cref="ExpiresAt"/>; domain has no magic timeout.
/// </summary>
public sealed record HoldCapacityCommand(
    BookingId BookingId,
    int SeatCount,
    int ConfiguredCapacity,
    Instant ExpiresAt,
    Instant Now,
    string IdempotencyKey);
