namespace TravelCore.Modules.Notification.Domain;

/// <summary>
/// P25-R7 operational boundary marker. No fake production send success; internal ops only until explicit lock.
/// </summary>
public static class NotificationOperationalBoundary
{
    public const string FakeProductionSendSuccess = "NOT ALLOWED";
    public const string PublicOperationalApiPosture = "NOT IMPLEMENTED";
    public const string AdminOperationalApiPosture = "NOT IMPLEMENTED";
    public const string InternalReadOpsPosture = "BOUNDARY ONLY";

    public const bool OperationalBoundaryImplemented = true;
    public const bool PublicApiImplemented = false;
    public const bool AdminApiImplemented = false;
    public const bool FakeSendSuccessImplemented = false;
}
