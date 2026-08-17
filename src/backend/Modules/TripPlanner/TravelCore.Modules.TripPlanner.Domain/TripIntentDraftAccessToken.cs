using System.Security.Cryptography;

namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Opaque retrieval token for anonymous TripIntent draft access (P18-R3).
/// Identifies access to a draft, not a human identity.
/// </summary>
public sealed class TripIntentDraftAccessToken
{
    public const int TokenByteLength = 32;
    public const int StoredValueMaxLength = 64;

    private TripIntentDraftAccessToken(string storedValue)
    {
        Value = storedValue;
    }

    public string Value { get; }

    public static TripIntentDraftAccessToken Generate()
    {
        Span<byte> bytes = stackalloc byte[TokenByteLength];
        RandomNumberGenerator.Fill(bytes);
        return new TripIntentDraftAccessToken(Convert.ToBase64String(bytes));
    }

    public static TripIntentDraftAccessToken FromStored(string storedValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedValue);
        var trimmed = storedValue.Trim();
        if (trimmed.Length > StoredValueMaxLength)
        {
            throw new ArgumentException(
                $"Draft access token max length is {StoredValueMaxLength}.",
                nameof(storedValue));
        }

        return new TripIntentDraftAccessToken(trimmed);
    }
}
