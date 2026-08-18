namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Logical airline/carrier reference. ReferenceData is the catalog authority; Flight stores a
/// validated IATA code without a Flight Airline aggregate or peer-schema FK.
/// </summary>
public readonly record struct AirlineReference
{
    public const int IataCodeLength = 2;

    public string IataCode { get; }

    public AirlineReference(string iataCode)
    {
        IataCode = Normalize(iataCode);
    }

    public static string Normalize(string iataCode)
    {
        if (string.IsNullOrWhiteSpace(iataCode))
        {
            throw new ArgumentException("Airline IATA code is required.", nameof(iataCode));
        }

        var normalized = iataCode.Trim().ToUpperInvariant();
        if (normalized.Length != IataCodeLength
            || !normalized.All(static c => char.IsAsciiLetterOrDigit(c)))
        {
            throw new ArgumentException(
                "Airline IATA code must be exactly 2 ASCII letters or digits.",
                nameof(iataCode));
        }

        return normalized;
    }

    public override string ToString() => IataCode;
}
