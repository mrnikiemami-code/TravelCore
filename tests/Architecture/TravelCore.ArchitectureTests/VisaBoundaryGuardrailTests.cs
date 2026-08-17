using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Visa.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P17-T003: VisaApplicability is structured facts, not a rules engine.
/// Visa != Destination · Visa != ReferenceData · Visa != Content · Visa != Pricing · Visa != Booking · Visa != SEO · Visa != Search.
/// </summary>
public sealed class VisaBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void VisaProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Visa.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Visa.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Visa.Infrastructure");
        Assert.Equal("visa", TravelCore.Modules.Visa.Infrastructure.VisaDbContext.SchemaName);
        Assert.Equal("visa", VisaOwnershipBoundary.SchemaName);
    }

    [Fact]
    public void Visa_DoesNot_Own_Peer_SoT_Or_Product_Types()
    {
        Assert.Equal("Visa", VisaOwnershipBoundary.OwnerModule);
        Assert.False(VisaOwnershipBoundary.OwnsDestinationFacts);
        Assert.False(VisaOwnershipBoundary.OwnsReferenceData);
        Assert.False(VisaOwnershipBoundary.OwnsContentCms);
        Assert.False(VisaOwnershipBoundary.OwnsMediaAssetTruth);
        Assert.False(VisaOwnershipBoundary.OwnsPricing);
        Assert.False(VisaOwnershipBoundary.OwnsQuote);
        Assert.False(VisaOwnershipBoundary.OwnsBooking);
        Assert.False(VisaOwnershipBoundary.OwnsPayment);
        Assert.False(VisaOwnershipBoundary.OwnsIndexPolicy);
        Assert.False(VisaOwnershipBoundary.OwnsSearch);
        Assert.False(VisaOwnershipBoundary.OwnsIdentityOrParty);
        Assert.True(VisaOwnershipBoundary.GeographicReferencesAreLogicalOnly);
        Assert.False(VisaOwnershipBoundary.GeographicReferencesAreSourceOfTruth);
        Assert.False(VisaOwnershipBoundary.RegulatoryEngineImplemented);
        Assert.True(VisaOwnershipBoundary.VisaDefinitionImplemented);
        Assert.True(VisaOwnershipBoundary.VisaRequirementSetImplemented);
        Assert.True(VisaOwnershipBoundary.VisaApplicabilityImplemented);
        Assert.False(VisaOwnershipBoundary.ApplicabilityIsRulesEngine);
        Assert.False(VisaOwnershipBoundary.VisaRequirementImplemented);
        Assert.True(VisaOwnershipBoundary.RequiredDocumentImplemented);
        Assert.True(VisaOwnershipBoundary.EligibilityModelImplemented);
        Assert.False(VisaOwnershipBoundary.EligibilityIsRulesEngine);
        Assert.True(VisaOwnershipBoundary.ProcessingValidityModelImplemented);
        Assert.False(VisaOwnershipBoundary.FeeModelImplemented);
        Assert.False(VisaOwnershipBoundary.ApplicationWorkflowImplemented);
    }

    [Fact]
    public void VisaInfrastructure_MustNotProjectReference_PeerBusinessModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.Visa.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Visa.Infrastructure must not project-reference peer business modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void VisaDomain_MustNotProjectReference_PeerBusinessModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.Visa.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)
                || IsForbiddenPeerModule(name))
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Visa.Domain must stay free of peer modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void VisaContracts_MustNotProjectReference_PeerBusinessModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.Visa.Contracts");
        var hits = contracts.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Visa.Contracts must not project-reference peer business modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Visa_T002_Separates_Definition_From_RequirementSet_Without_Later_R()
    {
        Assert.NotNull(typeof(TravelCore.Modules.Visa.Domain.VisaDefinition));
        Assert.NotNull(typeof(TravelCore.Modules.Visa.Domain.VisaRequirementSet));
        Assert.NotNull(typeof(TravelCore.Modules.Visa.Domain.VisaDefinitionTranslation));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaDefinition).GetProperty("Amount"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaDefinition).GetProperty("Currency"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaDefinition).GetProperty("Fee"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaDefinition).GetProperty("Price"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaRequirementSet).GetProperty("Amount"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaRequirementSet).GetProperty("Currency"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaRequirementSet).GetProperty("Fee"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaRequirementSet).GetProperty("Price"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaRequirementSet).GetProperty("DestinationId"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaRequirementSet).GetProperty("Nationality"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaRequirementSet).GetProperty("ApplicantNationality"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaRequirementSet).GetProperty("CountryOfResidence"));
        Assert.True(VisaOwnershipBoundary.VisaDefinitionImplemented);
        Assert.True(VisaOwnershipBoundary.VisaRequirementSetImplemented);
        Assert.True(VisaOwnershipBoundary.VisaApplicabilityImplemented);
        Assert.False(VisaOwnershipBoundary.ApplicabilityIsRulesEngine);
        Assert.True(VisaOwnershipBoundary.RequiredDocumentImplemented);
        Assert.True(VisaOwnershipBoundary.EligibilityModelImplemented);
        Assert.False(VisaOwnershipBoundary.EligibilityIsRulesEngine);
        Assert.True(VisaOwnershipBoundary.ProcessingValidityModelImplemented);
        Assert.False(VisaOwnershipBoundary.FeeModelImplemented);
        Assert.False(VisaOwnershipBoundary.ApplicationWorkflowImplemented);
    }

    [Fact]
    public void Visa_T002_MustNotImplement_Deferred_Product_Types()
    {
        var roots = new[]
        {
            Path.Combine(RepoRoot, "src", "backend", "Modules", "Visa"),
        };

        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(VisaRequirement|RequiredDocument|EligibilityRule|VisaFee|VisaApplication|VisaOffering|Country|Destination|Nationality|Region|RuleEngine|PolicyEngine|DecisionTable)\b",
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
            "T002 forbids document/fee/application and geo-clone product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Visa_T003_Applicability_Is_Structured_Facts_Not_Engine()
    {
        Assert.NotNull(typeof(TravelCore.Modules.Visa.Domain.VisaApplicability));
        Assert.NotNull(typeof(TravelCore.Modules.Visa.Domain.VisaApplicantCategory));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaApplicability).GetProperty("Expression"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaApplicability).GetProperty("Predicate"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaApplicability).GetProperty("Rules"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaApplicability).GetProperty("Amount"));
        Assert.True(VisaOwnershipBoundary.VisaApplicabilityImplemented);
        Assert.False(VisaOwnershipBoundary.ApplicabilityIsRulesEngine);
        Assert.False(VisaOwnershipBoundary.OwnsDestinationFacts);
        Assert.False(VisaOwnershipBoundary.OwnsReferenceData);
        Assert.False(VisaOwnershipBoundary.OwnsIdentityOrParty);
        Assert.True(VisaOwnershipBoundary.RequiredDocumentImplemented);
        Assert.True(VisaOwnershipBoundary.EligibilityModelImplemented);
        Assert.False(VisaOwnershipBoundary.EligibilityIsRulesEngine);
        Assert.False(VisaOwnershipBoundary.FeeModelImplemented);
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaApplicability).Assembly.GetType("TravelCore.Modules.Visa.Domain.Country"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaApplicability).Assembly.GetType("TravelCore.Modules.Visa.Domain.Destination"));
    }

    [Fact]
    public void Visa_T005_Separates_Processing_Validity_Stay_And_Entry()
    {
        Assert.NotNull(typeof(TravelCore.Modules.Visa.Domain.VisaProcessingTime));
        Assert.NotNull(typeof(TravelCore.Modules.Visa.Domain.VisaValidity));
        Assert.NotNull(typeof(TravelCore.Modules.Visa.Domain.VisaAllowedStay));
        Assert.NotNull(typeof(TravelCore.Modules.Visa.Domain.VisaEntryPolicy));
        Assert.NotEqual(
            typeof(TravelCore.Modules.Visa.Domain.VisaProcessingTime),
            typeof(TravelCore.Modules.Visa.Domain.VisaValidity));
        Assert.NotEqual(
            typeof(TravelCore.Modules.Visa.Domain.VisaValidity),
            typeof(TravelCore.Modules.Visa.Domain.VisaAllowedStay));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaRequirementSet).GetProperty("Duration"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaProcessingTime).GetProperty("Duration"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaProcessingTime).GetProperty("Amount"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaValidity).GetProperty("Fee"));
        Assert.True(VisaOwnershipBoundary.ProcessingValidityModelImplemented);
        Assert.False(VisaOwnershipBoundary.FeeModelImplemented);
        Assert.False(VisaOwnershipBoundary.ApplicationWorkflowImplemented);
        Assert.False(VisaOwnershipBoundary.RegulatoryEngineImplemented);
    }

    [Fact]
    public void Visa_Evidence_Keeps_Ascii_Invariants()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P17-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);
        var text = File.ReadAllText(plan);
        Assert.Contains("Visa != Tour", text, StringComparison.Ordinal);
        Assert.Contains("Visa != Destination", text, StringComparison.Ordinal);
        Assert.Contains("Visa != Content", text, StringComparison.Ordinal);
        Assert.Contains("Visa != Booking", text, StringComparison.Ordinal);
        Assert.Contains("Visa != Payment", text, StringComparison.Ordinal);
        Assert.Contains("Visa != SEO authority", text, StringComparison.Ordinal);
        Assert.Contains("Visa != Search authority", text, StringComparison.Ordinal);
        Assert.Contains("P17-R1", text, StringComparison.Ordinal);
        Assert.Contains("P17-R3", text, StringComparison.Ordinal);
        Assert.Contains("P17-R4", text, StringComparison.Ordinal);
        Assert.Contains("RequiredDocument != EligibilityRequirement", text, StringComparison.Ordinal);
        Assert.Contains("EligibilityRequirement != Rules Engine", text, StringComparison.Ordinal);
        Assert.Contains("ProcessingTime != VisaValidity", text, StringComparison.Ordinal);
        Assert.Contains("VisaValidity != AllowedStay", text, StringComparison.Ordinal);
        Assert.Contains("VisaDefinition != VisaRequirementSet", text, StringComparison.Ordinal);
        Assert.Contains("Applicability != Rules Engine", text, StringComparison.Ordinal);
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
        || name.Contains(".ReferenceData.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".ReferenceData", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Media.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Media", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Seo.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Seo", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Search.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Search", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Ugc.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Ugc", StringComparison.OrdinalIgnoreCase)
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
