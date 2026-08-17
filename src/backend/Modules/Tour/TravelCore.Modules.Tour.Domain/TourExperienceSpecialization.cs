using NodaTime;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Experience typed specialization on TourProduct (P09-R1 · P10-R1/R3/R4/R5/R6/R7 · TC-P10-T001…T007).
/// 1:1 with <see cref="TourProductId"/>. Owns itinerary, accommodation, meals (via days),
/// difficulty/eligibility/equipment/local-transport facts, and guide assignments.
/// Media Cover/Gallery = TourProduct media links (P09-R8 / P10-R4); Day/Stop media deferred. Package = P11.
/// </summary>
public sealed class TourExperienceSpecialization
{
    private ExperienceItinerary? _itinerary;
    private readonly List<ExperienceAccommodationPlanEntry> _accommodationPlan = [];
    private readonly List<ExperienceEligibilityRequirement> _eligibility = [];
    private readonly List<ExperienceEquipmentItem> _equipment = [];
    private readonly List<ExperienceLocalTransportItem> _localTransport = [];
    private readonly List<ExperienceGuideAssignment> _guideAssignments = [];

    private TourExperienceSpecialization()
    {
    }

    private TourExperienceSpecialization(TourProductId tourProductId, Instant createdAt)
    {
        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        TourProductId = tourProductId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>Same identity as the owning Experience <see cref="TourProduct"/> (1:1).</summary>
    public TourProductId TourProductId { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    /// <summary>Optional UX-level difficulty (P10-R6 · 0..1).</summary>
    public ExperienceDifficulty? Difficulty { get; private set; }

    /// <summary>Optional Experience-owned itinerary (P10-R1 · 0..1).</summary>
    public ExperienceItinerary? Itinerary => _itinerary;

    /// <summary>Accommodation plan entries (P10-R3 · 0..N). Not TourHotelOption.</summary>
    public IReadOnlyCollection<ExperienceAccommodationPlanEntry> AccommodationPlan => _accommodationPlan;

    public IReadOnlyList<ExperienceAccommodationPlanEntry> AccommodationPlanOrdered =>
        _accommodationPlan.OrderBy(x => x.SortOrder).ToList();

    public IReadOnlyCollection<ExperienceEligibilityRequirement> EligibilityRequirements => _eligibility;

    public IReadOnlyCollection<ExperienceEquipmentItem> Equipment => _equipment;

    public IReadOnlyCollection<ExperienceLocalTransportItem> LocalTransport => _localTransport;

    /// <summary>Guide assignments (P10-R7 · 0..N). Logical Party refs only.</summary>
    public IReadOnlyCollection<ExperienceGuideAssignment> GuideAssignments => _guideAssignments;

    /// <summary>
    /// Attaches Experience specialization to an Experience-kind TourProduct.
    /// Rejects Package (and any non-Experience kind) — no Package specialty in P10.
    /// </summary>
    public static TourExperienceSpecialization CreateFor(TourProduct product, Instant now)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (product.Kind != TourKind.Experience)
        {
            throw new InvalidOperationException(
                "TourExperienceSpecialization may only attach to TourKind.Experience products. Package specialty is out of P10 scope.");
        }

        return new TourExperienceSpecialization(product.Id, now);
    }

    /// <summary>Test / reconstitution helper when Kind has already been validated.</summary>
    public static TourExperienceSpecialization Reconstitute(
        TourProductId tourProductId,
        Instant createdAt,
        Instant updatedAt,
        ExperienceItinerary? itinerary = null,
        IEnumerable<ExperienceAccommodationPlanEntry>? accommodationPlan = null)
    {
        var specialization = new TourExperienceSpecialization(tourProductId, createdAt)
        {
            UpdatedAt = updatedAt
        };
        specialization._itinerary = itinerary;
        if (accommodationPlan is not null)
        {
            foreach (var entry in accommodationPlan.OrderBy(x => x.SortOrder))
            {
                specialization._accommodationPlan.Add(entry);
            }
        }

        return specialization;
    }

    /// <summary>
    /// Creates the Experience-owned itinerary (0..1). Idempotent if already present.
    /// </summary>
    public ExperienceItinerary EnsureItinerary(Instant now)
    {
        if (_itinerary is not null)
        {
            return _itinerary;
        }

        _itinerary = ExperienceItinerary.Create(TourProductId, now);
        UpdatedAt = now;
        return _itinerary;
    }

    public ExperienceAccommodationPlanEntry AddAccommodationPlanEntry(
        Instant now,
        Guid? placeId = null,
        int? sortOrder = null,
        ExperienceAccommodationPlanId? id = null)
    {
        if (_accommodationPlan.Count >= ExperienceAccommodationPlanEntry.MaxEntriesPerExperience)
        {
            throw new InvalidOperationException(
                $"An Experience may have at most {ExperienceAccommodationPlanEntry.MaxEntriesPerExperience} accommodation plan entries.");
        }

        var resolvedSort = sortOrder ?? NextAccommodationSortOrder();
        if (resolvedSort < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), resolvedSort, "SortOrder must be >= 0.");
        }

        if (_accommodationPlan.Any(x => x.SortOrder == resolvedSort))
        {
            throw new ArgumentException(
                $"Accommodation SortOrder {resolvedSort} is already used for this Experience.",
                nameof(sortOrder));
        }

