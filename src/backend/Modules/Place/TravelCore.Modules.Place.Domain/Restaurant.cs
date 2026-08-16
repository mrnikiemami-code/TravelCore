namespace TravelCore.Modules.Place.Domain;

/// <summary>
/// Restaurant catalog specialization (1:1 with <see cref="Place"/> via <see cref="PlaceId"/>).
/// </summary>
public sealed class Restaurant
{
    public const int CuisineTypeMaxLength = 64;

    private Restaurant()
    {
        CuisineType = null;
    }

    private Restaurant(PlaceId placeId, string? cuisineType)
    {
        PlaceId = placeId;
        CuisineType = cuisineType;
    }

    public PlaceId PlaceId { get; private set; }

    /// <summary>Optional cuisine classification code/label (catalog only).</summary>
    public string? CuisineType { get; private set; }

    public static Restaurant Create(PlaceId placeId, string? cuisineType = null)
    {
        if (placeId.Value == Guid.Empty)
        {
            throw new ArgumentException("PlaceId cannot be empty.", nameof(placeId));
        }

        return new Restaurant(placeId, NormalizeCuisineType(cuisineType));
    }

    public static string? NormalizeCuisineType(string? cuisineType)
    {
        if (string.IsNullOrWhiteSpace(cuisineType))
        {
            return null;
        }

        var trimmed = cuisineType.Trim();
        if (trimmed.Length > CuisineTypeMaxLength)
        {
            throw new ArgumentException(
                $"CuisineType max length is {CuisineTypeMaxLength}.",
                nameof(cuisineType));
        }

        return trimmed;
    }
}
