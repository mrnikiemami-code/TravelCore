namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Logical airport reference. ReferenceData is the catalog authority; Flight stores a validated
/// IATA code — not a catalog entity id and not a peer-schema FK.
/// </summary>
public readonly record struct AirportReference
{
    public const int IataCodeLength = 3;

    public string IataCode { get; }

    public AirportReference(string iataCode)
    {
        IataCode = Normalize(iataCode);
    }

    public static string Normalize(string iataCode)
    {
        if (string.IsNullOrWhiteSpace(iataCode))
        {
            throw new ArgumentException("Airport IATA code is required.", nameof(iataCode));
        }

        var normalized = iataCode.Trim().ToUpperInvariant();
        if (normalized.Length != IataCodeLength || !normalized.All(static c => c is >= 'A' and <= 'Z'))
        {
            throw new ArgumentException(
                "Airport IATA code must be exactly 3 ASCII letters.",
                nameof(iataCode));
        }

        return normalized;
    }

    public override string ToString() => IataCode;
}
