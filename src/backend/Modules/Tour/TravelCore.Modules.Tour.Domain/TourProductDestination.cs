namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Tour-owned logical Destination reference (P09-R2: 0..N; no cross-schema FK).
/// </summary>
public sealed class TourProductDestination
{
    public const int MaxLinksPerTourProduct = 32;

    private TourProductDestination()
    {
    }

    private TourProductDestination(TourProductId tourProductId, Guid destinationId)
    {
        TourProductId = tourProductId;
        DestinationId = destinationId;
    }

    public TourProductId TourProductId { get; private set; }

    /// <summary>Logical Destination identity only — never an EF navigation / cross-schema FK.</summary>
    public Guid DestinationId { get; private set; }

    internal static TourProductDestination Create(TourProductId tourProductId, Guid destinationId)
    {
        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        if (destinationId == Guid.Empty)
        {
            throw new ArgumentException("DestinationId cannot be empty.", nameof(destinationId));
        }

        return new TourProductDestination(tourProductId, destinationId);
    }
}
