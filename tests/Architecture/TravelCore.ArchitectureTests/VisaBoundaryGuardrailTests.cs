using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.PublicExperience.Contracts;
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
        Assert.True(VisaOwnershipBoundary.FeeModelImplemented);
        Assert.True(VisaOwnershipBoundary.PublicReadImplemented);
        Assert.False(VisaOwnershipBoundary.PublicPresenceEqualsSeoIndexed);
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
        Assert.True(VisaOwnershipBoundary.FeeModelImplemented);
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
        Assert.True(VisaOwnershipBoundary.FeeModelImplemented);
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
        Assert.True(VisaOwnershipBoundary.FeeModelImplemented);
        Assert.False(VisaOwnershipBoundary.ApplicationWorkflowImplemented);
        Assert.False(VisaOwnershipBoundary.RegulatoryEngineImplemented);
    }

    [Fact]
    public void Visa_T006_OfficialFee_Is_Not_Pricing_Quote_Or_Fx()
    {
        Assert.NotNull(typeof(TravelCore.Modules.Visa.Domain.VisaOfficialFee));
        Assert.NotNull(typeof(TravelCore.Modules.Visa.Domain.VisaOfficialFeeKind));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaOfficialFee).GetProperty("Quote"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaOfficialFee).GetProperty("Discount"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaOfficialFee).GetProperty("Commission"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaOfficialFee).GetProperty("Markup"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaOfficialFee).GetProperty("ExchangeRate"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaRequirementSet).GetProperty("TotalPrice"));
        Assert.True(VisaOwnershipBoundary.FeeModelImplemented);
        Assert.False(VisaOwnershipBoundary.OwnsPricing);
        Assert.False(VisaOwnershipBoundary.OwnsQuote);
        Assert.False(VisaOwnershipBoundary.OwnsPayment);
        Assert.False(VisaOwnershipBoundary.ApplicationWorkflowImplemented);
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaOfficialFee).Assembly.GetType("TravelCore.Modules.Visa.Domain.Price"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaOfficialFee).Assembly.GetType("TravelCore.Modules.Visa.Domain.Quote"));
    }

    [Fact]
    public void Visa_T007_Public_Read_Does_Not_Steal_Content_Seo_Search_Or_Application()
    {
        Assert.True(VisaOwnershipBoundary.PublicReadImplemented);
        Assert.False(VisaOwnershipBoundary.PublicPresenceEqualsSeoIndexed);
        Assert.False(VisaOwnershipBoundary.OwnsContentCms);
        Assert.False(VisaOwnershipBoundary.OwnsIndexPolicy);
        Assert.False(VisaOwnershipBoundary.OwnsSearch);
        Assert.False(VisaOwnershipBoundary.ApplicationWorkflowImplemented);
        Assert.Equal("Visa", VisaPublicCompositionBoundary.FactOwner);
        Assert.Equal("PublicExperience", VisaPublicCompositionBoundary.PresentationOwner);
        Assert.Equal("Content", VisaPublicCompositionBoundary.EditorialOwner);
        Assert.Equal("Seo", VisaPublicCompositionBoundary.IndexPolicyOwner);
        Assert.Equal("Search", VisaPublicCompositionBoundary.SearchOwner);
        Assert.False(VisaPublicCompositionBoundary.PublicPresenceEqualsSeoIndexed);
        Assert.False(VisaPublicCompositionBoundary.CopyContentIntoVisaAllowed);
        Assert.False(VisaPublicCompositionBoundary.ApplicationWorkflowAllowed);
        Assert.False(VisaPublicCompositionBoundary.CommercialPriceDisplayAllowed);
        Assert.Equal("Visa", PublicExperienceVisaCompositionBoundary.FactOwner);
        Assert.Equal("PublicExperience", PublicExperienceVisaCompositionBoundary.PresentationOwner);
        Assert.Equal("Seo", PublicExperienceVisaCompositionBoundary.IndexPolicyOwner);
        Assert.False(PublicExperienceVisaCompositionBoundary.PublicPresenceEqualsSeoIndexed);
        Assert.False(PublicExperienceVisaCompositionBoundary.ApplicationWorkflowAllowed);
        Assert.False(PublicExperienceVisaCompositionBoundary.BookingCtaAllowed);
        Assert.NotNull(typeof(IVisaPublicQuery));
        Assert.NotNull(typeof(PublicVisaDefinition));
        Assert.Null(typeof(PublicVisaRequirementSet).GetProperty("Duration"));
        Assert.Null(typeof(PublicVisaRequirementSet).GetProperty("Price"));
        Assert.Null(typeof(PublicVisaDefinition).GetProperty("IndexPolicy"));

        var endpoints = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Visa",
            "TravelCore.Modules.Visa.Infrastructure",
            "Endpoints",
            "VisaPublicEndpoints.cs"));
        Assert.Contains("/api/visa/public", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("RequireAuthorization", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIndexPolicy", endpoints, StringComparison.Ordinal);

        var frontendRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "visa-detail");
        Assert.True(Directory.Exists(frontendRoot), frontendRoot);
        foreach (var path in Directory.EnumerateFiles(frontendRoot, "*.ts", SearchOption.AllDirectories)
                     .Concat(Directory.EnumerateFiles(frontendRoot, "*.tsx", SearchOption.AllDirectories)))
        {
            var text = File.ReadAllText(path);
            Assert.DoesNotContain("Apply Now", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Book Now", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Pay Now", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Submit Application", text, StringComparison.Ordinal);
            Assert.DoesNotContain("/api/search", text, StringComparison.Ordinal);
            Assert.DoesNotContain("SetIndexPolicy", text, StringComparison.Ordinal);
        }

        var page = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "visas", "[code]", "page.tsx");
        Assert.True(File.Exists(page), page);
        var pageText = File.ReadAllText(page);
        Assert.Contains("loadComposedSeoMetadata", pageText, StringComparison.Ordinal);
        Assert.DoesNotContain("Apply Now", pageText, StringComparison.Ordinal);
        Assert.DoesNotContain("index: true", pageText, StringComparison.Ordinal);
    }

    [Fact]
    public void Visa_T008_Locks_Application_Boundary_Without_Product_Implementation()
    {
        Assert.True(VisaApplicationBoundary.VisaPolicyCompleteInP17);
        Assert.False(VisaApplicationBoundary.VisaApplicationImplemented);
        Assert.True(VisaApplicationBoundary.DeferredToFutureCapability);
        Assert.False(VisaApplicationBoundary.VisaEqualsVisaApplication);
        Assert.False(VisaApplicationBoundary.VisaApplicationEqualsBooking);
        Assert.False(VisaApplicationBoundary.VisaApplicationEqualsPayment);
        Assert.False(VisaApplicationBoundary.RequiredDocumentEqualsApplicantSubmittedDocument);
        Assert.False(VisaApplicationBoundary.OfficialVisaFeeEqualsPaymentAmount);
        Assert.False(VisaApplicationBoundary.VisaPolicyDataContainsApplicantPii);
        Assert.False(VisaApplicationBoundary.PublicVisaApiExposesPrivateCaseData);
        Assert.False(VisaApplicationBoundary.PrivateApplicationApiImplemented);
        Assert.False(VisaApplicationBoundary.DocumentUploadAllowed);
        Assert.False(VisaApplicationBoundary.OcrAllowed);
        Assert.False(VisaApplicationBoundary.AppointmentSchedulingAllowed);
        Assert.False(VisaApplicationBoundary.ExternalEmbassyIntegrationAllowed);
        Assert.False(VisaApplicationBoundary.CaseLifecycleStateMachineAllowed);
        Assert.False(VisaApplicationBoundary.P17VisaIsGenericWorkflowEngine);
        Assert.True(VisaOwnershipBoundary.VisaPolicyCapabilityCompleteInP17);
        Assert.False(VisaOwnershipBoundary.VisaApplicationCapabilityImplemented);
        Assert.False(VisaOwnershipBoundary.OwnsApplicantCase);
        Assert.False(VisaOwnershipBoundary.OwnsApplicantPii);
        Assert.True(VisaOwnershipBoundary.RequiredDocumentIsRequirementDefinitionOnly);
        Assert.False(VisaOwnershipBoundary.ApplicantSubmittedDocumentImplemented);
        Assert.False(VisaPublicCompositionBoundary.DocumentUploadAllowed);
        Assert.False(VisaPublicCompositionBoundary.AppointmentBookingAllowed);
        Assert.False(VisaPublicCompositionBoundary.PaymentCtaAllowed);
        Assert.False(PublicExperienceVisaCompositionBoundary.DocumentUploadAllowed);
        Assert.False(PublicExperienceVisaCompositionBoundary.AppointmentBookingAllowed);
        Assert.False(PublicExperienceVisaCompositionBoundary.PaymentCtaAllowed);

        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaDefinition).Assembly.GetType("TravelCore.Modules.Visa.Domain.VisaApplication"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaDefinition).Assembly.GetType("TravelCore.Modules.Visa.Domain.Applicant"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaDefinition).Assembly.GetType("TravelCore.Modules.Visa.Domain.ApplicantSubmittedDocument"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaDefinition).Assembly.GetType("TravelCore.Modules.Visa.Domain.UploadedDocument"));
        Assert.Null(typeof(TravelCore.Modules.Visa.Domain.VisaDefinition).Assembly.GetType("TravelCore.Modules.Visa.Domain.VisaAppointment"));

        var snapshot = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Visa",
            "TravelCore.Modules.Visa.Infrastructure",
            "Migrations",
            "VisaDbContextModelSnapshot.cs"));
        Assert.DoesNotContain("visa_applications", snapshot, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("application_documents", snapshot, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("appointments", snapshot, StringComparison.OrdinalIgnoreCase);

        var endpoints = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Visa",
            "TravelCore.Modules.Visa.Infrastructure",
            "Endpoints",
            "VisaPublicEndpoints.cs"));
        Assert.DoesNotContain("/applications", endpoints, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RequireAuthorization", endpoints, StringComparison.Ordinal);

        var frontendRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "visa-detail");
        foreach (var path in Directory.EnumerateFiles(frontendRoot, "*.*", SearchOption.AllDirectories)
                     .Where(p => p.EndsWith(".ts", StringComparison.OrdinalIgnoreCase)
                                 || p.EndsWith(".tsx", StringComparison.OrdinalIgnoreCase)))
        {
            var text = File.ReadAllText(path);
            Assert.DoesNotContain("Upload Documents", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Start Application", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Continue Application", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Book Appointment", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Pay Visa Fee", text, StringComparison.Ordinal);
        }
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
        Assert.Contains("OfficialVisaFee != CommercialPrice", text, StringComparison.Ordinal);
        Assert.Contains("Visa != Pricing", text, StringComparison.Ordinal);
        Assert.Contains("VisaDefinition != VisaRequirementSet", text, StringComparison.Ordinal);
        Assert.Contains("Applicability != Rules Engine", text, StringComparison.Ordinal);
        Assert.Contains("Public Visa Page != Automatically SEO Indexed", text, StringComparison.Ordinal);
        Assert.Contains("Structured Visa Fact != Editorial Guidance", text, StringComparison.Ordinal);
        Assert.Contains("Public Visa Visibility != SEO Indexed", text, StringComparison.Ordinal);
        Assert.Contains("Visa != PublicExperience", text, StringComparison.Ordinal);
        Assert.Contains("Visa != VisaApplication", text, StringComparison.Ordinal);
        Assert.Contains("VisaApplication != Booking", text, StringComparison.Ordinal);
        Assert.Contains("VisaApplication != Payment", text, StringComparison.Ordinal);
        Assert.Contains("RequiredDocument != ApplicantSubmittedDocument", text, StringComparison.Ordinal);
        Assert.Contains("P17-R8", text, StringComparison.Ordinal);
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
        || name.Contains(".Payment.", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".HotelBooking.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".HotelBooking", StringComparison.OrdinalIgnoreCase);

    private static bool IsGeneratedOrBin(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
