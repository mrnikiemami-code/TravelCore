namespace TravelCore.Modules.AgencyMarketplace.Contracts;

/// <summary>
/// AgencyOffer policy decision foundation (TC-P38-T011).
/// Answers Allow/Deny + reason/code only — no financial math.
/// </summary>
public enum AgencyOfferPolicyDecisionKind : short
{
    Allow = 1,
    Deny = 2
}

public sealed record AgencyOfferPolicyDecision(
    AgencyOfferPolicyDecisionKind Kind,
    string Code,
    string Reason,
    string PolicyName)
{
    public bool IsAllowed => Kind == AgencyOfferPolicyDecisionKind.Allow;

    public static AgencyOfferPolicyDecision Allow(string policyName, string code = "ALLOW_DEFAULT", string reason = "Default allow.") =>
        new(AgencyOfferPolicyDecisionKind.Allow, code, reason, policyName);

    public static AgencyOfferPolicyDecision Deny(string policyName, string code, string reason) =>
        new(AgencyOfferPolicyDecisionKind.Deny, code, reason, policyName);
}

/// <summary>
/// Composite policy gate owned by AgencyMarketplace governance (not Pricing / Booking).
/// </summary>
public interface IAgencyOfferPolicyEvaluator
{
    Task<AgencyOfferPolicyDecision> EvaluateAsync(
        AgencyOfferPolicyContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Future publication-policy hook (Submit/Approve/Publish gates). Default Allow.
/// </summary>
public interface IAgencyOfferPublicationPolicy
{
    Task<AgencyOfferPolicyDecision> EvaluateAsync(
        AgencyOfferPolicyContext context,
        CancellationToken cancellationToken = default);
}
