using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.B2B.Contracts;
using TravelCore.Modules.Payment.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P24-T001 / P24-R1: independent B2B module and schema b2b;
/// Identity/Access/Party ownership unchanged; Booking/Payment execution unchanged.
/// </summary>
public sealed class B2BBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void B2BProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.B2B.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.B2B.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.B2B.Infrastructure");
        Assert.Equal("b2b", TravelCore.Modules.B2B.Infrastructure.B2BDbContext.SchemaName);
        Assert.Equal("b2b", B2BOwnershipBoundary.SchemaName);
        Assert.Equal("B2B", B2BOwnershipBoundary.OwnerModule);
    }

    [Fact]
    public void B2BInfrastructure_MustNotProjectReference_ForbiddenPeerModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.B2B.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "B2B.Infrastructure peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void B2BDomain_MustNotProjectReference_ForbiddenPeerModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.B2B.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "B2B.Domain peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void B2BContracts_MustNotProjectReference_ForbiddenPeerModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.B2B.Contracts");
        var hits = contracts.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "B2B.Contracts peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Identity_Access_And_Party_DoNot_Depend_On_B2B()
    {
        foreach (var name in new[]
                 {
                     "TravelCore.Modules.Identity.Contracts",
                     "TravelCore.Modules.Identity.Domain",
                     "TravelCore.Modules.Identity.Infrastructure",
                     "TravelCore.Modules.Access.Contracts",
                     "TravelCore.Modules.Access.Domain",
                     "TravelCore.Modules.Access.Infrastructure",
                     "TravelCore.Modules.Party.Contracts",
                     "TravelCore.Modules.Party.Domain",
                     "TravelCore.Modules.Party.Infrastructure",
                 })
        {
            var project = Projects.Single(p => p.Name == name);
            var hits = project.ProjectReferences
                .Select(r => Path.GetFileNameWithoutExtension(r)!)
                .Where(r => r.StartsWith("TravelCore.Modules.B2B", StringComparison.Ordinal))
                .ToList();
            Assert.True(hits.Count == 0, $"{name} must not reference B2B:\n" + string.Join('\n', hits));
        }
    }

    [Fact]
    public void Booking_And_Payment_DoNot_Depend_On_B2B()
    {
        foreach (var name in new[]
                 {
                     "TravelCore.Modules.Booking.Contracts",
                     "TravelCore.Modules.Booking.Domain",
                     "TravelCore.Modules.Booking.Infrastructure",
                     "TravelCore.Modules.Payment.Contracts",
                     "TravelCore.Modules.Payment.Domain",
                     "TravelCore.Modules.Payment.Infrastructure",
                 })
        {
            var project = Projects.Single(p => p.Name == name);
            var hits = project.ProjectReferences
                .Select(r => Path.GetFileNameWithoutExtension(r)!)
                .Where(r => r.StartsWith("TravelCore.Modules.B2B", StringComparison.Ordinal))
                .ToList();
            Assert.True(hits.Count == 0, $"{name} must not reference B2B:\n" + string.Join('\n', hits));
        }
    }

    [Fact]
    public void B2BModule_DoesNotOwn_Forbidden_T001_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "B2B");
        Assert.True(Directory.Exists(root), root);

        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(Agency|AgencyUser|Commission|Contract|CreditLimit|Wallet|Settlement|BookingBase|GenericBookingAggregate|IBookingService|IPaymentService)\b",
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

        Assert.True(hits.Count == 0, "B2B T001 forbids early product entities:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void B2BModule_Forbids_PeerSchemaFk_And_SharedDbContext()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "B2B");
        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"principalSchema:\s*""(identity|access|party|booking|payment)""|HasOne<.*(Identity|Access|Party|Booking|Payment)|TravelCore\.Modules\.(Identity|Access|Party|Booking|Payment)\.(Domain|Infrastructure)|(Identity|Access|Party|Booking|Payment)DbContext|shared\s+DbContext",
                    RegexOptions.IgnoreCase)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "B2B must not introduce peer-schema FK/nav or share foreign DbContexts:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void Host_Registers_B2BModule_After_DynamicPackage_With_No_Endpoints()
    {
        var program = File.ReadAllText(Path.Combine(RepoRoot, "src", "backend", "TravelCore.Api", "Program.cs"));
        var dynamicPackageIndex = program.IndexOf("new DynamicPackageModule()", StringComparison.Ordinal);
        var b2bIndex = program.IndexOf("new B2BModule()", StringComparison.Ordinal);
        Assert.True(dynamicPackageIndex >= 0, "DynamicPackageModule must be registered.");
        Assert.True(b2bIndex > dynamicPackageIndex, "B2BModule must register after DynamicPackageModule.");

        var module = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "B2B",
            "TravelCore.Modules.B2B.Infrastructure",
            "B2BModule.cs"));
        Assert.Contains("AddDbContext<B2BDbContext>", module, StringComparison.Ordinal);
        Assert.DoesNotContain("MapGet", module, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPost", module, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/b2b", module, StringComparison.Ordinal);
    }

    [Fact]
    public void B2B_Does_Not_Add_Payment_Target_Or_Change_Party_Ownership()
    {
        var payment = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Contracts",
            "PaymentTargetKind.cs"));
        Assert.Contains("TourBooking", payment, StringComparison.Ordinal);
        Assert.Contains("HotelBooking", payment, StringComparison.Ordinal);
        Assert.Contains("FlightBooking", payment, StringComparison.Ordinal);
        Assert.DoesNotContain("B2B", payment, StringComparison.Ordinal);

        Assert.Equal("Party", B2BPartyIdentityBoundary.IdentitySourceModule);
        Assert.Equal("Access", B2BPartyIdentityBoundary.AccessSubjectModule);
        Assert.Equal("Identity", B2BPartyIdentityBoundary.CredentialModule);
        Assert.Equal("B2B", B2BPartyIdentityBoundary.CommercialLayerModule);
        Assert.False(B2BOwnershipBoundary.OwnsPartyIdentity);
        Assert.False(B2BOwnershipBoundary.PaymentTargetAdded);

        var names = Enum.GetNames<PaymentTargetKind>();
        Assert.Equal(3, names.Length);
    }

    [Fact]
    public void B2B_Evidence_Keeps_Ascii_Invariants()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P24-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);
        var text = File.ReadAllText(plan);
        Assert.Contains("P24-R1", text, StringComparison.Ordinal);
        Assert.Contains("schema `b2b`", text, StringComparison.Ordinal);
        Assert.Contains("B2B != Identity", text, StringComparison.Ordinal);
        Assert.Contains("B2B != Party", text, StringComparison.Ordinal);
        Assert.Contains("TC-P24-T001", text, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsForbiddenPeer(string name) =>
        name is "TravelCore.Modules.Identity.Infrastructure"
            or "TravelCore.Modules.Identity.Domain"
            or "TravelCore.Modules.Identity.Contracts"
            or "TravelCore.Modules.Access.Infrastructure"
            or "TravelCore.Modules.Access.Domain"
            or "TravelCore.Modules.Access.Contracts"
            or "TravelCore.Modules.Party.Infrastructure"
            or "TravelCore.Modules.Party.Domain"
            or "TravelCore.Modules.Party.Contracts"
            or "TravelCore.Modules.Booking.Infrastructure"
            or "TravelCore.Modules.Booking.Domain"
            or "TravelCore.Modules.Booking.Contracts"
            or "TravelCore.Modules.Payment.Infrastructure"
            or "TravelCore.Modules.Payment.Domain"
            or "TravelCore.Modules.Payment.Contracts";
}
