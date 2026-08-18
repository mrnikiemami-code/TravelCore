using System.Security.Cryptography;
using System.Text;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Opaque HotelBooking-scoped access token. Raw value is never persisted.
/// Independent of Tour BookingAccessToken (P21-R8).
/// </summary>
public static class HotelBookingAccessToken
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
