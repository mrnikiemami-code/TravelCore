namespace TravelCore.Modules.Party.Domain;

/// <summary>
/// Agency commercial specialization owned by the Party aggregate.
/// Agency is not an authentication silo — credentials stay in Identity; authz in Access.
/// </summary>
public sealed class AgencyParty
{
    private AgencyParty()
    {
        TradingName = null!;
    }

    internal AgencyParty(PartyId partyId, string tradingName, string? licenseCode)
    {
        PartyId = partyId;
        TradingName = NormalizeRequired(tradingName, nameof(tradingName), 200);
        LicenseCode = NormalizeOptional(licenseCode, nameof(licenseCode), 64);
    }

    public PartyId PartyId { get; private set; }

    public string TradingName { get; private set; }

    /// <summary>
    /// Opaque commercial license/code string. Not a ReferenceData foreign key (P04 deferred).
    /// </summary>
    public string? LicenseCode { get; private set; }

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

    private static string? NormalizeOptional(string? value, string paramName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Length must be <= {maxLength}.");
        }

        return trimmed;
    }
}
