using NodaTime;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// HotelBooking-scoped access verifier (TC-P21-T008 / P21-R8).
/// Raw token is returned once; only the SHA-256 hash is persisted.
/// HotelBookingId is not a credential.
/// </summary>
public sealed class HotelBookingAccessCredential
{
    public const int TokenHashLength = 64;

    private HotelBookingAccessCredential()
    {
        TokenHash = null!;
    }

    private HotelBookingAccessCredential(HotelBookingId hotelBookingId, string tokenHash, Instant createdAt)
    {
        HotelBookingId = hotelBookingId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
    }

    public HotelBookingId HotelBookingId { get; private set; }

    public string TokenHash { get; private set; }

    public Instant CreatedAt { get; private set; }

    public static HotelBookingAccessCredential Create(
        HotelBookingId hotelBookingId,
        string tokenHash,
        Instant now)
    {
        if (hotelBookingId.Value == Guid.Empty)
        {
            throw new ArgumentException("HotelBookingId cannot be empty.", nameof(hotelBookingId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash) || tokenHash.Length != TokenHashLength)
        {
            throw new ArgumentException("Token hash must be a 64-character SHA-256 hex digest.", nameof(tokenHash));
        }

        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new HotelBookingAccessCredential(hotelBookingId, tokenHash, now);
    }
}
