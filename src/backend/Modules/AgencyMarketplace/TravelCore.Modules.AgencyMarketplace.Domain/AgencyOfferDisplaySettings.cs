namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Marketplace listing display metadata. Does not copy TourProduct content SoR.
/// </summary>
public sealed class AgencyOfferDisplaySettings
{
    public const int TitleMaxLength = 200;
    public const int HighlightMaxLength = 500;

    public AgencyOfferDisplaySettings(string? titleOverride, string? highlight)
    {
        TitleOverride = NormalizeOptional(titleOverride, nameof(titleOverride), TitleMaxLength);
        Highlight = NormalizeOptional(highlight, nameof(highlight), HighlightMaxLength);
    }

    public string? TitleOverride { get; private set; }

    public string? Highlight { get; private set; }

    public static AgencyOfferDisplaySettings Empty() => new(null, null);

    private AgencyOfferDisplaySettings()
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
