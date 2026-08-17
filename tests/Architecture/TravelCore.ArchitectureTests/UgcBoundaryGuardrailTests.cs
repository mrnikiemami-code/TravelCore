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
        Assert.True(UgcOwnershipBoundary.TravelogueImplemented);
        Assert.True(UgcOwnershipBoundary.TravelogueIsNotContentItem);
        Assert.True(UgcOwnershipBoundary.UserPhotoImplemented);
        Assert.True(UgcOwnershipBoundary.UserPhotoIsNotMediaAsset);
        Assert.True(UgcOwnershipBoundary.CommentImplemented);
        Assert.False(UgcOwnershipBoundary.LikeImplemented);
        Assert.True(UgcOwnershipBoundary.LikeDeferred);
        Assert.True(UgcOwnershipBoundary.ReportImplemented);
        Assert.True(UgcOwnershipBoundary.ModerationWorkflowImplemented);
        Assert.False(UgcOwnershipBoundary.ApprovedEqualsPublished);
        Assert.False(UgcOwnershipBoundary.PublishedEqualsSeoIndexed);
        Assert.True(UgcOwnershipBoundary.TargetAttachmentModelCommitted);
        Assert.True(UgcOwnershipBoundary.ReviewTargetIsLogicalReferenceOnly);
        Assert.False(UgcOwnershipBoundary.OwnsTargetFacts);
        Assert.True(UgcOwnershipBoundary.PublicReadContractsImplemented);
        Assert.False(UgcOwnershipBoundary.PubliclyEligibleEqualsSeoIndexed);
        Assert.False(UgcOwnershipBoundary.PubliclyEligibleEqualsAutomaticallySearchIndexed);
        Assert.False(UgcOwnershipBoundary.IndependentAverageRatingEngineAllowed);
        Assert.True(UgcOwnershipBoundary.RatingSummaryIsDerivedRebuildable);
        Assert.False(UgcOwnershipBoundary.SearchEngineInUgcAllowed);
        Assert.False(UgcOwnershipBoundary.UgcOwnedSeoPagesAllowed);
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

        Assert.NotNull(typeof(TravelCore.Modules.Ugc.Domain.Travelogue));
        Assert.NotNull(typeof(TravelCore.Modules.Ugc.Domain.UserPhoto));
        Assert.NotNull(typeof(TravelCore.Modules.Ugc.Domain.Comment));
        Assert.NotNull(typeof(TravelCore.Modules.Ugc.Domain.UgcReport));
        Assert.NotNull(typeof(TravelCore.Modules.Ugc.Domain.ModerationStatus));
        Assert.NotNull(typeof(TravelCore.Modules.Ugc.Domain.PublicationStatus));

        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(Rating|RatingDimension|Like|Report)\b",
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
            "T007 forbids independent Rating aggregate and Like product types:\n" + string.Join('\n', hits));
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
        Assert.Contains("P16-R3", text, StringComparison.Ordinal);
        Assert.Contains("P16-R4", text, StringComparison.Ordinal);
        Assert.Contains("P16-R5", text, StringComparison.Ordinal);
        Assert.Contains("P16-R7", text, StringComparison.Ordinal);
        Assert.Contains("P16-R8", text, StringComparison.Ordinal);
        Assert.Contains("Approved != Published", text, StringComparison.Ordinal);
        Assert.Contains("Published != SEO Indexed", text, StringComparison.Ordinal);
        Assert.Contains("Publicly Eligible != SEO Indexed", text, StringComparison.Ordinal);
        Assert.Contains("Publicly Eligible != Automatically Search Indexed", text, StringComparison.Ordinal);
        Assert.Contains("Travelogue != ContentItem", text, StringComparison.Ordinal);
        Assert.Contains("UserPhoto relationship != MediaAsset", text, StringComparison.Ordinal);
        Assert.Contains("Like = DEFERRED", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Content_MustNot_Absorb_Travelogue_As_ContentItem_Flag()
    {
        var contentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Content");
        Assert.True(Directory.Exists(contentRoot), contentRoot);
        var forbidden = new Regex(@"\b(IsUserGenerated|UgcType)\b", RegexOptions.Compiled);
        var hits = Directory.EnumerateFiles(contentRoot, "*.cs", SearchOption.AllDirectories)
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

                    return forbidden.IsMatch(x.line);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Content must not absorb Travelogue via IsUserGenerated/UgcType:\n" + string.Join('\n', hits));
        Assert.Null(typeof(TravelCore.Modules.Ugc.Domain.Travelogue).GetProperty("ContentItemId"));
        Assert.NotNull(typeof(TravelCore.Modules.Ugc.Domain.Travelogue).GetProperty("PublicationStatus"));
        Assert.NotNull(typeof(TravelCore.Modules.Ugc.Domain.Travelogue).GetProperty("ModerationStatus"));
    }

    [Fact]
    public void Ugc_UserPhoto_MustNot_Persist_MediaTechnicalFacts()
    {
        Assert.Null(typeof(TravelCore.Modules.Ugc.Domain.UserPhoto).GetProperty("StorageKey"));
        Assert.Null(typeof(TravelCore.Modules.Ugc.Domain.UserPhoto).GetProperty("MimeType"));
        Assert.Null(typeof(TravelCore.Modules.Ugc.Domain.UserPhoto).GetProperty("FileSize"));
        Assert.Null(typeof(TravelCore.Modules.Ugc.Domain.UserPhoto).GetProperty("Width"));
        Assert.Null(typeof(TravelCore.Modules.Ugc.Domain.UserPhoto).GetProperty("Height"));
        Assert.Null(typeof(TravelCore.Modules.Ugc.Domain.UserPhoto).GetProperty("FocalPoint"));
        Assert.Null(typeof(TravelCore.Modules.Ugc.Domain.UserPhoto).GetProperty("Renditions"));
        Assert.NotNull(typeof(TravelCore.Modules.Ugc.Domain.UserPhoto).GetProperty("MediaAssetId"));
        Assert.Null(typeof(TravelCore.Modules.Ugc.Domain.Comment).GetProperty("ParentCommentId"));
        Assert.Null(typeof(TravelCore.Modules.Ugc.Domain.Comment).GetProperty("LikeCount"));
    }

    [Fact]
    public void Ugc_Public_Read_Is_Composition_Not_Search_Or_Seo()
    {
        Assert.NotNull(typeof(IUgcPublicReviewQuery));
        Assert.NotNull(typeof(IUgcPublicTravelogueQuery));
        Assert.NotNull(typeof(IUgcPublicUserPhotoQuery));
        Assert.NotNull(typeof(IUgcPublicCommentQuery));
        Assert.Equal("Ugc", UgcPublicCompositionBoundary.FactOwner);
        Assert.Equal("PublicExperience", UgcPublicCompositionBoundary.PresentationOwner);
        Assert.Equal("Search", UgcPublicCompositionBoundary.SearchOwner);
        Assert.Equal("Seo", UgcPublicCompositionBoundary.IndexPolicyOwner);
        Assert.False(UgcPublicCompositionBoundary.PubliclyEligibleEqualsSeoIndexed);
        Assert.False(UgcPublicCompositionBoundary.PubliclyEligibleEqualsAutomaticallySearchIndexed);
        Assert.False(UgcPublicCompositionBoundary.IndependentAverageRatingEngineAllowed);
        Assert.False(UgcPublicCompositionBoundary.SearchEngineInThisTaskAllowed);
        Assert.False(UgcPublicCompositionBoundary.UgcOwnedSeoPagesAllowed);

        var endpoints = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Ugc",
            "TravelCore.Modules.Ugc.Infrastructure",
            "Endpoints",
            "UgcPublicEndpoints.cs"));
        Assert.Contains("/api/ugc/public", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("RequireAuthorization", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIndexPolicy", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("Elasticsearch", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("to_tsvector", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/search", endpoints, StringComparison.Ordinal);

        var query = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Ugc",
            "TravelCore.Modules.Ugc.Infrastructure",
            "Services",
            "UgcPublicQuery.cs"));
        Assert.Contains("UgcPublicEligibility.IsPubliclyEligible", query, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIndexPolicy", query, StringComparison.Ordinal);
        Assert.DoesNotContain("Elasticsearch", query, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", query, StringComparison.Ordinal);
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
