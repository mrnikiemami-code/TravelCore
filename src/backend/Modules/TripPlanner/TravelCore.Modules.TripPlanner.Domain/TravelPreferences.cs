namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Structured travel preferences owned by TripIntent (P18-R4).
/// </summary>
public sealed class TravelPreferences
{
    public const int TravelerNoteMaxLength = 500;

    private readonly List<DestinationPreference> _destinations = [];
    private readonly List<InterestPreference> _interests = [];

    private TravelPreferences()
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

    public static TravelPreferences Empty() => new();

    public void SetTiming(TravelTimingPreference timing)
    {
        Timing = timing ?? throw new ArgumentNullException(nameof(timing));
    }

    public void SetTravelers(PlannerTravelerComposition? travelers) => Travelers = travelers;

    public void SetBudget(BudgetPreference? budget) => Budget = budget;

    public void SetAccommodation(AccommodationPreferenceKind? accommodation) => Accommodation = accommodation;

    public void SetTransport(TransportPreferenceKind? transport) => Transport = transport;

    public void SetTripStyle(TripStylePreference? tripStyle) => TripStyle = tripStyle;

    public void SetTravelerNote(string? travelerNote)
    {
        if (travelerNote is null)
        {
            TravelerNote = null;
            return;
        }

        var trimmed = travelerNote.Trim();
        if (trimmed.Length == 0)
        {
            TravelerNote = null;
            return;
        }

        if (trimmed.Length > TravelerNoteMaxLength)
        {
            throw new ArgumentException($"Traveler note max length is {TravelerNoteMaxLength}.", nameof(travelerNote));
        }

        TravelerNote = trimmed;
    }

    public void ReplaceDestinations(IEnumerable<DestinationPreference> destinations)
    {
        ArgumentNullException.ThrowIfNull(destinations);
        _destinations.Clear();
        _destinations.AddRange(destinations.OrderBy(d => d.SortOrder));
    }

    public void ReplaceInterests(IEnumerable<InterestPreference> interests)
    {
        ArgumentNullException.ThrowIfNull(interests);
        _interests.Clear();
        _interests.AddRange(interests);
    }

    internal void ValidateForLeadSubmission()
    {
        Travelers?.ValidateForLeadSubmission();
    }

    internal TravelPreferenceSnapshot CaptureSnapshot()
    {
        return new TravelPreferenceSnapshot(
            Timing.CaptureCopy(),
            Travelers?.CaptureCopy(),
            Budget?.CaptureCopy(),
            Accommodation,
            Transport,
            TripStyle,
            TravelerNote,
            _destinations.Select(d => d.CaptureCopy()).ToList(),
            _interests.Select(i => i.CaptureCopy()).ToList());
    }
}
