namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// Logical reference to payment responsibility posture for agency commerce.
/// Payment execution remains owned by Payment module.
/// </summary>
public sealed class PaymentResponsibilityReference
{
    private PaymentResponsibilityReference()
    {
        ResponsibilityCode = null!;
    }

    private PaymentResponsibilityReference(string responsibilityCode)
    {
        ResponsibilityCode = NormalizeRequired(responsibilityCode, nameof(responsibilityCode), 64);
    }

    public string ResponsibilityCode { get; private set; }

    public static PaymentResponsibilityReference FromCode(string responsibilityCode) =>
        new(responsibilityCode);

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
