using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.B2B.Contracts;
using TravelCore.Modules.B2B.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P24-T003 / P24-R3: agency membership intent references Access subjects without owning users/auth/authz.
/// </summary>
public sealed class B2BAgencyMembershipAccessGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void B2BDomain_Exposes_Membership_Access_Boundary_Models()
    {
        Assert.NotNull(typeof(AgencyMemberReference));
        Assert.NotNull(typeof(AgencyAccessRelationshipBoundary));
        Assert.True(B2BOwnershipBoundary.AgencyMemberReferenceImplemented);
        Assert.True(B2BOwnershipBoundary.AgencyAccessRelationshipBoundaryImplemented);
        Assert.False(B2BOwnershipBoundary.OwnsUsers);
        Assert.False(B2BOwnershipBoundary.OwnsAuthentication);
        Assert.False(B2BOwnershipBoundary.OwnsAuthorizationPolicies);
        Assert.False(B2BOwnershipBoundary.OwnsInvitationFlow);
    }

    [Fact]
    public void B2B_Does_Not_Own_Users_Authentication_Or_Authorization()
    {
        Assert.False(AgencyAccessRelationshipBoundary.B2BOwnsUsers);
        Assert.False(AgencyAccessRelationshipBoundary.B2BOwnsAuthentication);
        Assert.False(AgencyAccessRelationshipBoundary.B2BOwnsAuthorization);
        Assert.Equal("Identity", AgencyAccessRelationshipBoundary.UserIdentityOwner);
        Assert.Equal("Access", AgencyAccessRelationshipBoundary.AuthorizationOwner);
        Assert.Equal("Party", AgencyAccessRelationshipBoundary.OrganizationRelationshipOwner);
    }

    [Fact]
    public void B2B_T003_Forbids_Membership_Persistence_And_Access_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "B2B");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(AgencyMember|AgencyUser|User|Role|Permission|Invitation|AccessPolicy)\b",
            RegexOptions.Compiled);

        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return forbiddenType.IsMatch(x.line);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(hits.Count == 0, "B2B T003 forbids membership persistence/access product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void B2B_T003_Has_No_Migration_Or_Membership_Tables()
    {
        var migrationsDir = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "B2B",
            "TravelCore.Modules.B2B.Infrastructure",
            "Migrations");
        var migrationFiles = Directory.Exists(migrationsDir)
            ? Directory.GetFiles(migrationsDir, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith("ModelSnapshot.cs", StringComparison.OrdinalIgnoreCase)
                    && !f.Contains("InitialB2BScaffolding", StringComparison.OrdinalIgnoreCase))
                .ToList()
            : [];
        Assert.Empty(migrationFiles);
        Assert.False(AgencyAccessRelationshipBoundary.MembershipPersistenceImplemented);
        Assert.False(AgencyAccessRelationshipBoundary.AgencyMemberTableImplemented);
    }

    [Fact]
    public void P24_Evidence_Records_T003_And_R3()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P24-implementation-plan.md"));
        Assert.Contains("TC-P24-T003", plan, StringComparison.Ordinal);
        Assert.Contains("P24-R3", plan, StringComparison.Ordinal);
        Assert.Contains("AgencyMemberReference", plan, StringComparison.Ordinal);
        Assert.Contains("AgencyAccessRelationshipBoundary", plan, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
