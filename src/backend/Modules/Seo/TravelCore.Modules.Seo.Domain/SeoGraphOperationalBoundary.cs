namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// P26-R7 public/admin operational boundary for graph tooling.
/// </summary>
public static class SeoGraphOperationalBoundary
{
    public const string InternalReadOpsOnly = "Internal read/ops posture only until explicit product lock";
    public const string NoPublicGraphMutationApi = "No public graph mutation API by default";
    public const string NoFakeIndexSuccess = "No fake index success";

    public const bool OperationalBoundaryImplemented = true;
    public const bool PublicGraphMutationApiImplemented = false;
    public const bool FakeIndexSuccessImplemented = false;
}
