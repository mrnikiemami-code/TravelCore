using System.Security.Cryptography;
using System.Text;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Opaque FlightBooking-scoped access token. Raw value is never persisted.
/// Independent of Tour BookingAccessToken and HotelBookingAccessToken (P22-R8).
/// </summary>
public static class FlightBookingAccessToken
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
