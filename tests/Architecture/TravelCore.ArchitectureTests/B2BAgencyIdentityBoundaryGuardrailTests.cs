using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.B2B.Contracts;
using TravelCore.Modules.B2B.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P24-T002 / P24-R2: Agency business identity boundary without Party/Identity/Access ownership transfer.
/// </summary>
public sealed class B2BAgencyIdentityBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void B2BDomain_Exposes_AgencyBoundary_Models()
    {
        Assert.NotNull(typeof(AgencyReference));
        Assert.NotNull(typeof(AgencyReferenceId));
        Assert.NotNull(typeof(AgencyRelationshipBoundary));
        Assert.NotNull(typeof(AgencyMembershipBoundary));
        Assert.NotNull(typeof(AccessSubjectReferenceId));
        Assert.True(B2BOwnershipBoundary.AgencyReferenceBoundaryImplemented);
        Assert.True(B2BOwnershipBoundary.AgencyMembershipBoundaryImplemented);
        Assert.True(B2BOwnershipBoundary.AgencyRelationshipBoundaryImplemented);
        Assert.False(B2BOwnershipBoundary.AgencyEntityImplemented);
    }

    [Fact]
    public void B2BDomain_DoesNot_Own_Identity_Access_Or_Party_Organization_Data()
    {
        Assert.False(AgencyRelationshipBoundary.B2BOwnsIdentityCredentials);
        Assert.False(AgencyRelationshipBoundary.B2BOwnsAccessAuthorization);
        Assert.False(AgencyRelationshipBoundary.B2BOwnsPartyOrganizationData);
        Assert.Equal("Party", AgencyRelationshipBoundary.PartyIdentityOwner);
        Assert.Equal("Access", AgencyRelationshipBoundary.AccessAuthorizationOwner);
        Assert.Equal("Identity", AgencyRelationshipBoundary.IdentityCredentialOwner);
    }

    [Fact]
    public void B2B_T002_Forbids_Agency_Aggregate_Persistence_And_Commercial_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "B2B");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(AgencyUser|Commission|Contract|CreditLimit|Wallet|Settlement|BookingBase|GenericBookingAggregate|IBookingService|IPaymentService)\b",
            RegexOptions.Compiled);
        var forbiddenAgencyAggregate = new Regex(
            @"\b(class|record|enum|struct|interface)\s+Agency\b",
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

                    return forbiddenType.IsMatch(x.line) || forbiddenAgencyAggregate.IsMatch(x.line);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(hits.Count == 0, "B2B T002 forbids Agency aggregate/product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void B2B_T002_Has_No_Migration_Or_Product_Table_Additions()
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
        Assert.False(AgencyRelationshipBoundary.AgencyPersistenceImplemented);
    }

    [Fact]
    public void B2BDomain_Has_No_Booking_Or_Payment_ProjectReferences()
    {
        var domainCsproj = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "B2B",
            "TravelCore.Modules.B2B.Domain",
            "TravelCore.Modules.B2B.Domain.csproj"));
        Assert.DoesNotContain("Booking", domainCsproj, StringComparison.Ordinal);
        Assert.DoesNotContain("Payment", domainCsproj, StringComparison.Ordinal);
        Assert.DoesNotContain("Identity", domainCsproj, StringComparison.Ordinal);
        Assert.DoesNotContain("Access", domainCsproj, StringComparison.Ordinal);
        Assert.DoesNotContain("Party", domainCsproj, StringComparison.Ordinal);
    }

    [Fact]
    public void P24_Evidence_Records_T002_And_R2()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P24-implementation-plan.md"));
        Assert.Contains("TC-P24-T002", plan, StringComparison.Ordinal);
        Assert.Contains("P24-R2", plan, StringComparison.Ordinal);
        Assert.Contains("AgencyReference", plan, StringComparison.Ordinal);
        Assert.Contains("AgencyRelationshipBoundary", plan, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
