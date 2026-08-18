using System.Security.Cryptography;
using System.Text;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Opaque Booking-scoped access token. Raw value is never persisted.
/// </summary>
public static class BookingAccessToken
{
    public static string CreateRaw()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static string Hash(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            throw new ArgumentException("Access token is required.", nameof(rawToken));
        }

        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken.Trim()));
        return Convert.ToHexString(digest).ToLowerInvariant();
    }
}
