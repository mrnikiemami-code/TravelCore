using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.PublicExperience.Contracts;
using TravelCore.Modules.Visa.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P17-T009: Phase boundary evidence — Visa owns structured visa policy facts;
/// P17-R1…R8 RESOLVED; no Search engine, SEO ownership, application workflow, or GATE close.
/// </summary>
public sealed class VisaPhaseBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void P17_EvidencePack_Exists_And_DoesNotClose_Gate()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P17-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("P17-R1", text, StringComparison.Ordinal);
        Assert.Contains("P17-R8", text, StringComparison.Ordinal);
        Assert.Contains("Visa != Destination", text, StringComparison.Ordinal);
        Assert.Contains("Visa != Content", text, StringComparison.Ordinal);
        Assert.Contains("VisaDefinition != VisaRequirementSet", text, StringComparison.Ordinal);
        Assert.Contains("Applicability != Rules Engine", text, StringComparison.Ordinal);
        Assert.Contains("RequiredDocument != EligibilityRequirement", text, StringComparison.Ordinal);
        Assert.Contains("RequiredDocument != ApplicantSubmittedDocument", text, StringComparison.Ordinal);
        Assert.Contains("ProcessingTime != VisaValidity", text, StringComparison.Ordinal);
        Assert.Contains("VisaValidity != AllowedStay", text, StringComparison.Ordinal);
        Assert.Contains("OfficialVisaFee != CommercialPrice", text, StringComparison.Ordinal);
        Assert.Contains("OfficialVisaFee != PaymentAmount", text, StringComparison.Ordinal);
        Assert.Contains("Public Visa Page != Automatically SEO Indexed", text, StringComparison.Ordinal);
        Assert.Contains("Structured Visa Fact != Editorial Guidance", text, StringComparison.Ordinal);
        Assert.Contains("Visa != VisaApplication", text, StringComparison.Ordinal);
        Assert.Contains("VisaApplication != Booking", text, StringComparison.Ordinal);
        Assert.Contains("VisaApplication != Payment", text, StringComparison.Ordinal);
        Assert.Contains("Visa policy data != Applicant PII", text, StringComparison.Ordinal);
        Assert.Contains("TC-P17-GATE", text, StringComparison.Ordinal);
        Assert.DoesNotContain("TC-P17-GATE COMPLETE", text, StringComparison.Ordinal);
        Assert.Contains("no new product capability", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Visa_Keeps_All_P17_Boundaries_Resolved()
    {
        Assert.Equal("Visa", VisaOwnershipBoundary.OwnerModule);
        Assert.Equal("visa", VisaOwnershipBoundary.SchemaName);
        Assert.False(VisaOwnershipBoundary.OwnsDestinationFacts);
        Assert.False(VisaOwnershipBoundary.OwnsReferenceData);
        Assert.False(VisaOwnershipBoundary.OwnsContentCms);
        Assert.False(VisaOwnershipBoundary.OwnsPricing);
        Assert.False(VisaOwnershipBoundary.OwnsBooking);
        Assert.False(VisaOwnershipBoundary.OwnsPayment);
        Assert.False(VisaOwnershipBoundary.OwnsIndexPolicy);
        Assert.False(VisaOwnershipBoundary.OwnsSearch);
        Assert.True(VisaOwnershipBoundary.VisaDefinitionImplemented);
        Assert.True(VisaOwnershipBoundary.VisaRequirementSetImplemented);
        Assert.True(VisaOwnershipBoundary.VisaApplicabilityImplemented);
        Assert.False(VisaOwnershipBoundary.ApplicabilityIsRulesEngine);
        Assert.True(VisaOwnershipBoundary.RequiredDocumentImplemented);
        Assert.True(VisaOwnershipBoundary.EligibilityModelImplemented);
        Assert.False(VisaOwnershipBoundary.EligibilityIsRulesEngine);
        Assert.True(VisaOwnershipBoundary.ProcessingValidityModelImplemented);
        Assert.True(VisaOwnershipBoundary.FeeModelImplemented);
        Assert.True(VisaOwnershipBoundary.PublicReadImplemented);
        Assert.False(VisaOwnershipBoundary.PublicPresenceEqualsSeoIndexed);
        Assert.True(VisaOwnershipBoundary.VisaPolicyCapabilityCompleteInP17);
        Assert.False(VisaOwnershipBoundary.VisaApplicationCapabilityImplemented);
        Assert.False(VisaOwnershipBoundary.ApplicationWorkflowImplemented);
        Assert.True(VisaOwnershipBoundary.RequiredDocumentIsRequirementDefinitionOnly);
        Assert.False(VisaOwnershipBoundary.ApplicantSubmittedDocumentImplemented);
        Assert.False(VisaApplicationBoundary.VisaApplicationImplemented);
        Assert.False(VisaApplicationBoundary.VisaEqualsVisaApplication);
        Assert.Equal("Visa", VisaPublicCompositionBoundary.FactOwner);
        Assert.Equal("PublicExperience", VisaPublicCompositionBoundary.PresentationOwner);
        Assert.Equal("Seo", VisaPublicCompositionBoundary.IndexPolicyOwner);
        Assert.False(VisaPublicCompositionBoundary.ApplicationWorkflowAllowed);
        Assert.False(VisaPublicCompositionBoundary.DocumentUploadAllowed);
        Assert.Equal("Visa", PublicExperienceVisaCompositionBoundary.FactOwner);
        Assert.False(PublicExperienceVisaCompositionBoundary.ApplicationWorkflowAllowed);
        Assert.False(PublicExperienceVisaCompositionBoundary.BookingCtaAllowed);
        Assert.False(PublicExperienceVisaCompositionBoundary.DocumentUploadAllowed);
    }

    [Fact]
    public void Visa_Module_Keeps_Search_Seo_Application_And_Ai_Engines_Out()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Visa");
        Assert.True(Directory.Exists(root), root);

        var forbidden = new[]
        {
            "Elasticsearch",
            "OpenSearch",
            "pg_trgm",
            "to_tsvector",
            "SetIndexPolicy",
            "class VisaApplication",
            "record VisaApplication",
            "visa_applications",
            "ApplicantSubmittedDocument",
            "UploadedDocument",
            "Ocr",
            "Appointment",
            "embeddings",
            "vector search",
            "RAG",
            "RuleEngine",
            "PolicyEngine",
        };

        var hits = new List<string>();
        foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            if (path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith("Boundary.cs", StringComparison.OrdinalIgnoreCase))
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

        Assert.True(hits.Count == 0, "Visa must not introduce Search/SEO/Application/AI engines:\n" + string.Join('\n', hits));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaDefinition).Assembly.GetType("TravelCore.Modules.Visa.Domain.VisaApplication"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaDefinition).Assembly.GetType("TravelCore.Modules.Visa.Domain.Country"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaDefinition).Assembly.GetType("TravelCore.Modules.Visa.Domain.Destination"));
        Assert.True(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking")));
        Assert.True(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment")));
    }

    [Fact]
    public void PublicExperience_Does_Not_Own_Visa_Persistence()
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
        Assert.DoesNotContain("TravelCore.Modules.Visa.Infrastructure", csproj, StringComparison.Ordinal);

        var loader = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "visa-detail",
            "load-visa-detail.ts"));
        Assert.Contains("/api/visa/public", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("VisaDbContext", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIndexPolicy", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/search", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("Apply Now", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("Upload Documents", loader, StringComparison.Ordinal);
    }

    [Fact]
    public void P17_GateEvidence_Exists_And_Closes_Phase()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P17-GATE-acceptance-evidence.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("TC-P17-T001", text, StringComparison.Ordinal);
        Assert.Contains("TC-P17-T009", text, StringComparison.Ordinal);
        Assert.Contains("P17-R1", text, StringComparison.Ordinal);
        Assert.Contains("P17-R8", text, StringComparison.Ordinal);
        Assert.Contains("Visa != Content", text, StringComparison.Ordinal);
        Assert.Contains("VisaDefinition != VisaRequirementSet", text, StringComparison.Ordinal);
        Assert.Contains("RequiredDocument != ApplicantSubmittedDocument", text, StringComparison.Ordinal);
        Assert.Contains("Visa != VisaApplication", text, StringComparison.Ordinal);
        Assert.Contains("Public Visa Page != Automatically SEO Indexed", text, StringComparison.Ordinal);
        Assert.Contains("TC-P17-GATE COMPLETE", text, StringComparison.Ordinal);
        Assert.Contains("P17 COMPLETE", text, StringComparison.Ordinal);
        Assert.Contains("no new Visa capability", text, StringComparison.OrdinalIgnoreCase);
    }
}
