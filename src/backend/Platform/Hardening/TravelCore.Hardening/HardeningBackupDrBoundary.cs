namespace TravelCore.Hardening;

/// <summary>
/// P29-R5 backup/restore and DR foundation. Operational resilience posture without cloud backup product.
/// </summary>
public static class HardeningBackupDrBoundary
{
    public const string BackupRestoreIsOperationalPosture = "Backup/restore is operational posture boundary";
    public const string ModuleSchemasRequireBackupAwareness = "Module schemas require backup-awareness posture";
    public const string NoCloudBackupVendorLockIn = "No cloud backup vendor lock-in in Hardening module";
    public const string DrDoesNotRequireMultiRegionProduct =
        "DR posture != multi-region active-active product requirement";
    public const string RestoreDrillsDeferred = "Automated restore drills remain deferred in early P29 tasks";

    public const bool BackupDrBoundaryImplemented = true;
    public const bool CloudBackupVendorImplemented = false;
    public const bool AutomatedRestoreDrillProductImplemented = false;
    public const bool MultiRegionActiveActiveImplemented = false;
}
