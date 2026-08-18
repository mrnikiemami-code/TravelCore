using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.PublicExperience.Contracts;
using TravelCore.Modules.TripPlanner.Contracts;
using TravelCore.Modules.TripPlanner.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P18-T001: TripPlanner owns schema trip_planner; pre-transactional; not Booking/CRM/Search/Pricing.
/// </summary>
public sealed class TripPlannerBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void TripPlannerProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.TripPlanner.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.TripPlanner.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.TripPlanner.Infrastructure");
        Assert.Equal("trip_planner", TravelCore.Modules.TripPlanner.Infrastructure.TripPlannerDbContext.SchemaName);
        Assert.Equal("trip_planner", TripPlannerOwnershipBoundary.SchemaName);
    }

    [Fact]
    public void TripPlanner_DoesNot_Own_Peer_SoT_Or_Product_Types()
    {
        Assert.Equal("TripPlanner", TripPlannerOwnershipBoundary.OwnerModule);
        Assert.False(TripPlannerOwnershipBoundary.OwnsDestinationFacts);
        Assert.False(TripPlannerOwnershipBoundary.OwnsTourFacts);
        Assert.False(TripPlannerOwnershipBoundary.OwnsPlaceFacts);
        Assert.False(TripPlannerOwnershipBoundary.OwnsPricing);
        Assert.False(TripPlannerOwnershipBoundary.OwnsQuote);
        Assert.False(TripPlannerOwnershipBoundary.OwnsBooking);
        Assert.False(TripPlannerOwnershipBoundary.OwnsPayment);
        Assert.False(TripPlannerOwnershipBoundary.OwnsCrm);
        Assert.False(TripPlannerOwnershipBoundary.OwnsSearch);
        Assert.False(TripPlannerOwnershipBoundary.OwnsAgencyMarketplace);
        Assert.False(TripPlannerOwnershipBoundary.OwnsNotificationDelivery);
        Assert.False(TripPlannerOwnershipBoundary.OwnsIdentityOrParty);
        Assert.True(TripPlannerOwnershipBoundary.TripIntentImplemented);
        Assert.True(TripPlannerOwnershipBoundary.LeadImplemented);
        Assert.True(TripPlannerOwnershipBoundary.AnonymousTripIntentSupported);
        Assert.True(TripPlannerOwnershipBoundary.AuthenticatedAssociationOptional);
        Assert.True(TripPlannerOwnershipBoundary.LeadContactSnapshotImplemented);
        Assert.False(TripPlannerOwnershipBoundary.IdentityOrPartyCloneImplemented);
        Assert.True(TripPlannerOwnershipBoundary.TravelPreferencesImplemented);
        Assert.True(TripPlannerPreferenceBoundary.TravelPreferencesImplemented);
        Assert.True(TripPlannerOwnershipBoundary.LeadLifecycleImplemented);
        Assert.True(TripPlannerLifecycleBoundary.LeadLifecycleImplemented);
        Assert.False(TripPlannerOwnershipBoundary.AgencyRoutingImplemented);
        Assert.True(TripPlannerOwnershipBoundary.ConsentModelImplemented);
        Assert.False(TripPlannerOwnershipBoundary.NotificationProviderImplemented);
        Assert.False(TripPlannerOwnershipBoundary.SearchEngineImplemented);
        Assert.True(TripPlannerOwnershipBoundary.PublicExperienceCompositionImplemented);
        Assert.True(TripPlannerOwnershipBoundary.PublicPlannerRouteImplemented);
        Assert.False(TripPlannerOwnershipBoundary.RecommendationEngineImplemented);
        Assert.False(TripPlannerOwnershipBoundary.AiInfrastructureImplemented);
        Assert.False(TripPlannerOwnershipBoundary.GenericWorkflowEngineImplemented);
    }

    [Fact]
    public void TripPlannerInfrastructure_MustNotProjectReference_PeerBusinessModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.TripPlanner.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();
        Assert.True(
            hits.Count == 0,
            "TripPlanner.Infrastructure must not project-reference peer business modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void TripPlannerDomain_MustNotProjectReference_PeerBusinessModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.TripPlanner.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)
                || IsForbiddenPeerModule(name))
            .ToList();
        Assert.True(
            hits.Count == 0,
            "TripPlanner.Domain must stay free of peer modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void TripPlannerContracts_MustNotProjectReference_PeerBusinessModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.TripPlanner.Contracts");
        var hits = contracts.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();
        Assert.True(
            hits.Count == 0,
            "TripPlanner.Contracts must not project-reference peer business modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void TripPlanner_T003_MustNotImplement_Deferred_Identity_Or_Crm_Types()
    {
        var roots = new[]
        {
            Path.Combine(RepoRoot, "src", "backend", "Modules", "TripPlanner"),
        };

        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(PlannerContact|AgencyAssignment|PlannerUser|PlannerPerson|AnonymousUser|GuestAccount|Customer|Opportunity|SalesPipeline|Booking|Reservation|Checkout|Quote|Price|SearchIndex|RuleEngine|PolicyEngine|WorkflowEngine|CrmContact|Account|User|Person|Party|BookingPassenger|Passenger)\b",
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
            "T003 forbids Identity/Party/CRM/deferred product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void TripPlanner_T004_Implements_Structured_TravelPreferences_Without_Peer_Types()
    {
        Assert.True(TripPlannerOwnershipBoundary.TravelPreferencesImplemented);
        Assert.True(TripPlannerPreferenceBoundary.TravelPreferencesImplemented);
        Assert.NotNull(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.TravelPreferences"));
        Assert.NotNull(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.BudgetPreference"));
        Assert.Null(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.BookingPassenger"));
        Assert.Null(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.Price"));
        Assert.Equal(TripPlannerPreferenceBoundary.BudgetPreferenceNotEqualPrice, "BudgetPreference != Price");
        Assert.Equal(
            TripPlannerPreferenceBoundary.PlannerTravelerCompositionNotEqualBookingPassenger,
            "PlannerTravelerComposition != BookingPassenger");
    }

    [Fact]
    public void TripPlanner_T005_Implements_Minimal_Lead_Lifecycle_Without_Crm_Or_Booking()
    {
        Assert.True(TripPlannerOwnershipBoundary.LeadLifecycleImplemented);
        Assert.True(TripPlannerLifecycleBoundary.LeadLifecycleImplemented);
        Assert.Equal(["Submitted", "Contacted", "Closed", "Cancelled"], Enum.GetNames(typeof(LeadStatus)));
        Assert.Null(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.Opportunity"));
        Assert.Null(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.PipelineStage"));
        Assert.Equal(TripPlannerLifecycleBoundary.LeadStatusNotEqualCrmPipelineStage, "LeadStatus != CRM Pipeline Stage");
        Assert.Equal(TripPlannerLifecycleBoundary.ContactedNotEqualQualification, "Contacted != Qualification");
    }

    [Fact]
    public void TripPlanner_T006_Keeps_Agency_Routing_Deferred_Without_Assignment_Model()
    {
        Assert.True(TripPlannerAgencyRoutingBoundary.AgencyRoutingDecisionResolved);
        Assert.False(TripPlannerAgencyRoutingBoundary.AgencyRoutingProductImplemented);
        Assert.False(TripPlannerOwnershipBoundary.AgencyRoutingImplemented);
        Assert.Equal(TripPlannerAgencyRoutingBoundary.AgencyRoutingDeferred, "P18 Agency Routing = DEFERRED");
        Assert.Null(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.AgencyAssignment"));
        Assert.Null(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.LeadAssignment"));
        Assert.Null(typeof(Lead).GetProperty("AssignedAgencyId"));
        Assert.Null(typeof(Lead).GetProperty("PrimaryAgencyId"));

        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.TripPlanner.Infrastructure");
        var agencyMarketplaceRef = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Any(name => name.Contains("AgencyMarketplace", StringComparison.OrdinalIgnoreCase));
        Assert.False(agencyMarketplaceRef, "TripPlanner.Infrastructure must not reference AgencyMarketplace for routing.");
    }

    [Fact]
    public void TripPlanner_T007_Implements_Consent_Snapshot_Without_Notification_Provider()
    {
        Assert.True(TripPlannerConsentBoundary.ConsentModelImplemented);
        Assert.True(TripPlannerOwnershipBoundary.ConsentModelImplemented);
        Assert.False(TripPlannerConsentBoundary.NotificationProviderImplemented);
        Assert.False(TripPlannerNotificationBoundary.NotificationProviderImplemented);
        Assert.NotNull(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.LeadConsentSnapshot"));
        Assert.Null(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.TripPlanner.Domain.MarketingSubscriber"));
        Assert.Equal(
            TripPlannerConsentBoundary.ContactPermissionNotEqualMarketingConsent,
            "ContactPermission != MarketingConsent");
        Assert.Equal(TripPlannerConsentBoundary.ConsentNotEqualNotificationDelivery, "Consent != NotificationDelivery");

        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.TripPlanner.Infrastructure");
        var notificationRef = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Any(name => name.Contains("Notification", StringComparison.OrdinalIgnoreCase));
        Assert.False(notificationRef, "TripPlanner.Infrastructure must not reference Notification provider modules.");
    }

    [Fact]
    public void TripPlanner_T008_Implements_Public_Experience_Composition_Without_Search_Or_Booking()
    {
        Assert.True(TripPlannerOwnershipBoundary.PublicExperienceCompositionImplemented);
        Assert.True(TripPlannerOwnershipBoundary.PublicPlannerRouteImplemented);
        Assert.False(TripPlannerPublicCompositionBoundary.PublicExperienceOwnsLeadFacts);
        Assert.False(TripPlannerPublicCompositionBoundary.SearchEngineAllowed);
        Assert.False(TripPlannerPublicCompositionBoundary.BookingCtaAllowed);
        Assert.False(TripPlannerPublicCompositionBoundary.PaymentCtaAllowed);
        Assert.False(TripPlannerPublicCompositionBoundary.CheckoutCtaAllowed);
        Assert.False(TripPlannerPublicCompositionBoundary.CrmWorkflowAllowed);
        Assert.False(TripPlannerPublicCompositionBoundary.AgencyRoutingAllowed);
        Assert.False(TripPlannerPublicCompositionBoundary.NotificationProviderAllowed);
        Assert.Equal("/plan", TripPlannerPublicCompositionBoundary.PublicRoutePattern);
        Assert.Equal("/api/trip-planner/public", TripPlannerPublicCompositionBoundary.PublicApiGroup);
        Assert.Equal("RequestFollowUpOnly", TripPlannerPublicCompositionBoundary.HonestCtaPosture);
        Assert.False(PublicExperienceTripPlannerCompositionBoundary.PublicExperienceOwnsLeadFacts);
        Assert.False(PublicExperienceTripPlannerCompositionBoundary.SearchEngineAllowed);
        Assert.False(PublicExperienceTripPlannerCompositionBoundary.BookingCtaAllowed);

        var frontendPlanRoute = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "app",
            "[locale]",
            "plan",
            "page.tsx");
        Assert.True(File.Exists(frontendPlanRoute), frontendPlanRoute);
        var frontendText = File.ReadAllText(frontendPlanRoute);
        Assert.Contains("Honest follow-up CTA only", frontendText, StringComparison.Ordinal);
        Assert.Contains("honestCtaNote", frontendText, StringComparison.Ordinal);
        Assert.DoesNotContain("BookNow", frontendText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TripPlanner_T009_Hardening_Evidence_Pack_Locks_Accepted_Boundaries()
    {
        var evidence = Path.Combine(RepoRoot, "docs", "plans", "P18-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(evidence), evidence);
        var text = File.ReadAllText(evidence);

        string[] required =
        [
            "TripPlanner != Booking",
            "TripPlanner != Payment",
            "TripPlanner != Pricing",
            "TripPlanner != CRM",
            "TripPlanner != Search",
            "TripPlanner != AgencyMarketplace",
            "TripPlanner != Notification Provider",
            "TripPlanner != Party Identity",
            "TripIntent != Lead",
            "PlannerActorReference != Identity Account entity",
            "LeadContactSnapshot != Party",
            "DestinationPreference != Destination Source of Truth",
            "PlannerTravelerComposition != BookingPassenger",
            "BudgetPreference != Price",
            "BudgetPreference != Quote",
            "InterestPreference != Search Facet Authority",
            "LeadStatus != CRM Pipeline Stage",
            "Lead != CRM Opportunity",
            "Contacted != Qualification",
            "Closed != Booking conversion",
            "Lead != AgencyAssignment",
            "LeadStatus != AgencyAssignmentStatus",
            "ContactPermission != MarketingConsent",
            "Consent != NotificationDelivery",
            "LeadContactSnapshot != LeadConsentSnapshot",
            "FollowUpContactAllowed != AgencyDataSharingPermission",
            "PublicExperience != TripPlanner Source of Truth",
            "Planner Submission != Booking",
            "Planner Submission != Quote",
            "Planner Submission != Payment",
            "Planner Discovery != Search Ownership",
            "P18 Agency Routing = DEFERRED",
        ];

        foreach (var invariant in required)
        {
            Assert.Contains(invariant, text, StringComparison.Ordinal);
        }

        var toursRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "tours");
        Assert.True(File.Exists(Path.Combine(toursRoot, "[slug]", "page.tsx")));
        Assert.False(Directory.Exists(Path.Combine(toursRoot, "[productKey]")));
        Assert.True(File.Exists(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "plan", "page.tsx")));

        var endpoints = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "TripPlanner",
            "TravelCore.Modules.TripPlanner.Infrastructure",
            "Endpoints",
            "TripPlannerPublicEndpoints.cs");
        var endpointText = File.ReadAllText(endpoints);
        Assert.DoesNotContain("MapGet(\"/leads", endpointText, StringComparison.Ordinal);
        Assert.DoesNotContain("MapGet(\"/intents\"", endpointText, StringComparison.Ordinal);
        Assert.Contains("MapPost(\"/intents/{intentId:guid}/submit\"", endpointText, StringComparison.Ordinal);

        Assert.False(TripPlannerOwnershipBoundary.AgencyRoutingImplemented);
        Assert.False(TripPlannerOwnershipBoundary.NotificationProviderImplemented);
        Assert.False(TripPlannerOwnershipBoundary.OwnsBooking);
        Assert.False(TripPlannerOwnershipBoundary.OwnsPayment);
        Assert.False(TripPlannerOwnershipBoundary.OwnsCrm);
        Assert.False(TripPlannerOwnershipBoundary.OwnsSearch);
        Assert.True(TripPlannerOwnershipBoundary.PublicPlannerRouteImplemented);
    }

    [Fact]
    public void TripPlanner_Module_Keeps_Search_And_Ai_Engines_Out()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "TripPlanner");
        Assert.True(Directory.Exists(root), root);

        var forbidden = new[]
        {
            "Elasticsearch",
            "OpenSearch",
            "pg_trgm",
            "to_tsvector",
            "embeddings",
            "vector search",
            "RAG",
            "RuleEngine",
            "WorkflowEngine",
            "SmtpClient",
            "Twilio",
        };

        var hits = new List<string>();
        foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            if (IsGeneratedOrBin(path)
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

        Assert.True(hits.Count == 0, "TripPlanner must not introduce Search/AI/CRM engines:\n" + string.Join('\n', hits));
        Assert.Null(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.TripPlanner.Domain.Destination"));
        Assert.NotNull(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.TripPlanner.Domain.TripIntent"));
        Assert.NotNull(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.TripPlanner.Domain.Lead"));
        Assert.Null(typeof(TripPlannerDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.TripPlanner.Domain.Booking"));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Notification")));
    }

    [Fact]
    public void TripPlanner_Evidence_Keeps_Ascii_Invariants()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P18-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);
        var text = File.ReadAllText(plan);
        Assert.Contains("TripIntent != Lead", text, StringComparison.Ordinal);
        Assert.Contains("Lead != Booking", text, StringComparison.Ordinal);
        Assert.Contains("Lead contact != Party master identity", text, StringComparison.Ordinal);
        Assert.Contains("LeadContactSnapshot != Party", text, StringComparison.Ordinal);
        Assert.Contains("P18-R1", text, StringComparison.Ordinal);
        Assert.Contains("P18-R2", text, StringComparison.Ordinal);
        Assert.Contains("P18-R3", text, StringComparison.Ordinal);
        Assert.Contains("P18-R4", text, StringComparison.Ordinal);
        Assert.Contains("P18-R5", text, StringComparison.Ordinal);
        Assert.Contains("BudgetPreference != Price", text, StringComparison.Ordinal);
        Assert.Contains("PlannerTravelerComposition != Booking Passenger", text, StringComparison.Ordinal);
        Assert.Contains("LeadStatus != CRM Pipeline Stage", text, StringComparison.Ordinal);
        Assert.Contains("P18 Agency Routing = DEFERRED", text, StringComparison.Ordinal);
        Assert.Contains("P18-R7", text, StringComparison.Ordinal);
        Assert.Contains("ContactPermission != MarketingConsent", text, StringComparison.Ordinal);
        Assert.Contains("Consent != NotificationDelivery", text, StringComparison.Ordinal);
        Assert.Contains("P18-R8", text, StringComparison.Ordinal);
        Assert.Contains("PublicExperience ≠ Lead", text, StringComparison.Ordinal);
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
        || name.Contains(".Visa.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Visa", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Identity.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Identity", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Party.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Party", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Booking.", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Payment.", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".PublicExperience.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".PublicExperience", StringComparison.OrdinalIgnoreCase);

    private static bool IsGeneratedOrBin(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
