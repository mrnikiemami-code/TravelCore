namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Marketplace-facing display facts. Not Party identity DisplayName / TradingName.
/// Logo is a logical MediaAsset Guid only — no Media FK (P06-R5 contract-only posture).
/// </summary>
public sealed class AgencyDisplayInfo
{
    public const int DisplayNameMaxLength = 200;
    public const int DescriptionMaxLength = 4000;

    private AgencyDisplayInfo()
    {
        DisplayName = null!;
    }

    public AgencyDisplayInfo(string displayName, string? description, Guid? logoMediaAssetId)
    {
        DisplayName = NormalizeRequired(displayName, nameof(displayName), DisplayNameMaxLength);
        Description = NormalizeOptional(description, nameof(description), DescriptionMaxLength);
        if (logoMediaAssetId is { } logo && logo == Guid.Empty)
        {
            throw new ArgumentException("LogoMediaAssetId cannot be empty.", nameof(logoMediaAssetId));
        }

        LogoMediaAssetId = logoMediaAssetId;
    }

    public string DisplayName { get; private set; }

    public string? Description { get; private set; }

    /// <summary>Logical MediaAsset id. No Media schema FK.</summary>
    public Guid? LogoMediaAssetId { get; private set; }

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