        var entry = ExperienceAccommodationPlanEntry.Create(
            id ?? ExperienceAccommodationPlanId.New(),
            TourProductId,
            resolvedSort,
            placeId);
        _accommodationPlan.Add(entry);
        UpdatedAt = now;
        return entry;
    }

    public bool RemoveAccommodationPlanEntry(ExperienceAccommodationPlanId entryId, Instant now)
    {
        var entry = _accommodationPlan.FirstOrDefault(x => x.Id == entryId);
        if (entry is null)
        {
            return false;
        }

        _accommodationPlan.Remove(entry);
        UpdatedAt = now;
        return true;
    }

    public ExperienceDayMeal AddDayMeal(ItineraryDayId dayId, ExperienceMealType mealType, Instant now)
    {
        var itinerary = _itinerary
            ?? throw new InvalidOperationException("Experience itinerary was not found. Call EnsureItinerary first.");
        var day = itinerary.GetDay(dayId);
        var meal = day.AddMeal(mealType);
        UpdatedAt = now;
        itinerary.Touch(now);
        return meal;
    }

    public void SetDifficulty(ExperienceDifficulty? difficulty, Instant now)
    {
        if (difficulty is ExperienceDifficulty value && !Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Unsupported ExperienceDifficulty.");
        }

        Difficulty = difficulty;
        UpdatedAt = now;
    }

    public void ReplaceEligibilityRequirements(
        IEnumerable<(string Code, string? Value, string? Detail)> requirements,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(requirements);
        var normalized = requirements
            .Select(x => ExperienceEligibilityRequirement.Create(TourProductId, x.Code, x.Value, x.Detail))
            .GroupBy(x => x.Code, StringComparer.Ordinal)
            .Select(g => g.Last())
            .OrderBy(x => x.Code, StringComparer.Ordinal)
            .ToList();

        if (normalized.Count > ExperienceEligibilityRequirement.MaxEntriesPerExperience)
        {
            throw new ArgumentException(
                $"An Experience may have at most {ExperienceEligibilityRequirement.MaxEntriesPerExperience} eligibility requirements.",
                nameof(requirements));
        }

        _eligibility.Clear();
        _eligibility.AddRange(normalized);
        UpdatedAt = now;
    }

    public void ReplaceEquipment(
        IEnumerable<(string Code, ExperienceEquipmentKind Kind, string? Detail)> items,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(items);
        var normalized = items
            .Select(x => ExperienceEquipmentItem.Create(TourProductId, x.Code, x.Kind, x.Detail))
            .GroupBy(x => x.Code, StringComparer.Ordinal)
            .Select(g => g.Last())
            .OrderBy(x => x.Code, StringComparer.Ordinal)
            .ToList();

        if (normalized.Count > ExperienceEquipmentItem.MaxEntriesPerExperience)
        {
            throw new ArgumentException(
                $"An Experience may have at most {ExperienceEquipmentItem.MaxEntriesPerExperience} equipment items.",
                nameof(items));
        }

        _equipment.Clear();
        _equipment.AddRange(normalized);
        UpdatedAt = now;
    }

    public void ReplaceLocalTransport(
        IEnumerable<(string Code, string? Detail)> items,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(items);
        var normalized = items
            .Select(x => ExperienceLocalTransportItem.Create(TourProductId, x.Code, x.Detail))
            .GroupBy(x => x.Code, StringComparer.Ordinal)
            .Select(g => g.Last())
            .OrderBy(x => x.Code, StringComparer.Ordinal)
            .ToList();

        if (normalized.Count > ExperienceLocalTransportItem.MaxEntriesPerExperience)
        {
            throw new ArgumentException(
                $"An Experience may have at most {ExperienceLocalTransportItem.MaxEntriesPerExperience} local transport items.",
                nameof(items));
        }

        _localTransport.Clear();
        _localTransport.AddRange(normalized);
        UpdatedAt = now;
    }

    public ExperienceGuideAssignment AddGuideAssignment(
        Guid guidePartyId,
        ExperienceGuideRole role,
        Instant now,
        string? note = null,
        ExperienceGuideAssignmentId? id = null)
    {
        if (_guideAssignments.Count >= ExperienceGuideAssignment.MaxEntriesPerExperience)
        {
            throw new InvalidOperationException(
                $"An Experience may have at most {ExperienceGuideAssignment.MaxEntriesPerExperience} guide assignments.");
        }

        if (guidePartyId == Guid.Empty)
        {
            throw new ArgumentException("GuidePartyId cannot be empty.", nameof(guidePartyId));
        }

        if (_guideAssignments.Any(x => x.GuidePartyId == guidePartyId))
        {
            throw new ArgumentException(
                $"GuidePartyId '{guidePartyId}' is already assigned to this Experience.",
                nameof(guidePartyId));
        }

        var assignment = ExperienceGuideAssignment.Create(
            id ?? ExperienceGuideAssignmentId.New(),
            TourProductId,
            guidePartyId,
            role,
            note);
        _guideAssignments.Add(assignment);
        UpdatedAt = now;
        return assignment;
    }

    public bool RemoveGuideAssignment(ExperienceGuideAssignmentId assignmentId, Instant now)
    {
        var assignment = _guideAssignments.FirstOrDefault(x => x.Id == assignmentId);
        if (assignment is null)
        {
            return false;
        }

        _guideAssignments.Remove(assignment);
        UpdatedAt = now;
        return true;
    }

    public void Touch(Instant now) => UpdatedAt = now;

    private int NextAccommodationSortOrder()
    {
        if (_accommodationPlan.Count == 0)
        {
            return 0;
        }

        return _accommodationPlan.Max(x => x.SortOrder) + 1;
    }
}
