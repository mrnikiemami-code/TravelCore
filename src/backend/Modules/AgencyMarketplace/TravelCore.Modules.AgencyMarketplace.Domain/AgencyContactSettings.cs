namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Public commercial contact for Marketplace. Not Party primary identity contact.
/// </summary>
public sealed class AgencyContactSettings
{
    public const int ContactMaxLength = 256;
    public const int WebsiteMaxLength = 512;

    private AgencyContactSettings()
    {
    }

    public AgencyContactSettings(string? publicEmail, string? publicPhone, string? websiteUrl)
    {
        PublicEmail = NormalizeOptional(publicEmail, nameof(publicEmail), ContactMaxLength);
        PublicPhone = NormalizeOptional(publicPhone, nameof(publicPhone), ContactMaxLength);
        WebsiteUrl = NormalizeOptional(websiteUrl, nameof(websiteUrl), WebsiteMaxLength);
    }

    public string? PublicEmail { get; private set; }

    public string? PublicPhone { get; private set; }

    public string? WebsiteUrl { get; private set; }

    public static AgencyContactSettings Empty() => new(null, null, null);

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
