namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// Logical reference to a declared B2B commercial capability posture. Not a persisted product entity.
/// </summary>
public sealed class CommercialCapabilityReference
{
    private CommercialCapabilityReference()
    {
        CapabilityCode = null!;
    }

    private CommercialCapabilityReference(string capabilityCode)
    {
        CapabilityCode = NormalizeRequired(capabilityCode, nameof(capabilityCode), 64);
    }

    /// <summary>
    /// Opaque capability code for boundary documentation only (e.g. partner-booking-intent).
    /// </summary>
    public string CapabilityCode { get; private set; }

    public static CommercialCapabilityReference FromCode(string capabilityCode) =>
        new(capabilityCode);

    private static string NormalizeRequired(string value, string paramName, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Length must be <= {maxLength}.");
        }

        return trimmed;
    }
}
