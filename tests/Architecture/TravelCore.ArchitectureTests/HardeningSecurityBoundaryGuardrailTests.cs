using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Hardening;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P29-T003: security/authorization review boundary without identity provider or permission engine product.
/// </summary>
public sealed class HardeningSecurityBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void HardeningSecurityBoundary_Is_Declared()
    {
        Assert.True(HardeningSecurityBoundary.SecurityBoundaryImplemented);
        Assert.Equal("Security from day one is mandatory", HardeningSecurityBoundary.SecurityFromDayOneMandatory);
        Assert.Equal("Domain modules own authorization facts", HardeningSecurityBoundary.DomainOwnsAuthorizationFacts);
        Assert.Equal(
            "Platform owns cross-cutting security posture contracts",
            HardeningSecurityBoundary.PlatformOwnsCrossCuttingSecurityPosture);
        Assert.True(HardeningFoundationBoundary.SecurityBoundaryImplemented);
    }

    [Fact]
    public void HardeningDomainAuthorizationInteractionBoundary_Preserves_Domain_Ownership()
    {
        Assert.True(HardeningDomainAuthorizationInteractionBoundary.DomainAuthorizationInteractionBoundaryImplemented);
        Assert.Equal(
            "Access module owns permission model facts",
            HardeningDomainAuthorizationInteractionBoundary.AccessModuleOwnsPermissionModel);
        Assert.Equal(
            "Identity module owns identity facts",
            HardeningDomainAuthorizationInteractionBoundary.IdentityModuleOwnsIdentityFacts);
        Assert.Equal(
            "Hardening != Domain authorization replacement",
            HardeningDomainAuthorizationInteractionBoundary.HardeningDoesNotReplaceDomainAuthorization);
        Assert.Equal(
            "Hardening != DomainAuthorization",
            HardeningOwnershipBoundary.HardeningIsNotDomainAuthorization);
        Assert.False(HardeningDomainAuthorizationInteractionBoundary.IdentityModuleReferenceRequired);
        Assert.False(HardeningDomainAuthorizationInteractionBoundary.AccessModuleReferenceRequired);
    }

    [Fact]
    public void HardeningModule_DoesNot_Reference_Identity_Or_Access()
    {
        var hardening = Projects.Single(p => p.Name == "TravelCore.Hardening");
        var hits = hardening.ProjectReferences
            .Where(r => r.StartsWith("TravelCore.Modules.Identity", StringComparison.Ordinal)
                || r.StartsWith("TravelCore.Modules.Access", StringComparison.Ordinal))
            .ToList();
        Assert.True(hits.Count == 0, "Hardening must not reference Identity/Access:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Hardening_T003_Forbids_Identity_Provider_And_Permission_Engine_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Platform", "Hardening");
        var pattern = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(OAuthClient|OidcClient|IdentityProviderAdapter|PermissionEngine|AuthorizationService|JwtIssuer|TokenService|RoleManager|PolicyEvaluator|SecurityReviewAutomation)\b",
            RegexOptions.Compiled);

        var hits = Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .SelectMany(path =>
            {
                var text = File.ReadAllText(path);
                return pattern.Matches(text).Select(m => $"{path}: {m.Value}");
            })
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Hardening T003 forbids early identity/authorization product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void P29_Evidence_Records_T003_And_Security_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P29-implementation-plan.md"));
        Assert.Contains("TC-P29-T003", plan, StringComparison.Ordinal);
        Assert.Contains("HardeningSecurityBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("HardeningDomainAuthorizationInteractionBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("P29-R1", plan, StringComparison.Ordinal);
    }
}
