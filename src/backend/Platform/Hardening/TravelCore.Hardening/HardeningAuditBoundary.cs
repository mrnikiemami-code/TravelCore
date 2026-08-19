namespace TravelCore.Hardening;

/// <summary>
/// P29-R3 audit trail foundation. Compliance event posture without audit-event storage product or SIEM integration.
/// </summary>
public static class HardeningAuditBoundary
{
    public const string RowMetadataIsNotAuditEventProduct = "Row metadata != audit-event product";
    public const string HighRiskBusinessAuditEventsDeferredToBoundary =
        "High-risk business audit events require explicit boundary before storage product";
    public const string NoCrossModuleAuditMegaTable = "No cross-module audit mega-table without ADR";
    public const string AuditEventsDoNotReplaceDomainTransactions =
        "Audit events do not replace domain transaction SoR";
    public const string SiemIntegrationDeferred = "SIEM integration remains deferred in early P29 tasks";

    public const bool AuditBoundaryImplemented = true;
    public const bool AuditEventStoreImplemented = false;
    public const bool SiemIntegrationImplemented = false;
    public const bool CrossModuleAuditMegaTableImplemented = false;
    public const bool ImmutableAuditLogProductImplemented = false;
}
