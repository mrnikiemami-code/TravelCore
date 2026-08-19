namespace TravelCore.Modules.Analytics.Contracts;

/// <summary>
/// Canonical product-intent event kinds owned by Analytics (P27-R2 / ROADMAP P27). Not vendor taxonomy.
/// </summary>
public enum AnalyticsProductEventKind
{
    Unknown = 0,
    SearchPerformed = 1,
    SearchResultClicked = 2,
    SearchNoResults = 3,
    FilterApplied = 4,
    TourViewed = 5,
    HotelViewed = 6,
    QuoteCreated = 7,
    BookingStarted = 8,
    BookingCompleted = 9,
}

/// <summary>
/// Neutral semantic product event envelope from publisher modules. Analytics owns taxonomy; publishers emit facts/intent only.
/// </summary>
public sealed record AnalyticsSemanticEventEnvelope(
    AnalyticsProductEventKind EventKind,
    string SourceModule,
    string CorrelationReference,
    string ResourceReference,
    string OccurredAtReference);
