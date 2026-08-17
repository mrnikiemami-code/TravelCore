namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Non-price commercial metadata for an offer (TC-P13-T004 / P13-R4).
/// Notes + SalesRules only. Not Pricing, not commission, not Booking Amount.
/// </summary>
public sealed class AgencyOfferCommercialTerms
{
    public const int NotesMaxLength = 2000;

    public AgencyOfferCommercialTerms(string? notes, AgencyOfferSalesRules? salesRules = null)
    {
        Notes = NormalizeOptional(notes, nameof(notes), NotesMaxLength);
        SalesRules = salesRules ?? AgencyOfferSalesRules.Default();
    }

    public string? Notes { get; private set; }

    public AgencyOfferSalesRules SalesRules { get; private set; }

    public static AgencyOfferCommercialTerms Empty() => new(null, AgencyOfferSalesRules.Default());

    private AgencyOfferCommercialTerms()
    {
        SalesRules = null!;
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
