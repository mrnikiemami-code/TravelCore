using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.PublicExperience.Contracts;
using TravelCore.Modules.Ugc.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P16-T009: Phase boundary evidence — UGC owns user-generated facts/eligibility;
/// P16-R1…R8 RESOLVED; no Search engine, SEO ownership, Like, or GATE close.
/// </summary>
public sealed class UgcPhaseBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void P16_EvidencePack_Exists_And_DoesNotClose_Gate()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P16-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("P16-R1", text, StringComparison.Ordinal);
        Assert.Contains("P16-R8", text, StringComparison.Ordinal);
        Assert.Contains("UGC != Content", text, StringComparison.Ordinal);
        Assert.Contains("Travelogue != ContentItem", text, StringComparison.Ordinal);
        Assert.Contains("UserPhoto != MediaAsset", text, StringComparison.Ordinal);
        Assert.Contains("Rating != Independent Aggregate", text, StringComparison.Ordinal);
        Assert.Contains("Comment != Threaded Conversation System", text, StringComparison.Ordinal);
        Assert.Contains("Like = DEFERRED", text, StringComparison.Ordinal);
        Assert.Contains("ModerationStatus != PublicationStatus", text, StringComparison.Ordinal);
        Assert.Contains("Approved != Published", text, StringComparison.Ordinal);
        Assert.Contains("PublicEligibility = Approved + Published", text, StringComparison.Ordinal);
        Assert.Contains("Published != SEO Indexed", text, StringComparison.Ordinal);
        Assert.Contains("Publicly Eligible != Automatically Search Indexed", text, StringComparison.Ordinal);
        Assert.Contains("Report != Automatic Enforcement", text, StringComparison.Ordinal);
        Assert.Contains("PublicExperience != UGC Source of Truth", text, StringComparison.Ordinal);
        Assert.Contains("Search != UGC Source of Truth", text, StringComparison.Ordinal);
        Assert.Contains("UGC != SEO Authority", text, StringComparison.Ordinal);
        Assert.Contains("UGC != Search Ranking Authority", text, StringComparison.Ordinal);
        Assert.Contains("TC-P16-GATE", text, StringComparison.Ordinal);
        Assert.DoesNotContain("TC-P16-GATE COMPLETE", text, StringComparison.Ordinal);
        Assert.Contains("no new product capability", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ugc_Keeps_All_P16_Boundaries_Resolved()
    {
        Assert.Equal("Ugc", UgcOwnershipBoundary.OwnerModule);
        Assert.Equal("ugc", UgcOwnershipBoundary.SchemaName);
        Assert.False(UgcOwnershipBoundary.OwnsContentCms);
        Assert.False(UgcOwnershipBoundary.OwnsMediaAssetTruth);
        Assert.False(UgcOwnershipBoundary.OwnsIndexPolicy);
        Assert.False(UgcOwnershipBoundary.OwnsSearch);
        Assert.False(UgcOwnershipBoundary.RatingIsIndependentAggregate);
        Assert.True(UgcOwnershipBoundary.TravelogueIsNotContentItem);
        Assert.True(UgcOwnershipBoundary.UserPhotoIsNotMediaAsset);
        Assert.True(UgcOwnershipBoundary.LikeDeferred);
        Assert.False(UgcOwnershipBoundary.LikeImplemented);
        Assert.False(UgcOwnershipBoundary.ApprovedEqualsPublished);
        Assert.False(UgcOwnershipBoundary.PublishedEqualsSeoIndexed);
        Assert.False(UgcOwnershipBoundary.ReportTriggersAutomaticEnforcement);
        Assert.True(UgcOwnershipBoundary.PublicReadContractsImplemented);
        Assert.True(UgcOwnershipBoundary.RatingSummaryIsDerivedRebuildable);
        Assert.False(UgcOwnershipBoundary.SearchEngineInUgcAllowed);
        Assert.False(UgcOwnershipBoundary.UgcOwnedSeoPagesAllowed);
        Assert.Equal("Ugc", UgcPublicCompositionBoundary.FactOwner);
        Assert.Equal("PublicExperience", UgcPublicCompositionBoundary.PresentationOwner);
        Assert.Equal("Search", UgcPublicCompositionBoundary.SearchOwner);
        Assert.Equal("Seo", UgcPublicCompositionBoundary.IndexPolicyOwner);
        Assert.False(UgcPublicCompositionBoundary.PubliclyEligibleEqualsSeoIndexed);
        Assert.False(UgcPublicCompositionBoundary.PubliclyEligibleEqualsAutomaticallySearchIndexed);
        Assert.False(UgcPublicCompositionBoundary.IndependentAverageRatingEngineAllowed);
        Assert.Equal("Ugc", PublicExperienceUgcCompositionBoundary.FactOwner);
        Assert.False(PublicExperienceUgcCompositionBoundary.CopyUgcIntoCatalogAllowed);
        Assert.False(PublicExperienceUgcCompositionBoundary.SearchEngineAllowed);
        Assert.False(PublicExperienceUgcCompositionBoundary.UgcSeoPagesAllowed);
        Assert.False(PublicExperienceUgcCompositionBoundary.RankingFromUgcAllowed);
    }

    [Fact]
    public void Ugc_Module_Keeps_Search_Seo_Ai_And_Like_Engines_Out()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Ugc");
        Assert.True(Directory.Exists(root), root);

        var forbidden = new[]
        {
            "Elasticsearch",
            "OpenSearch",
            "pg_trgm",
            "to_tsvector",
            "SetIndexPolicy",
            "ParentCommentId",
            "LikeCount",
            "ReactionCount",
            "embeddings",
            "vector search",
            "RAG",
        };

        var hits = new List<string>();
        foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            if (path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var text = File.ReadAllText(path);
            foreach (var token in forbidden)
            {
                if (text.Contains(token, StringComparison.Ordinal))
                {
                    hits.Add($"{Path.GetRelativePath(RepoRoot, path)}:{token}");
                }
            }
        }

        Assert.True(hits.Count == 0, "UGC must not introduce Search/SEO/AI/Like engines:\n" + string.Join('\n', hits));
        Assert.DoesNotContain("Report", typeof(EligiblePublicReview).GetProperties().Select(p => p.Name));
        Assert.DoesNotContain("ModerationStatus", typeof(EligiblePublicReview).GetProperties().Select(p => p.Name));
        Assert.DoesNotContain("PublicationStatus", typeof(EligiblePublicReview).GetProperties().Select(p => p.Name));
        Assert.Null(typeof(TravelCore.Modules.Ugc.Domain.Comment).GetProperty("ParentCommentId"));
        Assert.Null(typeof(TravelCore.Modules.Ugc.Domain.Travelogue).GetProperty("ContentItemId"));
        Assert.Null(typeof(TravelCore.Modules.Ugc.Domain.UserPhoto).GetProperty("StorageKey"));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment")));
    }

    [Fact]
    public void PublicExperience_Does_Not_Own_Ugc_Persistence()
    {
        var peContracts = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "PublicExperience",
            "TravelCore.Modules.PublicExperience.Contracts",
            "TravelCore.Modules.PublicExperience.Contracts.csproj");
        Assert.True(File.Exists(peContracts), peContracts);
        var csproj = File.ReadAllText(peContracts);
        Assert.DoesNotContain("TravelCore.Modules.Ugc", csproj, StringComparison.Ordinal);

        var loader = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "public-experience",
            "load-ugc-composition.ts"));
        Assert.Contains("/api/ugc/public", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("UgcDbContext", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("schema ugc", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIndexPolicy", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/search", loader, StringComparison.Ordinal);
    }

    [Fact]
    public void P16_GateEvidence_Exists_And_Closes_Phase()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P16-GATE-acceptance-evidence.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("TC-P16-T001", text, StringComparison.Ordinal);
        Assert.Contains("TC-P16-T009", text, StringComparison.Ordinal);
        Assert.Contains("P16-R1", text, StringComparison.Ordinal);
        Assert.Contains("P16-R8", text, StringComparison.Ordinal);
        Assert.Contains("UGC != Content", text, StringComparison.Ordinal);
        Assert.Contains("Travelogue != ContentItem", text, StringComparison.Ordinal);
        Assert.Contains("UserPhoto != MediaAsset", text, StringComparison.Ordinal);
        Assert.Contains("Like = DEFERRED", text, StringComparison.Ordinal);
        Assert.Contains("PublicEligibility = Approved + Published", text, StringComparison.Ordinal);
        Assert.Contains("Publicly Eligible != Automatically Search Indexed", text, StringComparison.Ordinal);
        Assert.Contains("TC-P16-GATE COMPLETE", text, StringComparison.Ordinal);
        Assert.Contains("P16 COMPLETE", text, StringComparison.Ordinal);
        Assert.Contains("no new UGC capability", text, StringComparison.OrdinalIgnoreCase);
    }
}
