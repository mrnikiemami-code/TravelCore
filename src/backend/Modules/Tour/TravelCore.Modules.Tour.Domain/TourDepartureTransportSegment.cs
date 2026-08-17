namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Descriptive transport segment on a TourDeparture (P11-R5 · TC-P11-T005).
/// Logical itinerary fact only — not Flight entity, airline, ticket, seat inventory, or Booking.
/// Origin/Destination are opaque display labels (not DestinationId FKs).
/// </summary>
public sealed class TourDepartureTransportSegment
{
    public const int LabelMaxLength = 200;

    private TourDepartureTransportSegment()
    {
        Origin = null!;
        Destination = null!;
    }

    private TourDepartureTransportSegment(
        TourDepartureTransportSegmentId id,
        TourDepartureId tourDepartureId,
        int sequence,
        TourDepartureTransportMode transportMode,
        string origin,
        string destination)
    {
        Id = id;
        TourDepartureId = tourDepartureId;
        Sequence = sequence;
        TransportMode = transportMode;
        Origin = origin;
        Destination = destination;
    }

    public TourDepartureTransportSegmentId Id { get; private set; }

    public TourDepartureId TourDepartureId { get; private set; }

    public int Sequence { get; private set; }

    public TourDepartureTransportMode TransportMode { get; private set; }

    public string Origin { get; private set; }

    public string Destination { get; private set; }

    internal static TourDepartureTransportSegment Create(
        TourDepartureId tourDepartureId,
        int sequence,
        TourDepartureTransportMode transportMode,
        string origin,
        string destination)
    {
        if (sequence < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sequence), sequence, "Sequence must be >= 1.");
        }

        if (!Enum.IsDefined(transportMode))
        {
            throw new ArgumentOutOfRangeException(nameof(transportMode), transportMode, "Unsupported transport mode.");
        }

        var from = NormalizeLabel(origin, nameof(origin));
        var to = NormalizeLabel(destination, nameof(destination));

        return new TourDepartureTransportSegment(
            TourDepartureTransportSegmentId.New(),
            tourDepartureId,
            sequence,
            transportMode,
            from,
            to);
    }

    private static string NormalizeLabel(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        var trimmed = value.Trim();
        if (trimmed.Length > LabelMaxLength)
        {
            throw new ArgumentException($"Label max length is {LabelMaxLength}.", paramName);
        }

        return trimmed;
    }
}
