namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// Logical reference for future agency reporting posture.
/// No reporting engine, no persistence, no mutation in this boundary.
/// </summary>
public sealed class AgencyReportingReference
{
    private AgencyReportingReference()
    {
        ReportingCode = null!;
    }

    private AgencyReportingReference(string reportingCode)
    {
        ReportingCode = NormalizeRequired(reportingCode, nameof(reportingCode), 64);
    }

    public string ReportingCode { get; private set; }

    public static AgencyReportingReference FromCode(string reportingCode) =>
        new(reportingCode);

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
