namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// Logical reference to declared agency operational capability intent.
/// </summary>
public sealed class AgencyOperationalCapabilityReference
{
    private AgencyOperationalCapabilityReference()
    {
        CapabilityCode = null!;
    }

    private AgencyOperationalCapabilityReference(string capabilityCode)
    {
        CapabilityCode = NormalizeRequired(capabilityCode, nameof(capabilityCode), 64);
    }

    public string CapabilityCode { get; private set; }

    public static AgencyOperationalCapabilityReference FromCode(string capabilityCode) =>
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
