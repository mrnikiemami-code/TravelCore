namespace TravelCore.Hardening;

/// <summary>
/// P29 audit vs row metadata separation. Persistence row metadata remains; audit-event product is separate boundary.
/// </summary>
public static class HardeningRowMetadataInteractionBoundary
{
    public const string RowMetadataOwnedByModuleSchema = "Row metadata owned by module schema";
    public const string AuditEventStorageIsSeparateProduct = "Audit-event storage is separate product boundary";
    public const string PaymentAuditSnapshotRemainsInPayment =
        "Payment audit/snapshot facts remain in Payment module";
    public const string HardeningDoesNotOwnBusinessAuditFacts =
        "Hardening != business audit fact owner";

    public const bool RowMetadataInteractionBoundaryImplemented = true;
    public const bool AuditSchemaRequired = false;
    public const bool AuditEventPersistenceImplemented = false;
}
