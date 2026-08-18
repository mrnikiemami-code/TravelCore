using NodaTime;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Booking-scoped access verifier (TC-P19-T008 / P19-R8).
/// Raw token is returned once; only the hash is persisted. BookingId is not a credential.
/// </summary>
public sealed class BookingAccessCredential
{
    public const int TokenHashLength = 64;

    private BookingAccessCredential()
    {
        TokenHash = null!;
    }

    private BookingAccessCredential(BookingId bookingId, string tokenHash, Instant createdAt)
    {
        BookingId = bookingId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
    }

    public BookingId BookingId { get; private set; }

    public string TokenHash { get; private set; }

    public Instant CreatedAt { get; private set; }

    public static BookingAccessCredential Create(BookingId bookingId, string tokenHash, Instant now)
    {
        if (bookingId.Value == Guid.Empty)
        {
            throw new ArgumentException("BookingId cannot be empty.", nameof(bookingId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash) || tokenHash.Length != TokenHashLength)
        {
            throw new ArgumentException("Token hash must be a 64-character SHA-256 hex digest.", nameof(tokenHash));
        }

        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new BookingAccessCredential(bookingId, tokenHash, now);
    }
}
