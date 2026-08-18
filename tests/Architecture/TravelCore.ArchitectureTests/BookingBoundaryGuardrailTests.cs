using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P19-T001: Booking owns schema booking; no aggregate/lifecycle/passengers/payment yet.
/// </summary>
public sealed class BookingBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void BookingProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Booking.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Booking.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Booking.Infrastructure");
        Assert.Equal("booking", TravelCore.Modules.Booking.Infrastructure.BookingDbContext.SchemaName);
        Assert.Equal("booking", BookingOwnershipBoundary.SchemaName);
        Assert.Equal("TourDeparture", BookingOwnershipBoundary.InitialTarget);
    }

    [Fact]
    public void Booking_DoesNot_Own_Peer_SoT_Or_Product_Types()
    {
        Assert.Equal("Booking", BookingOwnershipBoundary.OwnerModule);
        Assert.False(BookingOwnershipBoundary.OwnsTourCatalog);
        Assert.False(BookingOwnershipBoundary.OwnsTourDeparture);
        Assert.False(BookingOwnershipBoundary.OwnsCapacityDefinition);
        Assert.True(BookingOwnershipBoundary.OwnsCapacityConsumption);
        Assert.True(BookingOwnershipBoundary.CapacityConsumptionImplemented);
        Assert.False(BookingOwnershipBoundary.OwnsPricing);
        Assert.False(BookingOwnershipBoundary.OwnsQuote);
        Assert.False(BookingOwnershipBoundary.OwnsPayment);
        Assert.False(BookingOwnershipBoundary.OwnsPartyOrIdentity);
        Assert.False(BookingOwnershipBoundary.OwnsAgencyMarketplace);
        Assert.False(BookingOwnershipBoundary.OwnsSearch);
        Assert.False(BookingOwnershipBoundary.OwnsSeo);
        Assert.False(BookingOwnershipBoundary.OwnsNotificationDelivery);
        Assert.False(BookingOwnershipBoundary.OwnsVisaApplication);
        Assert.False(BookingOwnershipBoundary.OwnsTripPlannerLead);
        Assert.True(BookingOwnershipBoundary.BookingAggregateImplemented);
        Assert.True(BookingOwnershipBoundary.BookingStatusImplemented);
        Assert.True(BookingOwnershipBoundary.CapacityHoldImplemented);
        Assert.True(BookingOwnershipBoundary.ContactSnapshotImplemented);
        Assert.True(BookingOwnershipBoundary.QuoteIntegrationImplemented);
        Assert.False(BookingOwnershipBoundary.PublicBookingSurfaceImplemented);
        Assert.False(BookingOwnershipBoundary.AiInfrastructureImplemented);
        Assert.False(BookingOwnershipBoundary.GenericWorkflowEngineImplemented);
        Assert.False(BookingOwnershipBoundary.NotificationProviderImplemented);
    }

    [Fact]
    public void BookingInfrastructure_MustNotProjectReference_PeerBusinessModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.Booking.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                IsForbiddenPeerModule(name)
                && !string.Equals(name, "TravelCore.Modules.Pricing.Contracts", StringComparison.Ordinal))
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Booking.Infrastructure must not project-reference peer business modules:\n" + string.Join('\n', hits));
        Assert.Contains(
            infra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Modules.Pricing.Contracts");
        Assert.DoesNotContain(
            infra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name is "TravelCore.Modules.Pricing.Infrastructure" or "TravelCore.Modules.Pricing.Domain");
    }

    [Fact]
    public void BookingDomain_MustNotProjectReference_PeerBusinessModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.Booking.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)
                || IsForbiddenPeerModule(name))
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Booking.Domain must stay free of peer modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void BookingContracts_MustNotProjectReference_PeerBusinessModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.Booking.Contracts");
        var hits = contracts.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Booking.Contracts must not project-reference peer business modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Booking_T001_MustNotImplement_Deferred_Product_Types()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking");
        Assert.True(Directory.Exists(root), root);

        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(SeatHold|Reservation|ReservedSeats|ConfirmedSeats|ReleasedSeats|PaymentIntent|PaymentStatus|Quote|Price|Checkout|Lead|VisaApplication|AgencyBooking|SearchIndex|RuleEngine|PolicyEngine|WorkflowEngine|TravelDocument|Passport)\b",
            RegexOptions.Compiled);

        var hits = new List<string>();
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

        Assert.True(
            hits.Count == 0,
            "T003 still forbids passenger/payment/pricing/public product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Booking_Module_Keeps_Search_Ai_Payment_And_Public_Surfaces_Out()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking");
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
            "MapGet(\"/api/booking",
            "MapPost(\"/api/booking",
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

        Assert.True(hits.Count == 0, "Booking T002 must not introduce Search/AI/public API:\n" + string.Join('\n', hits));
        Assert.NotNull(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.Booking"));
        Assert.NotNull(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.BookingStatus"));
        Assert.NotNull(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.BookingPassenger"));
        Assert.NotNull(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.CapacityHold"));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "checkout")));
        Assert.False(File.Exists(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "bookings", "page.tsx")));
    }

    [Fact]
    public void Booking_Evidence_Keeps_Ascii_Invariants()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P19-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);
        var text = File.ReadAllText(plan);
        Assert.Contains("Booking != TourProduct", text, StringComparison.Ordinal);
        Assert.Contains("Booking != TourDeparture", text, StringComparison.Ordinal);
        Assert.Contains("Booking != Price", text, StringComparison.Ordinal);
        Assert.Contains("Booking != Quote", text, StringComparison.Ordinal);
        Assert.Contains("Booking != Payment", text, StringComparison.Ordinal);
        Assert.Contains("P19-R1", text, StringComparison.Ordinal);
        Assert.Contains("schema `booking`", text, StringComparison.Ordinal);
        Assert.Contains("Pending", text, StringComparison.Ordinal);
        Assert.Contains("Confirmed != PaymentSucceeded", text, StringComparison.Ordinal);
        Assert.Contains("Cancelled != Refunded", text, StringComparison.Ordinal);
        Assert.Contains("P19-R2", text, StringComparison.Ordinal);
        Assert.Contains("CapacityDefinition != CapacityConsumption", text, StringComparison.Ordinal);
        Assert.Contains("P19-R3", text, StringComparison.Ordinal);
        Assert.Contains("CapacityHoldStatus != BookingStatus", text, StringComparison.Ordinal);
        Assert.Contains("Price != Quote", text, StringComparison.Ordinal);
        Assert.Contains("Quote != BookingMonetarySnapshot", text, StringComparison.Ordinal);
        Assert.Contains("BookingMonetarySnapshot != PaymentAmount", text, StringComparison.Ordinal);
        Assert.Contains("Booking != Pricing Authority", text, StringComparison.Ordinal);
        Assert.Contains("QuoteExpired != BookingStatus", text, StringComparison.Ordinal);
        Assert.Contains("QuoteExpiresAt != CapacityHold.ExpiresAt", text, StringComparison.Ordinal);
        Assert.Contains("BudgetPreference != BookingMonetarySnapshot", text, StringComparison.Ordinal);
        Assert.Contains("Booking != Payment", text, StringComparison.Ordinal);
        Assert.Contains("BookingMonetarySnapshot != PaymentTransaction", text, StringComparison.Ordinal);
        Assert.Contains("PaymentSucceeded != BookingConfirmed", text, StringComparison.Ordinal);
        Assert.Contains("BookingCancelled != PaymentRefunded", text, StringComparison.Ordinal);
        Assert.Contains("DEFERRED to Payment integration", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Booking_T002_Lifecycle_Is_Minimal_And_Not_Payment_Or_Capacity()
    {
        Assert.Equal(
            new[] { BookingStatus.Pending, BookingStatus.Confirmed, BookingStatus.Cancelled },
            Enum.GetValues<BookingStatus>());
        Assert.False(BookingLifecycleBoundary.UnrestrictedConfirmationImplemented);
        Assert.False(BookingLifecycleBoundary.ConfirmedToCancelledImplemented);
        Assert.False(BookingLifecycleBoundary.ExpiredStatusImplemented);
        Assert.False(BookingLifecycleBoundary.AwaitingPaymentStatusImplemented);
        Assert.False(BookingLifecycleBoundary.PaidStatusImplemented);
        Assert.False(BookingLifecycleBoundary.RefundedStatusImplemented);
        Assert.Equal("Confirmed != PaymentSucceeded", BookingLifecycleBoundary.ConfirmedIsNotPaymentSucceeded);
        Assert.Equal("Cancelled != Refunded", BookingLifecycleBoundary.CancelledIsNotRefunded);
        Assert.Equal("BookingStatus != PaymentStatus", BookingLifecycleBoundary.BookingStatusIsNotPaymentStatus);
        Assert.Equal("BookingStatus != CapacityStatus", BookingLifecycleBoundary.BookingStatusIsNotCapacityStatus);
        Assert.Equal("BookingStatus != QuoteStatus", BookingLifecycleBoundary.BookingStatusIsNotQuoteStatus);
        var methodNames = typeof(Booking).GetMethods(
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(m => m.Name)
            .ToArray();
        Assert.DoesNotContain("Confirm", methodNames);
        Assert.DoesNotContain("SetStatus", methodNames);
        Assert.Contains("CancelPending", methodNames);
        Assert.Equal(
            new[]
            {
                "ActorReference",
                "Contact",
                "CreatedAt",
                "Id",
                "MonetarySnapshot",
                "PartyReference",
                "PassengerCount",
                "Passengers",
                "Status",
                "StatusChangedAt",
                "TourDeparture",
            },
            typeof(Booking).GetProperties().Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
        Assert.NotNull(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.BookingPassenger"));
        Assert.NotNull(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.CapacityHold"));
    }

    [Fact]
    public void Booking_T003_CapacityHold_Is_Consumption_Not_Definition_Or_BookingStatus()
    {
        Assert.Equal(
            new[]
            {
                CapacityHoldStatus.Active,
                CapacityHoldStatus.Consumed,
                CapacityHoldStatus.Released,
                CapacityHoldStatus.Expired,
            },
            Enum.GetValues<CapacityHoldStatus>());
        Assert.Equal("Tour", CapacityConsumptionBoundary.CapacityDefinitionOwner);
        Assert.Equal("Booking", CapacityConsumptionBoundary.CapacityConsumptionOwner);
        Assert.Equal("CapacityDefinition != CapacityConsumption", CapacityConsumptionBoundary.CapacityDefinitionIsNotCapacityConsumption);
        Assert.Equal("CapacityHoldStatus != BookingStatus", CapacityConsumptionBoundary.CapacityHoldStatusIsNotBookingStatus);
        Assert.Equal("Pending != CapacityHeld", CapacityConsumptionBoundary.PendingIsNotCapacityHeld);
        Assert.Equal("Consumed != BookingConfirmed", CapacityConsumptionBoundary.ConsumedIsNotBookingConfirmed);
        Assert.Equal("Expired Hold != Expired Booking", CapacityConsumptionBoundary.ExpiredHoldIsNotExpiredBooking);
        Assert.Equal("HeldSeatCount != BookingPassenger", CapacityConsumptionBoundary.HeldSeatCountIsNotBookingPassenger);
        Assert.Equal("NOT Tour Source of Truth", CapacityConsumptionBoundary.ObservedCapacityIsNotTourSourceOfTruth);
        Assert.Equal("PostgreSqlAdvisoryTransactionLock", CapacityConsumptionBoundary.ConcurrencyMechanism);
        Assert.False(CapacityConsumptionBoundary.ProcessLocalLockIsAuthoritative);
        Assert.False(CapacityConsumptionBoundary.ClientInventedConfiguredCapacityIsAuthoritative);
        Assert.False(CapacityConsumptionBoundary.UnrestrictedBookingConfirmationImplemented);
        Assert.False(CapacityConsumptionBoundary.PublicHoldSurfaceImplemented);
        Assert.False(CapacityConsumptionBoundary.HoldDurationHardcoded);
        Assert.True(BookingOwnershipBoundary.CapacityHoldImplemented);
        Assert.True(BookingOwnershipBoundary.CapacityConsumptionImplemented);
        Assert.True(BookingOwnershipBoundary.BookingPassengerImplemented);
        Assert.True(BookingOwnershipBoundary.ContactSnapshotImplemented);
        Assert.False(BookingOwnershipBoundary.PublicBookingSurfaceImplemented);
        Assert.DoesNotContain("Expired", Enum.GetNames<BookingStatus>());
        Assert.Null(typeof(Booking).GetMethod("Confirm"));

        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking");
        var lockHits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path).Select((line, i) => (path, line, i)))
            .Where(x =>
            {
                var trimmed = x.line.TrimStart();
                return trimmed.Contains("Semaphore", StringComparison.Ordinal)
                    || trimmed.Contains("Mutex", StringComparison.Ordinal)
                    || trimmed.Contains("ReaderWriterLock", StringComparison.Ordinal)
                    || Regex.IsMatch(trimmed, @"\block\s*\(");
            })
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();
        Assert.True(lockHits.Count == 0, "Process-local locks are forbidden as correctness:\n" + string.Join('\n', lockHits));

        var service = File.ReadAllText(Path.Combine(
            root,
            "TravelCore.Modules.Booking.Infrastructure",
            "Services",
            "BookingCapacityService.cs"));
        Assert.Contains("pg_advisory_xact_lock", service, StringComparison.Ordinal);
    }

    [Fact]
    public void Booking_T004_People_Are_Transaction_Snapshots_Not_Masters()
    {
        Assert.Equal(
            new[] { TravelerCategory.Adult, TravelerCategory.Child, TravelerCategory.Infant },
            Enum.GetValues<TravelerCategory>());
        Assert.Equal("PlannerTravelerComposition != BookingPassenger", BookingPeopleBoundary.PlannerTravelerCompositionIsNotBookingPassenger);
        Assert.Equal("BookingPassenger != Party Person Master", BookingPeopleBoundary.BookingPassengerIsNotPartyPersonMaster);
        Assert.Equal("BookingContactSnapshot != Party", BookingPeopleBoundary.BookingContactSnapshotIsNotParty);
        Assert.Equal("BookingContactSnapshot != Identity Account", BookingPeopleBoundary.BookingContactSnapshotIsNotIdentityAccount);
        Assert.Equal("BookingPassenger != CapacityHold", BookingPeopleBoundary.BookingPassengerIsNotCapacityHold);
        Assert.Equal("BookingPassenger != VisaApplication", BookingPeopleBoundary.BookingPassengerIsNotVisaApplication);
        Assert.Equal("BookingPassenger != TravelDocument", BookingPeopleBoundary.BookingPassengerIsNotTravelDocument);
        Assert.Equal("Passenger PII != public Search/SEO data", BookingPeopleBoundary.PassengerPiiIsNotPublicSearch);
        Assert.Equal("future explicit operational/legal policy", BookingPeopleBoundary.PiiRetention);
        Assert.False(BookingPeopleBoundary.BirthDateImplemented);
        Assert.False(BookingPeopleBoundary.PassportImplemented);
        Assert.False(BookingPeopleBoundary.DocumentUploadImplemented);
        Assert.False(BookingPeopleBoundary.InfantSeatPolicyImplemented);
        Assert.False(BookingPeopleBoundary.PostConfirmationPassengerAmendmentImplemented);
        Assert.True(BookingPeopleBoundary.AllowPassengerCountAtMostHeldSeats);
        Assert.True(BookingOwnershipBoundary.BookingPassengerImplemented);
        Assert.True(BookingOwnershipBoundary.ContactSnapshotImplemented);
        Assert.DoesNotContain("PassengerPending", Enum.GetNames<BookingStatus>());
        Assert.Null(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.TravelDocument"));
        Assert.Null(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.Passport"));
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P19-implementation-plan.md"));
        Assert.Contains("PlannerTravelerComposition != BookingPassenger", plan, StringComparison.Ordinal);
        Assert.Contains("BookingPassenger != Party Person Master", plan, StringComparison.Ordinal);
    }

    [Fact]
    public void Booking_T006_Orchestration_Does_Not_Implement_Payment_Or_Confirm()
    {
        Assert.Equal("Booking != Payment", BookingOrchestrationBoundary.BookingIsNotPayment);
        Assert.Equal("BookingStatus != PaymentStatus", BookingOrchestrationBoundary.BookingStatusIsNotPaymentStatus);
        Assert.Equal("BookingMonetarySnapshot != PaymentTransaction", BookingOrchestrationBoundary.BookingMonetarySnapshotIsNotPaymentTransaction);
        Assert.Equal("PaymentSucceeded != BookingConfirmed", BookingOrchestrationBoundary.PaymentSucceededIsNotBookingConfirmed);
        Assert.Equal("BookingCancelled != PaymentRefunded", BookingOrchestrationBoundary.BookingCancelledIsNotPaymentRefunded);
        Assert.Equal("DEFERRED to Payment integration", BookingOrchestrationBoundary.ExecutableConfirmWorkflow);
        Assert.Equal("DEFERRED", BookingOrchestrationBoundary.ConfirmedCancellation);
        Assert.False(BookingOrchestrationBoundary.FakePaymentImplemented);
        Assert.False(BookingOrchestrationBoundary.PaymentDrivenConfirmationImplemented);
        Assert.False(BookingOrchestrationBoundary.CallerControlledPaymentBooleanImplemented);
        Assert.False(BookingOrchestrationBoundary.ConfirmedToCancelledImplemented);
        Assert.True(BookingOrchestrationBoundary.PendingCancellationImplemented);
        Assert.True(BookingOrchestrationBoundary.PendingCancellationReleasesActiveHold);
        Assert.False(BookingOwnershipBoundary.PaymentIntegrationImplemented);
        Assert.False(BookingLifecycleBoundary.UnrestrictedConfirmationImplemented);
        Assert.False(BookingLifecycleBoundary.ConfirmedToCancelledImplemented);
        Assert.DoesNotContain("AwaitingPayment", Enum.GetNames<BookingStatus>());
        Assert.DoesNotContain("Paid", Enum.GetNames<BookingStatus>());
        Assert.DoesNotContain("Refunded", Enum.GetNames<BookingStatus>());
        Assert.Null(typeof(Booking).GetMethod("Confirm"));
        Assert.NotNull(typeof(BookingDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.Booking.Domain.BookingOrchestrationBoundary"));
        var service = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Booking",
            "TravelCore.Modules.Booking.Infrastructure",
            "Services",
            "BookingCancellationService.cs"));
        Assert.Contains("pg_advisory_xact_lock", service, StringComparison.Ordinal);
        Assert.DoesNotContain("paymentSucceeded", service, StringComparison.Ordinal);
        Assert.DoesNotContain("isPaid", service, StringComparison.Ordinal);
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P19-implementation-plan.md"));
        Assert.Contains("Booking != Payment", plan, StringComparison.Ordinal);
        Assert.Contains("BookingMonetarySnapshot != PaymentTransaction", plan, StringComparison.Ordinal);
        Assert.Contains("PaymentSucceeded != BookingConfirmed", plan, StringComparison.Ordinal);
        Assert.Contains("BookingCancelled != PaymentRefunded", plan, StringComparison.Ordinal);
        Assert.Contains("DEFERRED to Payment integration", plan, StringComparison.Ordinal);
    }

    [Fact]
    public void Booking_T005_Monetary_Snapshot_Is_Not_Pricing_Or_Payment()
    {
        Assert.Equal("Price != Quote", BookingMonetaryBoundary.PriceIsNotQuote);
        Assert.Equal("Quote != BookingMonetarySnapshot", BookingMonetaryBoundary.QuoteIsNotBookingMonetarySnapshot);
        Assert.Equal("BookingMonetarySnapshot != PaymentAmount", BookingMonetaryBoundary.BookingMonetarySnapshotIsNotPaymentAmount);
        Assert.Equal("Booking != Pricing Authority", BookingMonetaryBoundary.BookingIsNotPricingAuthority);
        Assert.Equal("QuoteExpired != BookingStatus", BookingMonetaryBoundary.QuoteExpiredIsNotBookingStatus);
        Assert.Equal("QuoteExpiresAt != CapacityHold.ExpiresAt", BookingMonetaryBoundary.QuoteExpiresAtIsNotCapacityHoldExpiresAt);
        Assert.Equal("BudgetPreference != BookingMonetarySnapshot", BookingMonetaryBoundary.BudgetPreferenceIsNotBookingMonetarySnapshot);
        Assert.False(BookingMonetaryBoundary.RecalculationImplemented);
        Assert.False(BookingMonetaryBoundary.FxImplemented);
        Assert.False(BookingMonetaryBoundary.RepricingImplemented);
        Assert.False(BookingMonetaryBoundary.PaymentInferenceImplemented);
        Assert.True(BookingOwnershipBoundary.QuoteIntegrationImplemented);
        Assert.False(BookingOwnershipBoundary.OwnsQuote);
        Assert.False(BookingOwnershipBoundary.OwnsPricing);
        Assert.False(BookingOwnershipBoundary.PaymentIntegrationImplemented);
        Assert.DoesNotContain("Quoted", Enum.GetNames<BookingStatus>());
        Assert.DoesNotContain("PriceLocked", Enum.GetNames<BookingStatus>());
        Assert.DoesNotContain("QuoteExpired", Enum.GetNames<BookingStatus>());
        Assert.Null(typeof(Booking).GetMethod("Confirm"));
        Assert.Null(typeof(Booking).GetMethod("SetPrice"));
        Assert.Null(typeof(Booking).GetMethod("SetTotal"));
        Assert.Null(typeof(Booking).GetMethod("UpdateAmount"));
        Assert.NotNull(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.BookingMonetarySnapshot"));
        Assert.Null(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.Quote"));
        Assert.Null(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.Price"));
        Assert.Contains("AcceptQuote", typeof(Booking).GetMethods().Select(m => m.Name));

        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.Booking.Domain");
        Assert.DoesNotContain(
            domain.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name.Contains(".Pricing.", StringComparison.OrdinalIgnoreCase));

        var infraCs = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Booking",
            "TravelCore.Modules.Booking.Infrastructure");
        var infraHits = Directory.EnumerateFiles(infraCs, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path).Select((line, i) => (path, line, i)))
            .Where(x =>
                x.line.Contains("TravelCore.Modules.Pricing.Infrastructure", StringComparison.Ordinal)
                || x.line.Contains("TravelCore.Modules.Pricing.Domain", StringComparison.Ordinal)
                || x.line.Contains("PricingDbContext", StringComparison.Ordinal))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();
        Assert.True(infraHits.Count == 0, "Booking must not reach Pricing engine/persistence:\n" + string.Join('\n', infraHits));
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
        || name.Contains(".TripPlanner.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".TripPlanner", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Payment.", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".PublicExperience.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".PublicExperience", StringComparison.OrdinalIgnoreCase);

    private static bool IsGeneratedOrBin(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
