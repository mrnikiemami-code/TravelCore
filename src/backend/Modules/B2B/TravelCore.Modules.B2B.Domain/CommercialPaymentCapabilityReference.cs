namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// Logical reference to declared payment capability intent in B2B.
/// Not a Payment execution contract and not a financial ledger model.
/// </summary>
public sealed class CommercialPaymentCapabilityReference
{
    private CommercialPaymentCapabilityReference()
    {
        CapabilityCode = null!;
    }

    private CommercialPaymentCapabilityReference(string capabilityCode)
    {
        CapabilityCode = NormalizeRequired(capabilityCode, nameof(capabilityCode), 64);
    }

    public string CapabilityCode { get; private set; }

    public static CommercialPaymentCapabilityReference FromCode(string capabilityCode) =>
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
