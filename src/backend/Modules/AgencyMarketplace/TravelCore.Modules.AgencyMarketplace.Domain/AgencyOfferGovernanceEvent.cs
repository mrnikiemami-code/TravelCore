using NodaTime;

namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Operational governance history for an AgencyOffer (TC-P38-T013).
/// Not an accounting / commission / settlement ledger.
/// </summary>
public enum AgencyOfferGovernanceEventKind : short
{
    Submitted = 1,
    Approved = 2,
    Rejected = 3,
    Published = 4,
    Unpublished = 5,
    Suspended = 6,
    Retired = 7,
    PolicyDenied = 8
}

public sealed class AgencyOfferGovernanceEvent
{
    private AgencyOfferGovernanceEvent()
    {
        ActorKind = null!;
    }

    private AgencyOfferGovernanceEvent(
        Guid id,
        AgencyOfferId offerId,
        AgencyProfileId agencyProfileId,
        AgencyOfferGovernanceEventKind kind,
        string actorKind,
        Guid? actorAccountId,
        string? fromPublicationStatus,
        string? toPublicationStatus,
        string? policyCode,
        string? policyName,
        string? reason,
        Instant occurredAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(actorKind))
        {
            throw new ArgumentException("ActorKind is required.", nameof(actorKind));
        }

        Id = id;
        OfferId = offerId;
        AgencyProfileId = agencyProfileId;
        Kind = kind;
        ActorKind = actorKind.Trim();
        ActorAccountId = actorAccountId;
        FromPublicationStatus = fromPublicationStatus;
        ToPublicationStatus = toPublicationStatus;
        PolicyCode = policyCode;
        PolicyName = policyName;
        Reason = reason;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; }

    public AgencyOfferId OfferId { get; private set; }

    public AgencyProfileId AgencyProfileId { get; private set; }

    public AgencyOfferGovernanceEventKind Kind { get; private set; }

    public string ActorKind { get; private set; }

    public Guid? ActorAccountId { get; private set; }

    public string? FromPublicationStatus { get; private set; }

    public string? ToPublicationStatus { get; private set; }

    public string? PolicyCode { get; private set; }

    public string? PolicyName { get; private set; }

    public string? Reason { get; private set; }

    public Instant OccurredAt { get; private set; }

    public static AgencyOfferGovernanceEvent Create(
        AgencyOfferId offerId,
        AgencyProfileId agencyProfileId,
        AgencyOfferGovernanceEventKind kind,
        string actorKind,
        Guid? actorAccountId = null,
        string? fromPublicationStatus = null,
        string? toPublicationStatus = null,
        string? policyCode = null,
        string? policyName = null,
        string? reason = null,
        Instant? occurredAt = null) =>
        new(
            Guid.CreateVersion7(),
            offerId,
            agencyProfileId,
            kind,
            actorKind,
            actorAccountId,
            fromPublicationStatus,
            toPublicationStatus,
            policyCode,
            policyName,
            reason,
            occurredAt ?? SystemClock.Instance.GetCurrentInstant());
}
