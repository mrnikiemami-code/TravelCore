namespace TravelCore.Modules.Place.Domain;

/// <summary>
/// Attraction catalog specialization (1:1 with <see cref="Place"/> via <see cref="PlaceId"/>).
/// </summary>
public sealed class Attraction
{
    public const int CategoryCodeMaxLength = 64;

    private Attraction()
    {
        CategoryCode = null;
    }

    private Attraction(PlaceId placeId, string? categoryCode)
    {
        PlaceId = placeId;
        CategoryCode = categoryCode;
    }

    public PlaceId PlaceId { get; private set; }

    /// <summary>Optional attraction category code (catalog only).</summary>
    public string? CategoryCode { get; private set; }

    public static Attraction Create(PlaceId placeId, string? categoryCode = null)
    {
        if (placeId.Value == Guid.Empty)
        {
            throw new ArgumentException("PlaceId cannot be empty.", nameof(placeId));
        }

        return new Attraction(placeId, NormalizeCategoryCode(categoryCode));
    }

    public static string? NormalizeCategoryCode(string? categoryCode)
    {
        if (string.IsNullOrWhiteSpace(categoryCode))
        {
            return null;
        }

        var trimmed = categoryCode.Trim();
        if (trimmed.Length > CategoryCodeMaxLength)
        {
            throw new ArgumentException(
                $"CategoryCode max length is {CategoryCodeMaxLength}.",
                nameof(categoryCode));
        }

        return trimmed;
    }
}
