using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P20-T001: Payment owns schema payment; initial target Booking; no aggregate/lifecycle/provider yet.
/// </summary>
public sealed class PaymentBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void PaymentProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Payment.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Payment.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Payment.Infrastructure");
        Assert.Equal("payment", TravelCore.Modules.Payment.Infrastructure.PaymentDbContext.SchemaName);
        Assert.Equal("payment", PaymentOwnershipBoundary.SchemaName);
        Assert.Equal("Booking", PaymentOwnershipBoundary.InitialTarget);
        Assert.Equal("Tour Booking", PaymentOwnershipBoundary.InitialScope);
    }

    [Fact]
    public void Payment_DoesNot_Own_Peer_SoT_Or_Product_Types()
    {
        Assert.Equal("Payment", PaymentOwnershipBoundary.OwnerModule);
        Assert.Equal("Payment != Booking", PaymentOwnershipBoundary.PaymentIsNotBooking);
        Assert.Equal("Payment != Pricing", PaymentOwnershipBoundary.PaymentIsNotPricing);
        Assert.Equal("Payment != Quote", PaymentOwnershipBoundary.PaymentIsNotQuote);
        Assert.Equal("Payment != BookingMonetarySnapshot", PaymentOwnershipBoundary.PaymentIsNotBookingMonetarySnapshot);
        Assert.Equal("Payment != Bank Settlement", PaymentOwnershipBoundary.PaymentIsNotBankSettlement);
        Assert.Equal("Payment != Accounting Ledger", PaymentOwnershipBoundary.PaymentIsNotAccountingLedger);
        Assert.Equal("Payment != Agency Settlement", PaymentOwnershipBoundary.PaymentIsNotAgencySettlement);
        Assert.Equal("PaymentStatus != BookingStatus", PaymentOwnershipBoundary.PaymentStatusIsNotBookingStatus);
        Assert.Equal("PaymentSucceeded != BookingConfirmed", PaymentOwnershipBoundary.PaymentSucceededIsNotBookingConfirmed);
        Assert.Equal("BookingCancelled != PaymentRefunded", PaymentOwnershipBoundary.BookingCancelledIsNotPaymentRefunded);
        Assert.False(PaymentOwnershipBoundary.OwnsBooking);
        Assert.False(PaymentOwnershipBoundary.OwnsPricing);
        Assert.False(PaymentOwnershipBoundary.OwnsQuote);
        Assert.False(PaymentOwnershipBoundary.OwnsBookingMonetarySnapshot);
        Assert.True(PaymentOwnershipBoundary.PaymentAggregateImplemented);
        Assert.True(PaymentOwnershipBoundary.PaymentStatusImplemented);
        Assert.True(PaymentOwnershipBoundary.PaymentAttemptImplemented);
        Assert.True(PaymentOwnershipBoundary.RefundImplemented);
        Assert.False(PaymentOwnershipBoundary.ProviderAdapterImplemented);
        Assert.False(PaymentOwnershipBoundary.ProviderSdkImplemented);
        Assert.True(PaymentOwnershipBoundary.ProviderPortImplemented);
        Assert.True(PaymentOwnershipBoundary.CallbackEndpointImplemented);
        Assert.False(PaymentOwnershipBoundary.PaymentApiImplemented);
        Assert.False(PaymentOwnershipBoundary.PaymentUiImplemented);
        Assert.False(PaymentOwnershipBoundary.BookingConfirmImplemented);
        Assert.False(PaymentOwnershipBoundary.SharedDbContextImplemented);
        Assert.False(PaymentOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(PaymentOwnershipBoundary.GeneralizedTargetTypeImplemented);
        Assert.NotNull(typeof(PaymentDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Payment.Domain.Payment"));
        Assert.NotNull(typeof(PaymentDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Payment.Domain.PaymentStatus"));
        Assert.NotNull(typeof(PaymentDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Payment.Domain.PaymentAttempt"));
        Assert.NotNull(typeof(PaymentDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Payment.Domain.Refund"));
    }

    [Fact]
    public void PaymentInfrastructure_MustNotProjectReference_PeerBusinessModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.Payment.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                IsForbiddenPeerModule(name)
                && !string.Equals(name, "TravelCore.Modules.Booking.Contracts", StringComparison.Ordinal))
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Payment.Infrastructure must not project-reference peer business modules:\n" + string.Join('\n', hits));
        Assert.DoesNotContain(
            infra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name is "TravelCore.Modules.Booking.Infrastructure"
                or "TravelCore.Modules.Booking.Domain"
                or "TravelCore.Modules.Pricing.Infrastructure"
                or "TravelCore.Modules.Pricing.Domain"
                or "TravelCore.Modules.AgencyMarketplace.Infrastructure");
    }

    [Fact]
    public void PaymentDomain_MustNotProjectReference_PeerBusinessModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.Payment.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)
                || IsForbiddenPeerModule(name))
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Payment.Domain must stay free of peer modules:\n" + string.Join('\n', hits));
        Assert.Contains(
            domain.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Identifiers");
        Assert.Contains(
            domain.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Money");
    }

    [Fact]
    public void PaymentContracts_MustNotProjectReference_PeerBusinessModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.Payment.Contracts");
        var hits = contracts.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Payment.Contracts must not project-reference peer business modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Payment_T001_MustNotImplement_Deferred_Product_Types()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment");
        Assert.True(Directory.Exists(root), root);

        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(PaymentIntent|Stripe|Zarinpal|IDPay|PayPal|Adyen|Checkout|Wallet|Settlement|JournalEntry|Ledger|Commission|Payout|StripeStatus|ZarinpalStatus|GatewayStatus)\b",
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
            "T002 forbids refund/provider product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Payment_Module_Keeps_Provider_Callback_Api_And_Ui_Out()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment");
        Assert.True(Directory.Exists(root), root);

        var forbidden = new[]
        {
            "Stripe",
            "Zarinpal",
            "IDPay",
            "PayPal",
            "Adyen",
            "MapGet(\"/api/payment\"",
            "MapPost(\"/api/payment\"",
            "MapPut(\"/api/payment\"",
            "webhook",
            "Webhook",
            "isPaid",
            "paymentSucceeded",
            "providerSuccess",
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

        Assert.True(
            hits.Count == 0,
            "Payment T003 must not introduce named providers, public Payment API/UI, or callback-as-webhook tokens:\n" + string.Join('\n', hits));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "checkout")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "pay")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "payment")));
    }

    [Fact]
    public void Payment_Csproj_MustNotReference_Provider_Sdks()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment");
        var forbiddenPackages = new[]
        {
            "Stripe",
            "Zarinpal",
            "IDPay",
            "PayPal",
            "Adyen",
            "Braintree",
        };

        var hits = new List<string>();
        foreach (var path in Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(path);
            foreach (var token in forbiddenPackages)
            {
                if (text.Contains(token, StringComparison.OrdinalIgnoreCase))
                {
                    hits.Add($"{Path.GetRelativePath(RepoRoot, path)}:{token}");
                }
            }
        }

        Assert.True(hits.Count == 0, "Payment must not package-reference provider SDKs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Payment_Evidence_Keeps_Ascii_Invariants()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P20-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);
        var text = File.ReadAllText(plan);
        Assert.Contains("P20-R1 = RESOLVED", text, StringComparison.Ordinal);
        Assert.Contains("P20-R2 = RESOLVED", text, StringComparison.Ordinal);
        Assert.Contains("P20-R3 = RESOLVED", text, StringComparison.Ordinal);
        Assert.Contains("P20-R4 = RESOLVED", text, StringComparison.Ordinal);
        Assert.Contains("schema `payment`", text, StringComparison.Ordinal);
        Assert.Contains("initial Payment target = Booking", text, StringComparison.Ordinal);
        Assert.Contains("Payment != Booking", text, StringComparison.Ordinal);
        Assert.Contains("Payment != Pricing", text, StringComparison.Ordinal);
        Assert.Contains("Payment != PaymentAttempt", text, StringComparison.Ordinal);
        Assert.Contains("PaymentStatus != BookingStatus", text, StringComparison.Ordinal);
        Assert.Contains("PaymentStatus != PaymentAttemptStatus", text, StringComparison.Ordinal);
        Assert.Contains("Failed PaymentAttempt != Failed Payment", text, StringComparison.Ordinal);
        Assert.Contains("PaymentSucceeded != BookingConfirmed", text, StringComparison.Ordinal);
        Assert.Contains("BrowserReturn != PaymentSuccess", text, StringComparison.Ordinal);
        Assert.Contains("UnverifiedCallback != PaymentSuccess", text, StringComparison.Ordinal);
        Assert.Contains("P20-R5", text, StringComparison.Ordinal);
        Assert.Contains("P20-R8", text, StringComparison.Ordinal);
        Assert.DoesNotContain("P20 COMPLETE", text, StringComparison.Ordinal);
        Assert.DoesNotContain("TC-P20-GATE COMPLETE", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Payment_Host_Registers_Module_Without_Endpoints()
    {
        var program = File.ReadAllText(Path.Combine(RepoRoot, "src", "backend", "TravelCore.Api", "Program.cs"));
        Assert.Contains("new PaymentModule()", program, StringComparison.Ordinal);
        var module = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Infrastructure",
            "PaymentModule.cs"));
        Assert.Contains("AddDbContext<PaymentDbContext>", module, StringComparison.Ordinal);
        Assert.DoesNotContain("MapGet", module, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPost", module, StringComparison.Ordinal);
    }

    [Fact]
    public void Payment_T002_Lifecycle_Is_Pending_Succeeded_With_Distinct_Attempts()
    {
        Assert.Equal(
            new[] { PaymentStatus.Pending, PaymentStatus.Succeeded },
            Enum.GetValues<PaymentStatus>());
        Assert.Equal(
            new[]
            {
                PaymentAttemptStatus.Created,
                PaymentAttemptStatus.Initiated,
                PaymentAttemptStatus.Succeeded,
                PaymentAttemptStatus.Failed,
            },
            Enum.GetValues<PaymentAttemptStatus>());
        Assert.Equal("Payment != PaymentAttempt", PaymentLifecycleBoundary.PaymentIsNotPaymentAttempt);
        Assert.Equal("PaymentStatus != PaymentAttemptStatus", PaymentLifecycleBoundary.PaymentStatusIsNotPaymentAttemptStatus);
        Assert.Equal("Failed PaymentAttempt != Failed Payment", PaymentLifecycleBoundary.FailedAttemptIsNotFailedPayment);
        Assert.Equal("PaymentSucceeded != BookingConfirmed", PaymentLifecycleBoundary.PaymentSucceededIsNotBookingConfirmed);
        Assert.False(PaymentLifecycleBoundary.PaymentFailedStatusImplemented);
        Assert.False(PaymentLifecycleBoundary.PaymentRefundedStatusImplemented);
        Assert.False(PaymentLifecycleBoundary.PaymentCancelledStatusImplemented);
        Assert.False(PaymentLifecycleBoundary.PaymentExpiredStatusImplemented);
        Assert.False(PaymentLifecycleBoundary.CallerControlledSuccessImplemented);
        Assert.False(PaymentLifecycleBoundary.PublicSuccessEndpointImplemented);
        Assert.True(PaymentLifecycleBoundary.ProviderPortImplemented);
        Assert.False(PaymentLifecycleBoundary.ProviderAdapterImplemented);
        Assert.DoesNotContain("Failed", Enum.GetNames<PaymentStatus>());
        Assert.DoesNotContain("Refunded", Enum.GetNames<PaymentStatus>());
        Assert.DoesNotContain("Cancelled", Enum.GetNames<PaymentStatus>());
        Assert.DoesNotContain("Expired", Enum.GetNames<PaymentStatus>());
        var paymentType = typeof(TravelCore.Modules.Payment.Domain.Payment);
        var methodNames = paymentType.GetMethods(
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(m => m.Name)
            .ToArray();
        Assert.DoesNotContain("SetStatus", methodNames);
        Assert.DoesNotContain("MarkSucceeded", methodNames);
        Assert.Contains("RecordAuthoritativeCollectionSuccess", methodNames);
        Assert.Contains("CreateAttempt", methodNames);
        Assert.NotNull(typeof(PaymentDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Payment.Domain.Refund"));
    }

    [Fact]
    public void Payment_T003_ProviderNeutral_Trust_Boundary()
    {
        Assert.Equal("BrowserReturn != PaymentSuccess", PaymentProviderTrustBoundary.BrowserReturnIsNotPaymentSuccess);
        Assert.Equal("UnverifiedCallback != PaymentSuccess", PaymentProviderTrustBoundary.UnverifiedCallbackIsNotPaymentSuccess);
        Assert.Equal("ClientSuccessFlag != PaymentSuccess", PaymentProviderTrustBoundary.ClientSuccessFlagIsNotPaymentSuccess);
        Assert.Equal("ProviderRedirect != PaymentSuccess", PaymentProviderTrustBoundary.ProviderRedirectIsNotPaymentSuccess);
        Assert.Equal("ProviderReference != PaymentId", PaymentProviderTrustBoundary.ProviderReferenceIsNotPaymentId);
        Assert.Equal("ProviderReference != PaymentAttemptId", PaymentProviderTrustBoundary.ProviderReferenceIsNotPaymentAttemptId);
        Assert.Equal("NONE", PaymentProviderTrustBoundary.NamedProviderSelected);
        Assert.Equal("ExecutionSnapshotMatchRequired", PaymentProviderTrustBoundary.AmountMismatchEnforcement);
        Assert.True(PaymentProviderTrustBoundary.ProviderPortImplemented);
        Assert.False(PaymentProviderTrustBoundary.NamedProductionAdapterImplemented);
        Assert.False(PaymentProviderTrustBoundary.ProductionFakeProviderRegistered);
        Assert.True(PaymentProviderTrustBoundary.AmountMismatchEnforcementImplemented);
        Assert.True(typeof(IPaymentProviderGateway).IsInterface);
        Assert.True(typeof(IPaymentProviderResolver).IsInterface);
        Assert.Contains("InitiateRefundAsync", typeof(IPaymentProviderGateway).GetMethods().Select(m => m.Name));
        Assert.Contains("VerifyRefundAsync", typeof(IPaymentProviderGateway).GetMethods().Select(m => m.Name));
        Assert.Contains("QueryRefundStatusAsync", typeof(IPaymentProviderGateway).GetMethods().Select(m => m.Name));
        Assert.Contains("VerifyCallbackAsync", typeof(IPaymentProviderGateway).GetMethods().Select(m => m.Name));
        Assert.Null(typeof(PaymentDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Payment.Domain.GatewayStatus"));
        Assert.DoesNotContain(
            typeof(PaymentDomainAssemblyMarker).Assembly.GetTypes().Select(t => t.Name),
            name => name is "StripeStatus");
        var module = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Infrastructure",
            "PaymentModule.cs"));
        Assert.DoesNotContain("AddSingleton<IPaymentProviderGateway", module, StringComparison.Ordinal);
        Assert.Contains("/api/payment/providers", File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Infrastructure",
            "Endpoints",
            "PaymentProviderCallbackEndpoints.cs")), StringComparison.Ordinal);
    }

    [Fact]
    public void Payment_T004_Idempotency_Is_Database_Backed()
    {
        Assert.Equal("Booking 1 -> 1 logical Payment", PaymentIdempotencyBoundary.OneBookingOneLogicalPayment);
        Assert.Equal("Retry = PaymentAttempt, not new Payment", PaymentIdempotencyBoundary.RetryIsAttemptNotPayment);
        Assert.Equal("NOT ASSUMED", PaymentIdempotencyBoundary.ExactlyOnceExternalPayment);
        Assert.Equal(
            "Unknown/Ambiguous provider outcome != PaymentAttempt.Failed",
            PaymentIdempotencyBoundary.AmbiguousIsNotFailedAttempt);
        Assert.Equal("Reconciliation != Settlement", PaymentIdempotencyBoundary.ReconciliationIsNotSettlement);
        Assert.Equal("Reconciliation != Accounting", PaymentIdempotencyBoundary.ReconciliationIsNotAccounting);
        Assert.True(PaymentIdempotencyBoundary.UniqueLogicalPaymentPerBookingImplemented);
        Assert.False(PaymentIdempotencyBoundary.ProcessLocalIdempotencyAuthorityImplemented);
        Assert.False(PaymentIdempotencyBoundary.AutomaticRetryOnAmbiguityImplemented);
        Assert.False(PaymentIdempotencyBoundary.AutomaticProviderFailoverImplemented);
        Assert.False(PaymentIdempotencyBoundary.ReconciliationSchedulerImplemented);
        var infraRoot = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Infrastructure");
        var forbidden = new[] { "ConcurrentDictionary", "SemaphoreSlim", "static readonly object" };
        var hits = Directory.EnumerateFiles(infraRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => forbidden.Any(token => x.line.Contains(token, StringComparison.Ordinal))))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();
        Assert.True(hits.Count == 0, "Payment idempotency must not use process-local authority:\n" + string.Join('\n', hits));
        var paymentConfiguration = File.ReadAllText(Path.Combine(
            infraRoot,
            "Persistence",
            "PaymentConfiguration.cs"));
        Assert.Contains("ux_payments_booking_id", paymentConfiguration, StringComparison.Ordinal);
        Assert.Contains("IsConcurrencyToken", paymentConfiguration, StringComparison.Ordinal);
        Assert.Contains("version", paymentConfiguration, StringComparison.Ordinal);
    }

    [Fact]
    public void Payment_T005_Durability_Uses_Module_Local_Outbox()
    {
        Assert.True(PaymentSuccessOutboxBoundary.TransactionalOutboxImplemented);
        Assert.False(PaymentSuccessOutboxBoundary.EventMeansBookingConfirmed);
        Assert.Equal("at-least-once", PaymentSuccessOutboxBoundary.DeliverySemantics);
        Assert.Equal("idempotent/effectively-once", PaymentSuccessOutboxBoundary.LocalEffectSemantics);
        Assert.Equal("PaymentSucceeded != BookingConfirmed", PaymentOwnershipBoundary.PaymentSucceededIsNotBookingConfirmed);
        Assert.False(PaymentOwnershipBoundary.SharedDbContextImplemented);
        Assert.False(PaymentOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(PaymentOwnershipBoundary.BookingConfirmImplemented);
        var module = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Infrastructure",
            "PaymentModule.cs"));
        Assert.Contains("PaymentSuccessOutboxDispatcher", module, StringComparison.Ordinal);
        Assert.Contains("RefundSucceededOutboxDispatcher", module, StringComparison.Ordinal);
        Assert.Contains("IBookingPaymentCompensationRequiredHandler", module, StringComparison.Ordinal);
        Assert.DoesNotContain(
            Projects.Single(p => p.Name == "TravelCore.Modules.Payment.Infrastructure")
                .ProjectReferences
                .Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Modules.Booking.Infrastructure");
        var bookingModule = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Booking",
            "TravelCore.Modules.Booking.Infrastructure",
            "BookingModule.cs"));
        Assert.Contains("IPaymentSucceededIntegrationHandler", bookingModule, StringComparison.Ordinal);
        Assert.Contains("IRefundSucceededIntegrationHandler", bookingModule, StringComparison.Ordinal);
        Assert.Contains("BookingCompensationOutboxDispatcher", bookingModule, StringComparison.Ordinal);
        Assert.True(PaymentRefundBoundary.RefundAggregateImplemented);
        Assert.False(PaymentRefundBoundary.PublicRefundApiImplemented);
        Assert.False(PaymentRefundBoundary.PartialRefundImplemented);
        Assert.False(RefundSuccessOutboxBoundary.EventMeansBookingCancelled);
        Assert.Equal("Payment != Refund", PaymentRefundBoundary.PaymentIsNotRefund);
        Assert.Equal("PaymentSucceeded != RefundSucceeded", PaymentRefundBoundary.PaymentSucceededIsNotRefundSucceeded);
        Assert.Equal("RefundSucceeded != BookingCancelled", PaymentRefundBoundary.RefundSucceededIsNotBookingCancelled);
    }

    private static bool IsForbiddenPeerModule(string name) =>
        name.Contains(".Booking.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Booking", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Tour.", StringComparison.OrdinalIgnoreCase)
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
        || name.Contains(".PublicExperience.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".PublicExperience", StringComparison.OrdinalIgnoreCase);

    private static bool IsGeneratedOrBin(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
