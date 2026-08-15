namespace TravelCore.Modules.Party.Domain;

/// <summary>
/// Organization specialization owned by the Party aggregate.
/// </summary>
public sealed class OrganizationParty
{
    private OrganizationParty()
    {
        LegalName = null!;
    }

    internal OrganizationParty(PartyId partyId, string legalName, string? tradeName)
    {
        PartyId = partyId;
        LegalName = NormalizeRequired(legalName, nameof(legalName), 200);
        TradeName = NormalizeOptional(tradeName, nameof(tradeName), 200);
    }

    public PartyId PartyId { get; private set; }

    public string LegalName { get; private set; }

    public string? TradeName { get; private set; }

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
