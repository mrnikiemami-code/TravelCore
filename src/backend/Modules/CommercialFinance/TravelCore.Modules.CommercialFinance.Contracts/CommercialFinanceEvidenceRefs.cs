namespace TravelCore.Modules.CommercialFinance.Contracts;

/// <summary>
/// Read-only evidence reference shapes consumed by Commercial Finance (P39-T003).
/// Source domains remain SoT; Finance stores ids + optional snapshot metadata only.
/// </summary>
public static class CommercialFinanceEvidenceRefs
{
    public const string AgencyOfferEvidenceKind = "AgencyOffer";
    public const string BookingEvidenceKind = "Booking";
    public const string PaymentEvidenceKind = "Payment";
    public const string RefundEvidenceKind = "Refund";

    public const string EvidenceIsReadOnly = "Evidence consumption is read-only";
    public const string GovernanceAuditIsNotFinancialEvidence =
        "AgencyOffer governance audit != financial obligation evidence";
}

/// <summary>Logical AgencyOffer evidence reference (no AgencyMarketplace FK).</summary>
public readonly record struct CommercialFinanceAgencyOfferEvidenceRef(Guid AgencyOfferId);

/// <summary>Logical Booking evidence reference (no booking schema FK).</summary>
public readonly record struct CommercialFinanceBookingEvidenceRef(Guid BookingId);

/// <summary>Logical Payment evidence reference (no payment schema FK).</summary>
public readonly record struct CommercialFinancePaymentEvidenceRef(Guid PaymentId);

/// <summary>Logical AgencyProfile evidence reference (no agency_marketplace FK).</summary>
public readonly record struct CommercialFinanceAgencyProfileEvidenceRef(Guid AgencyProfileId);

/// <summary>Optional evidence snapshot metadata stored on obligations.</summary>
public readonly record struct CommercialFinanceEvidenceSnapshotRef(
    string EvidenceKind,
    string SnapshotHash,
    DateTimeOffset CapturedAt);
