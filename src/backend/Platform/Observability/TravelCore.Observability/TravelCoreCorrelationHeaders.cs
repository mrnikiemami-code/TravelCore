namespace TravelCore.Observability;

/// <summary>
/// Canonical application-level correlation header (HTTP comparison is case-insensitive).
/// </summary>
public static class TravelCoreCorrelationHeaders
{
    public const string CorrelationId = "X-Correlation-ID";

    /// <summary>
    /// Maximum accepted length for a caller-supplied correlation identifier.
    /// </summary>
    public const int MaxLength = 128;
}
