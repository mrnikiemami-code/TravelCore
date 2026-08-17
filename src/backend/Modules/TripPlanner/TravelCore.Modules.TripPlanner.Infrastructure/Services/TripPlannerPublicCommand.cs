using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.TripPlanner.Contracts;
using TravelCore.Modules.TripPlanner.Domain;

namespace TravelCore.Modules.TripPlanner.Infrastructure.Services;

/// <summary>
/// Anonymous public Trip Planner command service (TC-P18-T008 / P18-R8).
/// </summary>
internal sealed class TripPlannerPublicCommand : ITripPlannerPublicCommand
{
    private readonly TripPlannerDbContext _db;
    private readonly IClock _clock;

    public TripPlannerPublicCommand(TripPlannerDbContext db, IClock clock)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<TripPlannerCreateIntentResponse> CreateIntentAsync(
        TripPlannerCreateIntentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        _ = NormalizeLocaleCode(request.LocaleCode);

        var now = _clock.GetCurrentInstant();
        var intent = TripIntent.Create(now);
        _db.TripIntents.Add(intent);
        await _db.SaveChangesAsync(cancellationToken);

        return new TripPlannerCreateIntentResponse(
            intent.Id.Value,
            intent.DraftAccessToken.Value,
            TripPlannerPublicCompositionBoundary.PublicRoutePattern,
            intent.CreatedAt.ToString());
    }

    public async Task<TripPlannerIntentDraftResponse?> GetIntentAsync(
        Guid intentId,
        string draftAccessToken,
        CancellationToken cancellationToken = default)
    {
        var intent = await LoadAuthorizedIntentAsync(intentId, draftAccessToken, cancellationToken);
        return intent is null ? null : await MapDraftAsync(intent, cancellationToken);
    }

    public async Task<TripPlannerIntentDraftResponse?> UpdateIntentAsync(
        Guid intentId,
        string draftAccessToken,
        TripPlannerUpdateIntentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var intent = await LoadAuthorizedIntentAsync(intentId, draftAccessToken, cancellationToken);
        if (intent is null)
        {
            return null;
        }

        var now = _clock.GetCurrentInstant();
        if (request.PlanningNote is not null)
        {
            intent.UpdatePlanningNote(request.PlanningNote, now);
        }

        intent.UpdatePreferences(
            preferences => TripPlannerPublicPreferenceMapper.ApplyUpdate(preferences, request),
            now);

        await _db.SaveChangesAsync(cancellationToken);
        return await MapDraftAsync(intent, cancellationToken);
    }

    public async Task<TripPlannerSubmitLeadResponse?> SubmitLeadAsync(
        Guid intentId,
        string draftAccessToken,
        TripPlannerSubmitLeadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var intent = await LoadAuthorizedIntentAsync(intentId, draftAccessToken, cancellationToken);
        if (intent is null)
        {
            return null;
        }

        var existingLead = await _db.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SourceTripIntentId == intent.Id, cancellationToken);
        if (existingLead is not null)
        {
            return new TripPlannerSubmitLeadResponse(
                intent.Id.Value,
                existingLead.Id.Value,
                existingLead.Status.ToString(),
                existingLead.SubmittedAt.ToString(),
                AlreadySubmitted: true);
        }

        var now = _clock.GetCurrentInstant();
        var contact = LeadContactSnapshot.Create(
            request.DisplayName,
            request.Email,
            request.Phone);
        var consent = MapConsentOrNull(request, contact, now);
        var lead = intent.SubmitAsLead(now, contact, consent);
        _db.Leads.Add(lead);
        await _db.SaveChangesAsync(cancellationToken);

        return new TripPlannerSubmitLeadResponse(
            intent.Id.Value,
            lead.Id.Value,
            lead.Status.ToString(),
            lead.SubmittedAt.ToString(),
            AlreadySubmitted: false);
    }

    private async Task<TripIntent?> LoadAuthorizedIntentAsync(
        Guid intentId,
        string draftAccessToken,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(draftAccessToken);

        TripIntentDraftAccessToken token;
        try
        {
            token = TripIntentDraftAccessToken.FromStored(draftAccessToken);
        }
        catch (ArgumentException)
        {
            return null;
        }

        var intent = await _db.TripIntents
            .FirstOrDefaultAsync(x => x.Id == TripIntentId.From(intentId), cancellationToken);
        if (intent is null || !string.Equals(intent.DraftAccessToken.Value, token.Value, StringComparison.Ordinal))
        {
            return null;
        }

        return intent;
    }

    private async Task<TripPlannerIntentDraftResponse> MapDraftAsync(
        TripIntent intent,
        CancellationToken cancellationToken)
    {
        var lead = await _db.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SourceTripIntentId == intent.Id, cancellationToken);

        return new TripPlannerIntentDraftResponse(
            intent.Id.Value,
            intent.DraftAccessToken.Value,
            intent.PlanningRevision,
            intent.PlanningNote,
            TripPlannerPublicPreferenceMapper.ToDraft(intent.Preferences),
            intent.CreatedAt.ToString(),
            intent.UpdatedAt.ToString(),
            lead is not null,
            lead?.Id.Value,
            TripPlannerPublicCompositionBoundary.PublicRoutePattern);
    }

    private static LeadConsentSnapshot? MapConsentOrNull(
        TripPlannerSubmitLeadRequest request,
        LeadContactSnapshot contact,
        Instant capturedAt)
    {
        if (request.FollowUpContactAllowed is null
            && request.MarketingAllowed is null
            && request.PrivacyNoticeVersion is null
            && request.PreferredContactChannel is null)
        {
            return null;
        }

        var followUp = request.FollowUpContactAllowed ?? contact != LeadContactSnapshot.Empty;
        var marketing = request.MarketingAllowed ?? false;
        LeadContactChannelPreference? channel = null;
        if (!string.IsNullOrWhiteSpace(request.PreferredContactChannel)
            && Enum.TryParse<LeadContactChannelPreference>(
                request.PreferredContactChannel,
                ignoreCase: true,
                out var parsed))
        {
            channel = parsed;
        }

        return LeadConsentSnapshot.Create(
            followUp,
            marketing,
            request.PrivacyNoticeVersion,
            channel,
            capturedAt);
    }

    private static string? NormalizeLocaleCode(string? localeCode)
    {
        if (string.IsNullOrWhiteSpace(localeCode))
        {
            return null;
        }

        var trimmed = localeCode.Trim();
        if (trimmed.Length is < 2 or > 8)
        {
            throw new ArgumentException("Locale code must be 2..8 characters.", nameof(localeCode));
        }

        return trimmed;
    }
}
