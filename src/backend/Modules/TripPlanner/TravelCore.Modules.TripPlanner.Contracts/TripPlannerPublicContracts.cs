namespace TravelCore.Modules.TripPlanner.Contracts;

/// <summary>
/// Anonymous public Trip Planner command contracts (TC-P18-T008 / P18-R8).
/// Draft token secures anonymous TripIntent access — not identity authentication.
/// </summary>
public sealed record TripPlannerCreateIntentRequest(string? LocaleCode);

public sealed record TripPlannerCreateIntentResponse(
    Guid IntentId,
    string DraftAccessToken,
    string PublicPath,
    string CreatedAt);

public sealed record TripPlannerTimingDraft(
    string Kind,
    string? ExactStartDate,
    string? ExactEndDate,
    string? FlexibleEarliestStart,
    string? FlexibleLatestStart,
    int? FlexibleMaxTripDurationDays,
    int? ApproximateYear,
    int? ApproximateMonth,
    string? ApproximateSeason);

public sealed record TripPlannerTravelersDraft(
    int AdultCount,
    int ChildCount,
    int InfantCount);

public sealed record TripPlannerBudgetDraft(
    decimal? MinimumAmount,
    decimal? MaximumAmount,
    string CurrencyCode);

public sealed record TripPlannerDestinationDraft(
    bool Undecided,
    IReadOnlyList<Guid>? LogicalDestinationIds);

public sealed record TripPlannerPreferencesDraft(
    TripPlannerTimingDraft Timing,
    TripPlannerTravelersDraft? Travelers,
    TripPlannerBudgetDraft? Budget,
    string? Accommodation,
    string? Transport,
    string? TripStyle,
    IReadOnlyList<string>? InterestCodes,
    TripPlannerDestinationDraft Destination,
    string? TravelerNote);

public sealed record TripPlannerIntentDraftResponse(
    Guid IntentId,
    string DraftAccessToken,
    int PlanningRevision,
    string? PlanningNote,
    TripPlannerPreferencesDraft Preferences,
    string CreatedAt,
    string UpdatedAt,
    bool LeadSubmitted,
    Guid? LeadId,
    string PublicPath);

public sealed record TripPlannerUpdateIntentRequest(
    string? PlanningNote,
    TripPlannerTimingDraft? Timing,
    TripPlannerTravelersDraft? Travelers,
    TripPlannerBudgetDraft? Budget,
    string? Accommodation,
    string? Transport,
    string? TripStyle,
    IReadOnlyList<string>? InterestCodes,
    TripPlannerDestinationDraft? Destination,
    string? TravelerNote);

public sealed record TripPlannerSubmitLeadRequest(
    string? DisplayName,
    string? Email,
    string? Phone,
    bool? FollowUpContactAllowed,
    bool? MarketingAllowed,
    string? PrivacyNoticeVersion,
    string? PreferredContactChannel);

public sealed record TripPlannerSubmitLeadResponse(
    Guid IntentId,
    Guid LeadId,
    string LeadStatus,
    string SubmittedAt,
    bool AlreadySubmitted);

/// <summary>
/// Anonymous public Trip Planner commands — TripIntent draft lifecycle and Lead submission.
/// </summary>
public interface ITripPlannerPublicCommand
{
    Task<TripPlannerCreateIntentResponse> CreateIntentAsync(
        TripPlannerCreateIntentRequest request,
        CancellationToken cancellationToken = default);

    Task<TripPlannerIntentDraftResponse?> GetIntentAsync(
        Guid intentId,
        string draftAccessToken,
        CancellationToken cancellationToken = default);

    Task<TripPlannerIntentDraftResponse?> UpdateIntentAsync(
        Guid intentId,
        string draftAccessToken,
        TripPlannerUpdateIntentRequest request,
        CancellationToken cancellationToken = default);

    Task<TripPlannerSubmitLeadResponse?> SubmitLeadAsync(
        Guid intentId,
        string draftAccessToken,
        TripPlannerSubmitLeadRequest request,
        CancellationToken cancellationToken = default);
}
