namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Non-price commercial metadata for an offer. Not Pricing, not commission, not Booking Amount.
/// </summary>
public sealed class AgencyOfferCommercialTerms
{
    public const int NotesMaxLength = 2000;

    public AgencyOfferCommercialTerms(string? notes)
    {
        Notes = NormalizeOptional(notes, nameof(notes), NotesMaxLength);
    }

    public string? Notes { get; private set; }

    public static AgencyOfferCommercialTerms Empty() => new(null);

    private AgencyOfferCommercialTerms()
    {
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
