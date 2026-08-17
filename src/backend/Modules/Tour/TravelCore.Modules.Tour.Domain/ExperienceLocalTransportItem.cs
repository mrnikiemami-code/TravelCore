namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Local transport fact for an Experience itinerary/product (TC-P10-T005 plan scope).
/// Not FlightSegment / live Flight / carrier product (P11).
/// </summary>
public sealed class ExperienceLocalTransportItem
{
    public const int MaxEntriesPerExperience = 32;

    private ExperienceLocalTransportItem()
    {
        Code = null!;
    }

    private ExperienceLocalTransportItem(TourProductId tourProductId, string code, string? detail)
    {
        TourProductId = tourProductId;
        Code = code;
        Detail = detail;
    }

    public TourProductId TourProductId { get; private set; }

    public string Code { get; private set; }

    public string? Detail { get; private set; }

    internal static ExperienceLocalTransportItem Create(TourProductId tourProductId, string code, string? detail)
    {
        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        return new ExperienceLocalTransportItem(
            tourProductId,
            TourCatalogFactCode.NormalizeCode(code),
            TourCatalogFactCode.NormalizeDetail(detail));
    }

    public static ExperienceLocalTransportItem Reconstitute(
        TourProductId tourProductId,
        string code,
        string? detail)
        => Create(tourProductId, code, detail);
}
