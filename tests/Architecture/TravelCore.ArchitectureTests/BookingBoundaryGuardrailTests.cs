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
        Assert.True(BookingOwnershipBoundary.BookingSourceContextImplemented);
        Assert.True(BookingOwnershipBoundary.PublicBookingSurfaceImplemented);
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
                && !string.Equals(name, "TravelCore.Modules.Pricing.Contracts", StringComparison.Ordinal)
                && !string.Equals(name, "TravelCore.Modules.Tour.Contracts", StringComparison.Ordinal)
                && !string.Equals(name, "TravelCore.Modules.AgencyMarketplace.Contracts", StringComparison.Ordinal)
                && !string.Equals(name, "TravelCore.Modules.Payment.Contracts", StringComparison.Ordinal))
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Booking.Infrastructure must not project-reference peer business modules:\n" + string.Join('\n', hits));
        Assert.Contains(
            infra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Modules.Pricing.Contracts");
        Assert.Contains(
            infra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Modules.Tour.Contracts");
        Assert.Contains(
            infra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Modules.AgencyMarketplace.Contracts");
        Assert.Contains(
            infra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Modules.Payment.Contracts");
        Assert.DoesNotContain(
            infra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name is "TravelCore.Modules.Pricing.Infrastructure" or "TravelCore.Modules.Pricing.Domain");
        Assert.DoesNotContain(
            infra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name is "TravelCore.Modules.Tour.Infrastructure" or "TravelCore.Modules.Tour.Domain");
        Assert.DoesNotContain(
            infra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name is "TravelCore.Modules.AgencyMarketplace.Infrastructure"
                or "TravelCore.Modules.AgencyMarketplace.Domain");
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
            @"\b(class|record|enum|struct|interface)\s+(SeatHold|Reservation|ReservedSeats|ConfirmedSeats|ReleasedSeats|PaymentIntent|PaymentStatus|Quote|Price|Checkout|Lead|VisaApplication|AgencyBooking|DirectBooking|SearchIndex|RuleEngine|PolicyEngine|WorkflowEngine|TravelDocument|Passport|Commission|AgencyPrice)\b",
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

        Assert.True(
            hits.Count == 0,
            "Booking T008 must not introduce Search/AI engines:\n" + string.Join('\n', hits));
        Assert.NotNull(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.Booking"));
        Assert.NotNull(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.BookingStatus"));
        Assert.NotNull(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.BookingPassenger"));
        Assert.NotNull(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.CapacityHold"));
        Assert.True(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment")));
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
        Assert.Contains("Booking != AgencyMarketplace", text, StringComparison.Ordinal);
        Assert.Contains("BookingSourceKind != BookingStatus", text, StringComparison.Ordinal);
        Assert.Contains("AgencyOffer != Booking", text, StringComparison.Ordinal);
        Assert.Contains("AgencyOffer != Quote", text, StringComparison.Ordinal);
        Assert.Contains("Agency context != Pricing Authority", text, StringComparison.Ordinal);
        Assert.Contains("Lead != Booking", text, StringComparison.Ordinal);
        Assert.Contains("VisaApplication != Booking", text, StringComparison.Ordinal);
        Assert.Contains("PublicExperience != Booking Source of Truth", text, StringComparison.Ordinal);
        Assert.Contains("Public Booking initiation != Booking confirmation", text, StringComparison.Ordinal);
        Assert.Contains("Pending != Confirmed", text, StringComparison.Ordinal);
        Assert.Contains("BookingId != Access Credential", text, StringComparison.Ordinal);
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
                "Source",
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
        Assert.True(BookingOwnershipBoundary.PublicBookingSurfaceImplemented);
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
        Assert.Equal("AuthoritativePaymentSuccessRequired", BookingOrchestrationBoundary.ExecutableConfirmWorkflow);
        Assert.Equal("DEFERRED", BookingOrchestrationBoundary.ConfirmedCancellation);
        Assert.False(BookingOrchestrationBoundary.FakePaymentImplemented);
        Assert.True(BookingOrchestrationBoundary.PaymentDrivenConfirmationImplemented);
        Assert.False(BookingOrchestrationBoundary.CallerControlledPaymentBooleanImplemented);
        Assert.False(BookingOrchestrationBoundary.ConfirmedToCancelledImplemented);
        Assert.True(BookingOrchestrationBoundary.PendingCancellationImplemented);
        Assert.True(BookingOrchestrationBoundary.PendingCancellationReleasesActiveHold);
        Assert.True(BookingOrchestrationBoundary.ConfirmationRecoveryEvidenceImplemented);
        Assert.Equal("RecoveryIssue != Refund", BookingOrchestrationBoundary.RecoveryIssueIsNotRefund);
        Assert.True(BookingOwnershipBoundary.PaymentIntegrationImplemented);
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
        Assert.True(BookingOwnershipBoundary.PaymentIntegrationImplemented);
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

    [Fact]
    public void Booking_T007_Source_Is_Not_Marketplace_Pricing_Or_Acceptance()
    {
        Assert.Equal("Booking != AgencyMarketplace", BookingSourceBoundary.BookingIsNotAgencyMarketplace);
        Assert.Equal("BookingSourceKind != BookingStatus", BookingSourceBoundary.BookingSourceKindIsNotBookingStatus);
        Assert.Equal("AgencyOffer != Booking", BookingSourceBoundary.AgencyOfferIsNotBooking);
        Assert.Equal("AgencyOffer != Quote", BookingSourceBoundary.AgencyOfferIsNotQuote);
        Assert.Equal("AgencyOffer != Price", BookingSourceBoundary.AgencyOfferIsNotPrice);
        Assert.Equal("Agency context != Pricing Authority", BookingSourceBoundary.AgencyContextIsNotPricingAuthority);
        Assert.Equal("Lead != Booking", BookingSourceBoundary.LeadIsNotBooking);
        Assert.Equal("VisaApplication != Booking", BookingSourceBoundary.VisaApplicationIsNotBooking);
        Assert.Equal("BookingStatus != AgencyOfferStatus", BookingSourceBoundary.BookingStatusIsNotAgencyOfferStatus);
        Assert.Equal("BookingStatus != AgencyAcceptanceStatus", BookingSourceBoundary.BookingStatusIsNotAgencyAcceptanceStatus);
        Assert.Equal(
            "AgencyOfferReference is optional; AgencyProfileReference is required for Agency source",
            BookingSourceBoundary.AgencyOfferReferenceRequirement);
        Assert.Equal("future object-level authorization; not globally visible", BookingSourceBoundary.AgencyAccessPolicy);
        Assert.True(BookingSourceBoundary.DirectAndAgencyUseSameAggregate);
        Assert.False(BookingSourceBoundary.AgencyBookingAggregateImplemented);
        Assert.False(BookingSourceBoundary.DirectBookingAggregateImplemented);
        Assert.False(BookingSourceBoundary.AgencyPriceOverrideImplemented);
        Assert.False(BookingSourceBoundary.CommissionImplemented);
        Assert.False(BookingSourceBoundary.SettlementImplemented);
        Assert.False(BookingSourceBoundary.AgencyAcceptanceLifecycleImplemented);
        Assert.False(BookingSourceBoundary.AgencyCapacityPoolImplemented);
        Assert.False(BookingSourceBoundary.AgencyPiiSharingImplemented);
        Assert.False(BookingSourceBoundary.AgencyInboxImplemented);
        Assert.False(BookingSourceBoundary.LeadConversionImplemented);
        Assert.False(BookingSourceBoundary.SourceMutationImplemented);
        Assert.True(BookingOwnershipBoundary.BookingSourceContextImplemented);
        Assert.False(BookingOwnershipBoundary.OwnsAgencyMarketplace);
        Assert.True(BookingOwnershipBoundary.PublicBookingSurfaceImplemented);
        Assert.Equal(
            new[] { BookingSourceKind.Direct, BookingSourceKind.Agency },
            Enum.GetValues<BookingSourceKind>());
        Assert.DoesNotContain("AwaitingAgency", Enum.GetNames<BookingStatus>());
        Assert.DoesNotContain("AgencyAccepted", Enum.GetNames<BookingStatus>());
        Assert.DoesNotContain("AgencyRejected", Enum.GetNames<BookingStatus>());
        Assert.Null(typeof(Booking).GetMethod("Confirm"));
        Assert.Null(typeof(Booking).GetMethod("SetSource"));
        Assert.Null(typeof(Booking).GetMethod("ConvertFromLead"));
        Assert.NotNull(typeof(BookingDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.Booking.Domain.BookingSourceContext"));
        Assert.Null(typeof(BookingDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.Booking.Domain.AgencyBooking"));
        Assert.Null(typeof(BookingDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.Booking.Domain.DirectBooking"));
        Assert.Null(typeof(BookingDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.Booking.Domain.Commission"));

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
                x.line.Contains("TravelCore.Modules.AgencyMarketplace.Infrastructure", StringComparison.Ordinal)
                || x.line.Contains("TravelCore.Modules.AgencyMarketplace.Domain", StringComparison.Ordinal)
                || x.line.Contains("AgencyMarketplaceDbContext", StringComparison.Ordinal)
                || x.line.Contains("TourDbContext", StringComparison.Ordinal))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();
        Assert.True(
            infraHits.Count == 0,
            "Booking must not reach AgencyMarketplace/Tour persistence:\n" + string.Join('\n', infraHits));

        var creation = File.ReadAllText(Path.Combine(infraCs, "Services", "BookingCreationService.cs"));
        Assert.Contains("IAgencyOriginContextQuery", creation, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPost(\"/api/booking", creation, StringComparison.Ordinal);

        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P19-implementation-plan.md"));
        Assert.Contains("Booking != AgencyMarketplace", plan, StringComparison.Ordinal);
        Assert.Contains("BookingSourceKind != BookingStatus", plan, StringComparison.Ordinal);
        Assert.Contains("AgencyOffer != Booking", plan, StringComparison.Ordinal);
        Assert.Contains("AgencyOffer != Quote", plan, StringComparison.Ordinal);
        Assert.Contains("Agency context != Pricing Authority", plan, StringComparison.Ordinal);
        Assert.Contains("Lead != Booking", plan, StringComparison.Ordinal);
        Assert.Contains("VisaApplication != Booking", plan, StringComparison.Ordinal);
    }

    [Fact]
    public void Booking_T008_Public_Surface_Is_Pending_Initiation_Not_Confirm_Or_Payment()
    {
        Assert.True(BookingOwnershipBoundary.PublicBookingSurfaceImplemented);
        Assert.Equal("PublicExperience != Booking Source of Truth", PublicBookingCompositionBoundary.PublicExperienceIsNotBookingSourceOfTruth);
        Assert.Equal("Public Booking initiation != Booking confirmation", PublicBookingCompositionBoundary.PublicInitiationIsNotConfirmation);
        Assert.Equal("Pending != Confirmed", PublicBookingCompositionBoundary.PendingIsNotConfirmed);
        Assert.Equal("BookingId != Access Credential", PublicBookingCompositionBoundary.BookingIdIsNotAccessCredential);
        Assert.Equal("/api/booking/public", PublicBookingCompositionBoundary.PublicApiGroup);
        Assert.Equal("X-TravelCore-Booking-Access-Token", PublicBookingCompositionBoundary.AccessTokenHeader);
        Assert.False(PublicBookingCompositionBoundary.PublicCancellationImplemented);
        Assert.False(PublicBookingCompositionBoundary.PublicListingImplemented);
        Assert.False(PublicBookingCompositionBoundary.ConfirmEndpointImplemented);
        Assert.True(PublicBookingCompositionBoundary.PaymentEndpointImplemented);
        Assert.False(PublicBookingCompositionBoundary.AgencyOriginOnPublicInitiationImplemented);
        Assert.True(BookingOwnershipBoundary.PaymentIntegrationImplemented);
        Assert.True(BookingOrchestrationBoundary.PaymentDrivenConfirmationImplemented);
        Assert.Null(typeof(Booking).GetMethod("Confirm"));

        var endpoints = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Booking",
            "TravelCore.Modules.Booking.Infrastructure",
            "Endpoints",
            "PublicBookingEndpoints.cs"));
        Assert.Contains("MapGroup(PublicBookingCompositionBoundary.PublicApiGroup)", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapPost(\"/initiations\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapGet(\"/{bookingId:guid}\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapGet(\"/{bookingId:guid}/payment\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapPost(\"/{bookingId:guid}/payment/initiation\"", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPost(\"/confirm", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("PaymentIntent", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPost(\"/cancel", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPost(\"/refund", endpoints, StringComparison.Ordinal);

        var bookPage = Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "tours", "[slug]", "book", "page.tsx");
        Assert.True(File.Exists(bookPage), bookPage);
        var bookText = File.ReadAllText(bookPage);
        Assert.Contains("robots: { index: false, follow: false }", bookText, StringComparison.Ordinal);
        Assert.DoesNotContain("رزرو قطعی شد", bookText, StringComparison.Ordinal);
        Assert.DoesNotContain("Booking confirmed", bookText, StringComparison.Ordinal);
        Assert.DoesNotContain("Payment completed", bookText, StringComparison.Ordinal);

        var statusPage = Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "bookings", "[bookingId]", "page.tsx");
        Assert.True(File.Exists(statusPage), statusPage);
        var statusText = File.ReadAllText(statusPage);
        Assert.Contains("robots: { index: false, follow: false }", statusText, StringComparison.Ordinal);
        Assert.DoesNotContain("رزرو قطعی شد", statusText, StringComparison.Ordinal);
        Assert.DoesNotContain("Booking confirmed", statusText, StringComparison.Ordinal);

        var paymentPage = Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "bookings", "[bookingId]", "payment", "page.tsx");
        var returnPage = Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "bookings", "[bookingId]", "payment", "return", "page.tsx");
        Assert.True(File.Exists(paymentPage), paymentPage);
        Assert.True(File.Exists(returnPage), returnPage);
        var paymentText = File.ReadAllText(paymentPage);
        var returnText = File.ReadAllText(returnPage);
        Assert.Contains("robots: { index: false, follow: false }", paymentText, StringComparison.Ordinal);
        Assert.Contains("robots: { index: false, follow: false }", returnText, StringComparison.Ordinal);
        Assert.Contains("Browser return is a sibling route and also does not mark Payment successful", paymentText, StringComparison.Ordinal);
        Assert.Contains("BrowserReturn != PaymentSuccess", returnText, StringComparison.Ordinal);
        Assert.DoesNotContain("accessToken", paymentText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cardNumber", paymentText + returnText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cvv", paymentText + returnText, StringComparison.OrdinalIgnoreCase);

        Assert.False(File.Exists(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "bookings", "page.tsx")));
        Assert.False(Directory.Exists(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "checkout")));

        var bookingFeature = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "booking");
        Assert.True(Directory.Exists(bookingFeature), bookingFeature);
        var featureText = string.Join(
            '\n',
            Directory.EnumerateFiles(bookingFeature, "*.ts", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(bookingFeature, "*.tsx", SearchOption.AllDirectories))
                .Select(File.ReadAllText));
        Assert.Contains("/api/booking/public", featureText, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", featureText, StringComparison.Ordinal);
        Assert.DoesNotContain("رزرو قطعی شد", featureText, StringComparison.Ordinal);
        Assert.DoesNotContain("Payment completed", featureText, StringComparison.Ordinal);

        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P19-implementation-plan.md"));
        Assert.Contains("PublicExperience != Booking Source of Truth", plan, StringComparison.Ordinal);
        Assert.Contains("Public Booking initiation != Booking confirmation", plan, StringComparison.Ordinal);
        Assert.Contains("Pending != Confirmed", plan, StringComparison.Ordinal);
        Assert.Contains("BookingId != Access Credential", plan, StringComparison.Ordinal);
        Assert.Contains("P19-R8", plan, StringComparison.Ordinal);
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
