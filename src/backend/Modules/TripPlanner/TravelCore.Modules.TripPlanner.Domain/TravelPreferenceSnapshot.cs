namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Immutable preference snapshot copied onto Lead at submission (P18-R2 + P18-R4).
/// </summary>
public sealed class TravelPreferenceSnapshot
{
    private readonly List<DestinationPreference> _destinations = [];
    private readonly List<InterestPreference> _interests = [];

    internal TravelPreferenceSnapshot(
        TravelTimingPreference timing,
        PlannerTravelerComposition? travelers,
        BudgetPreference? budget,
        AccommodationPreferenceKind? accommodation,
        TransportPreferenceKind? transport,
        TripStylePreference? tripStyle,
        string? travelerNote,
        IReadOnlyList<DestinationPreference> destinations,
        IReadOnlyList<InterestPreference> interests)
    {
        Timing = timing;
        Travelers = travelers;
        Budget = budget;
        Accommodation = accommodation;
        Transport = transport;
        TripStyle = tripStyle;
        TravelerNote = travelerNote;
        _destinations.AddRange(destinations.Select(d => d.CaptureCopy()));
        _interests.AddRange(interests.Select(i => i.CaptureCopy()));
    }

    private TravelPreferenceSnapshot()
    {
        Timing = TravelTimingPreference.Undecided();
    }

    public TravelTimingPreference Timing { get; private set; }

    public PlannerTravelerComposition? Travelers { get; private set; }

    public BudgetPreference? Budget { get; private set; }

    public AccommodationPreferenceKind? Accommodation { get; private set; }

    public TransportPreferenceKind? Transport { get; private set; }

    public TripStylePreference? TripStyle { get; private set; }

    public string? TravelerNote { get; private set; }

    public IReadOnlyList<DestinationPreference> Destinations => _destinations;

    public IReadOnlyList<InterestPreference> Interests => _interests;
}
