using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Ugc.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P16-T002: UGC owns Review + child dimension ratings. Rating is not an independent aggregate.
/// UGC != Content · UGC != Media · UGC != target domain owner · UGC != SEO · UGC != Search.
/// </summary>
public sealed class UgcBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void UgcProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Ugc.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Ugc.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Ugc.Infrastructure");
        Assert.Equal("ugc", TravelCore.Modules.Ugc.Infrastructure.UgcDbContext.SchemaName);
        Assert.Equal("ugc", UgcOwnershipBoundary.SchemaName);
    }

    [Fact]
    public void Ugc_DoesNot_Own_Peer_SoT_Or_Product_Types()
    {
        Assert.Equal("Ugc", UgcOwnershipBoundary.OwnerModule);
        Assert.False(UgcOwnershipBoundary.OwnsIdentityOrParty);
        Assert.False(UgcOwnershipBoundary.OwnsContentCms);
        Assert.False(UgcOwnershipBoundary.OwnsMediaAssetTruth);
        Assert.False(UgcOwnershipBoundary.OwnsTourFacts);
        Assert.False(UgcOwnershipBoundary.OwnsPlaceFacts);
        Assert.False(UgcOwnershipBoundary.OwnsDestinationFacts);
        Assert.False(UgcOwnershipBoundary.OwnsIndexPolicy);
        Assert.False(UgcOwnershipBoundary.OwnsSearch);
        Assert.False(UgcOwnershipBoundary.OwnsBooking);
        Assert.False(UgcOwnershipBoundary.OwnsPayment);
        Assert.True(UgcOwnershipBoundary.ReviewImplemented);
        Assert.False(UgcOwnershipBoundary.RatingImplemented);
        Assert.False(UgcOwnershipBoundary.RatingIsIndependentAggregate);
        Assert.True(UgcOwnershipBoundary.OverallRatingOwnedByReview);
        Assert.True(UgcOwnershipBoundary.DimensionRatingsAreReviewChildren);
        Assert.False(UgcOwnershipBoundary.TravelogueImplemented);
        Assert.False(UgcOwnershipBoundary.LikeImplemented);
        Assert.False(UgcOwnershipBoundary.TargetAttachmentModelCommitted);
    }

    [Fact]
    public void UgcInfrastructure_MustNotProjectReference_PeerBusinessModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.Ugc.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Ugc.Infrastructure must not project-reference peer business modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void UgcDomain_MustNotProjectReference_PeerBusinessModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.Ugc.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)
                || IsForbiddenPeerModule(name))
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Ugc.Domain must stay free of peer modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void UgcContracts_MustNotProjectReference_PeerBusinessModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.Ugc.Contracts");
        var hits = contracts.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Ugc.Contracts must not project-reference peer business modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Ugc_MustNotImplement_IndependentRating_Or_DeferredProductAggregates()
    {
        Assert.NotNull(typeof(TravelCore.Modules.Ugc.Domain.Review));
        Assert.NotNull(typeof(TravelCore.Modules.Ugc.Domain.ReviewDimensionRating));
        Assert.NotNull(typeof(TravelCore.Modules.Ugc.Domain.RatingValue));

        var roots = new[]
        {
            Path.Combine(RepoRoot, "src", "backend", "Modules", "Ugc"),
        };

        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(Rating|RatingDimension|Travelogue|UserPhoto|Comment|Like|Report)\b",
            RegexOptions.Compiled);

        var hits = new List<string>();
        foreach (var root in roots)
        {
            Assert.True(Directory.Exists(root), root);
            hits.AddRange(
                Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
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
                    .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}"));
        }

        Assert.True(
            hits.Count == 0,
            "T002 forbids independent Rating aggregate and Travelogue/Comment/Like/Report product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Ugc_Evidence_Keeps_Ascii_Invariants()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P16-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);
        var text = File.ReadAllText(plan);
        Assert.Contains("UGC != Content", text, StringComparison.Ordinal);
        Assert.Contains("UGC != Media", text, StringComparison.Ordinal);
        Assert.Contains("UGC != target domain owner", text, StringComparison.Ordinal);
        Assert.Contains("UGC != SEO", text, StringComparison.Ordinal);
        Assert.Contains("UGC != Search", text, StringComparison.Ordinal);
        Assert.Contains("P16-R1", text, StringComparison.Ordinal);
        Assert.Contains("P16-R2", text, StringComparison.Ordinal);
    }

    private static bool IsForbiddenPeerModule(string name) =>
        name.Contains(".Tour.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Tour", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Content.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Content", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Pricing.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Pricing", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".AgencyMarketplace.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".AgencyMarketplace", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Place.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Place", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Destination.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Destination", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Media.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Media", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Seo.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Seo", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Search.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Search", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Identity.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Identity", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Party.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Party", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Booking.", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Payment.", StringComparison.OrdinalIgnoreCase);

    private static bool IsGeneratedOrBin(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
