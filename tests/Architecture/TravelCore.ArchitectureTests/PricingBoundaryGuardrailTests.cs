using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P12-T001 / P12-R1: Pricing is an independent module owning schema <c>pricing</c>.
/// May logically reference TourDeparture identity (Guid) later — must not own Tour/Booking/Payment types
/// or project-reference Booking/Payment (modules may not exist yet).
/// </summary>
public sealed class PricingBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void PricingProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Pricing.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Pricing.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Pricing.Infrastructure");

        Assert.Equal("pricing", TravelCore.Modules.Pricing.Infrastructure.PricingDbContext.SchemaName);
    }

    [Fact]
    public void PricingInfrastructure_MustNotProjectReference_TourBookingPayment()
    {
        var pricingInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Pricing.Infrastructure");
        var violations = pricingInfra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.StartsWith("TravelCore.Modules.Tour.", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("TravelCore.Modules.Booking.", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("TravelCore.Modules.Payment.", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            violations.Count == 0,
            "Pricing.Infrastructure must not project-reference Tour/Booking/Payment:\n"
            + string.Join('\n', violations));
    }

    [Fact]
    public void PricingDomain_MustNotProjectReference_PeerBusinessModules()
    {
        var pricingDomain = Projects.Single(p => p.Name == "TravelCore.Modules.Pricing.Domain");
        var forbidden = pricingDomain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("TravelCore.Modules.Tour.", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("TravelCore.Modules.Booking.", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("TravelCore.Modules.Payment.", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            forbidden.Count == 0,
            "Pricing.Domain must stay free of Tour/Booking/Payment and peer Infrastructure:\n"
            + string.Join('\n', forbidden));
    }

    [Fact]
    public void PricingModule_DoesNotOwn_TourProduct_TourDeparture_Booking_Payment_Types()
    {
        var pricingRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Pricing");
        Assert.True(Directory.Exists(pricingRoot), pricingRoot);

        // Comments may mention TourDeparture as a future logical Guid reference; forbid ownership types only.
        var hits = Directory.EnumerateFiles(pricingRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
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

                    return Regex.IsMatch(
                        x.line,
                        @"\b(class|record|enum|struct|interface)\s+(TourProduct|TourDeparture|Booking|Payment|PaymentIntent|Reservation|Checkout)\b")
                        || Regex.IsMatch(
                            x.line,
                            @"\b(IBookingService|IPaymentService|ICheckoutService|DbSet<\s*(TourProduct|TourDeparture|Booking|Payment))\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Pricing must not own TourProduct/TourDeparture/Booking/Payment types:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void PricingModule_Forbids_TourSchemaFk_And_SharedDbContext()
    {
        var pricingRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Pricing");
        var hits = Directory.EnumerateFiles(pricingRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"principalSchema:\s*""tour""|HasOne<.*Tour|TravelCore\.Modules\.Tour\.(Domain|Infrastructure)|TourDbContext|shared\s+DbContext",
                    RegexOptions.IgnoreCase)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Pricing must not introduce Tour schema FK/nav or share Tour DbContext:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void Booking_And_Payment_Modules_DoNotExist_Yet()
    {
        // Guardrail documents absence: Pricing must not invent project refs to missing commerce peers.
        var booking = Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking");
        var payment = Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment");
        Assert.False(Directory.Exists(booking), "Booking module must not exist in P12 scaffolding.");
        Assert.False(Directory.Exists(payment), "Payment module must not exist in P12 scaffolding.");
    }
}
