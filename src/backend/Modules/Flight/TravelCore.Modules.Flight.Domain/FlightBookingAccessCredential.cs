using NodaTime;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// FlightBooking-scoped access verifier (TC-P22-T008 / P22-R8).
/// Raw token is returned once; only the SHA-256 hash is persisted.
/// FlightBookingId is not a credential.
/// </summary>
public sealed class FlightBookingAccessCredential
{
    public const int TokenHashLength = 64;

    private FlightBookingAccessCredential()
    {
        TokenHash = null!;
    }

    private FlightBookingAccessCredential(FlightBookingId flightBookingId, string tokenHash, Instant createdAt)
    {
        FlightBookingId = flightBookingId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
    }

    public FlightBookingId FlightBookingId { get; private set; }

    public string TokenHash { get; private set; }

    public Instant CreatedAt { get; private set; }

    public static FlightBookingAccessCredential Create(
        FlightBookingId flightBookingId,
        string tokenHash,
        Instant now)
    {
        if (flightBookingId.Value == Guid.Empty)
        {
            throw new ArgumentException("FlightBookingId cannot be empty.", nameof(flightBookingId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash) || tokenHash.Length != TokenHashLength)
        {
            throw new ArgumentException("Token hash must be a 64-character SHA-256 hex digest.", nameof(tokenHash));
        }

        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new FlightBookingAccessCredential(flightBookingId, tokenHash, now);
    }
}
