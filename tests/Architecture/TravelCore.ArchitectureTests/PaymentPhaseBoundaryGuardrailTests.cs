using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P20-T009: Phase-boundary evidence for accepted P20-R1 through P20-R8.
/// Hardening only — no new Payment capability and no GATE close.
/// </summary>
public sealed class PaymentPhaseBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void P20_EvidencePack_Exists_And_DoesNotClose_Gate()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P20-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);

        string[] required =
        [
            "TC-P20-PLAN ACCEPTED",
            "TC-P20-T001 ACCEPTED",
            "TC-P20-T002 ACCEPTED",
            "TC-P20-T003 ACCEPTED",
            "TC-P20-T004 ACCEPTED",
            "TC-P20-T005 ACCEPTED",
            "TC-P20-T006 ACCEPTED",
            "TC-P20-T007 ACCEPTED",
            "TC-P20-T008 ACCEPTED",
            "TC-P20-T009",
            "TC-P20-GATE NOT EXECUTED",
            "P20-R1",
            "P20-R8",
            "Payment != Booking",
            "Payment != Pricing",
            "Payment != Quote",
            "Payment != BookingMonetarySnapshot",
            "Payment != PaymentAttempt",
            "Payment != Refund",
            "PaymentStatus != BookingStatus",
            "PaymentStatus != PaymentAttemptStatus",
            "Failed PaymentAttempt != Failed Payment",
            "PaymentSucceeded != BookingConfirmed",
            "BrowserReturn != PaymentSuccess",
            "UnverifiedCallback != PaymentSuccess",
            "ClientSuccessFlag != PaymentSuccess",
            "ProviderRedirect != PaymentSuccess",
            "BookingCancelled != PaymentRefunded",
            "RefundSucceeded != BookingCancelled",
            "OperationalRead != FinancialTruthAuthority",
            "Production Provider: NONE / NOT CONFIGURED",
            "Real Provider SDK = NO",
            "P20 READY FOR GATE: YES",
            "P20 remains IN PROGRESS",
            "no new product capability",
            "Confirmed Booking cancellation remains DEFERRED",
            "Consumed capacity reversal remains DEFERRED",
            "Partial Refund remains DEFERRED",
        ];

        foreach (var invariant in required)
        {
            Assert.Contains(invariant, text, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("TC-P20-GATE COMPLETE", text, StringComparison.Ordinal);
        Assert.DoesNotContain("P20 COMPLETE", text, StringComparison.Ordinal);
    }

    [Fact]
    public void P20_GateEvidence_Exists_And_Closes_Phase()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P20-GATE-acceptance-evidence.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("TC-P20-T001", text, StringComparison.Ordinal);
        Assert.Contains("TC-P20-T009", text, StringComparison.Ordinal);
        Assert.Contains("P20-R1", text, StringComparison.Ordinal);
        Assert.Contains("P20-R8", text, StringComparison.Ordinal);
        Assert.Contains("Payment != Booking", text, StringComparison.Ordinal);
        Assert.Contains("PaymentSucceeded != BookingConfirmed", text, StringComparison.Ordinal);
        Assert.Contains("BrowserReturn != PaymentSuccess", text, StringComparison.Ordinal);
        Assert.Contains("Production Provider: NONE / NOT CONFIGURED", text, StringComparison.Ordinal);
        Assert.Contains("TC-P20-GATE COMPLETE", text, StringComparison.Ordinal);
        Assert.Contains("P20 COMPLETE", text, StringComparison.Ordinal);
        Assert.Contains("no new Payment capability", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("P21 — Hotel Booking (PLANNED)", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Payment_Keeps_All_P20_Boundaries_Resolved()
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
        Assert.Equal(
            new[] { RefundStatus.Pending, RefundStatus.Succeeded },
            Enum.GetValues<RefundStatus>());
        Assert.Equal(
            new[]
            {
                RefundAttemptStatus.Created,
                RefundAttemptStatus.Initiated,
                RefundAttemptStatus.Succeeded,
                RefundAttemptStatus.Failed,
            },
            Enum.GetValues<RefundAttemptStatus>());
        Assert.Equal(
            new[] { BookingStatus.Pending, BookingStatus.Confirmed, BookingStatus.Cancelled },
            Enum.GetValues<BookingStatus>());
        Assert.Equal(
            new[]
            {
                CapacityHoldStatus.Active,
                CapacityHoldStatus.Consumed,
                CapacityHoldStatus.Released,
                CapacityHoldStatus.Expired,
            },
            Enum.GetValues<CapacityHoldStatus>());
        Assert.Equal(
            new[]
            {
                "RedirectInitiation",
                "CallbackVerification",
                "PaymentStatusQuery",
                "RefundInitiation",
                "RefundVerification",
                "RefundStatusQuery",
            },
            PaymentProviderCapabilitySet.ExactValues);
        Assert.Equal("payment", PaymentOwnershipBoundary.SchemaName);
        Assert.Equal("Booking", PaymentOwnershipBoundary.InitialTarget);
        Assert.False(PaymentOwnershipBoundary.SharedDbContextImplemented);
        Assert.False(PaymentOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(PaymentOwnershipBoundary.ProviderSdkImplemented);
        Assert.False(PaymentProviderTrustBoundary.NamedProductionAdapterImplemented);
        Assert.Equal("NONE", PaymentProviderTrustBoundary.NamedProviderSelected);
        Assert.Equal("NOT CONFIGURED / NONE", PaymentProviderTrustBoundary.ProductionProviderPosture);
        Assert.False(PaymentOperationalBoundary.PublicOperationalEndpointImplemented);
        Assert.False(PaymentOperationalBoundary.ManualPaymentMutationImplemented);
        Assert.False(PaymentOperationalBoundary.ManualRefundMutationImplemented);
        Assert.False(PaymentRefundBoundary.PartialRefundImplemented);
        Assert.False(PaymentRefundBoundary.PublicRefundApiImplemented);
        Assert.False(PublicPaymentCompositionBoundary.CardCollectionImplemented);
        Assert.Equal("at-least-once", PaymentSuccessOutboxBoundary.DeliverySemantics);
        Assert.Equal("idempotent/effectively-once", PaymentSuccessOutboxBoundary.LocalEffectSemantics);
        Assert.False(PaymentIdempotencyBoundary.AutomaticProviderFailoverImplemented);
        Assert.False(PaymentIdempotencyBoundary.ProcessLocalIdempotencyAuthorityImplemented);
        Assert.Null(typeof(TravelCore.Modules.Booking.Domain.Booking).GetMethod("Confirm"));
        Assert.Null(typeof(TravelCore.Modules.Booking.Domain.Booking).GetMethod("SetConfirmed"));
        Assert.Equal(
            "/api/booking/public/{bookingId}/payment",
            PublicPaymentCompositionBoundary.StatusRoute);
        Assert.Equal(
            "/api/booking/public/{bookingId}/payment/initiation",
            PublicPaymentCompositionBoundary.InitiationRoute);
    }

    [Fact]
    public void Payment_And_Booking_Infrastructure_Remain_Peer_Isolated()
    {
        var paymentInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Payment.Infrastructure");
        var bookingInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Booking.Infrastructure");
        Assert.DoesNotContain(
            paymentInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name is "TravelCore.Modules.Booking.Infrastructure"
                or "TravelCore.Modules.Booking.Domain"
                or "TravelCore.Modules.HotelBooking.Infrastructure"
                or "TravelCore.Modules.HotelBooking.Domain"
                or "TravelCore.Modules.Pricing.Infrastructure");
        Assert.DoesNotContain(
            bookingInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name is "TravelCore.Modules.Payment.Infrastructure"
                or "TravelCore.Modules.Payment.Domain"
                or "TravelCore.Modules.Pricing.Infrastructure");
        Assert.Contains(
            paymentInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Modules.Booking.Contracts");
        Assert.Contains(
            paymentInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Modules.HotelBooking.Contracts");
        Assert.Contains(
            bookingInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Modules.Payment.Contracts");
    }

    [Fact]
    public void Payment_Public_And_Frontend_Surfaces_Stay_Private_Honest_And_Cardless()
    {
        var paymentPage = Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "bookings", "[bookingId]", "payment", "page.tsx");
        var returnPage = Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "bookings", "[bookingId]", "payment", "return", "page.tsx");
        var view = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "booking", "payment-view.tsx");
        var copy = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "booking", "copy.ts");
        Assert.True(File.Exists(paymentPage));
        Assert.True(File.Exists(returnPage));
        var paymentText = File.ReadAllText(paymentPage);
        var returnText = File.ReadAllText(returnPage);
        var viewText = File.ReadAllText(view);
        var copyText = File.ReadAllText(copy);
        Assert.Contains("robots: { index: false, follow: false }", paymentText, StringComparison.Ordinal);
        Assert.Contains("robots: { index: false, follow: false }", returnText, StringComparison.Ordinal);
        Assert.Contains("sessionStorage", viewText, StringComparison.Ordinal);
        Assert.DoesNotContain("localStorage", viewText, StringComparison.Ordinal);
        Assert.DoesNotContain("searchParams", viewText, StringComparison.Ordinal);
        Assert.Contains("LtrValue", viewText, StringComparison.Ordinal);
        Assert.Contains("MoneyText", viewText, StringComparison.Ordinal);
        Assert.Contains("min-h-11", viewText, StringComparison.Ordinal);
        Assert.Contains("focus-visible:outline", viewText, StringComparison.Ordinal);
        Assert.Contains("payTitle: \"پرداخت رزرو\"", copyText, StringComparison.Ordinal);
        Assert.Contains("payTitle: \"Booking payment\"", copyText, StringComparison.Ordinal);
        Assert.Contains("payTitle: \"دفع الحجز\"", copyText, StringComparison.Ordinal);
        Assert.DoesNotContain("Payment completed", copyText, StringComparison.Ordinal);
        Assert.DoesNotContain("Booking confirmed", copyText, StringComparison.Ordinal);

        var frontendRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src");
        var cardHits = Directory.EnumerateFiles(frontendRoot, "*.ts*", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(path => File.ReadAllLines(path).Select((line, i) => (path, line, i)))
            .Where(x =>
                Regex.IsMatch(x.line, @"\b(PAN|CVV|CVC|cardNumber|card_number|creditCard)\b", RegexOptions.IgnoreCase)
                || x.line.Contains("autocomplete=\"cc-", StringComparison.Ordinal))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();
        Assert.True(cardHits.Count == 0, "Frontend must not collect card data:\n" + string.Join('\n', cardHits));
    }

    [Fact]
    public void Payment_Does_Not_Share_Schema_Sql_Secrets_Or_Search_Projection()
    {
        var paymentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment");
        var bookingRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking");
        var searchRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Search");
        Assert.True(Directory.Exists(paymentRoot));
        Assert.True(Directory.Exists(bookingRoot));

        var paymentSql = ScanCs(paymentRoot, line =>
            line.Contains("schema: \"booking\"", StringComparison.Ordinal)
            || line.Contains("FROM booking.", StringComparison.OrdinalIgnoreCase)
            || line.Contains("JOIN booking.", StringComparison.OrdinalIgnoreCase));
        var bookingSql = ScanCs(bookingRoot, line =>
            line.Contains("schema: \"payment\"", StringComparison.Ordinal)
            || line.Contains("FROM payment.", StringComparison.OrdinalIgnoreCase)
            || line.Contains("JOIN payment.", StringComparison.OrdinalIgnoreCase));
        Assert.True(paymentSql.Count == 0, "Payment must not query booking schema:\n" + string.Join('\n', paymentSql));
        Assert.True(bookingSql.Count == 0, "Booking must not query payment schema:\n" + string.Join('\n', bookingSql));

        var secretHits = ScanCs(paymentRoot, line =>
            Regex.IsMatch(line, @"sk_live_|sk_test_|merchant[_-]?key|callback[_-]?secret|api[_-]?secret", RegexOptions.IgnoreCase)
            && !line.TrimStart().StartsWith("//", StringComparison.Ordinal)
            && !line.TrimStart().StartsWith("///", StringComparison.Ordinal));
        Assert.True(secretHits.Count == 0, "Payment must not commit provider secrets:\n" + string.Join('\n', secretHits));

        if (Directory.Exists(searchRoot))
        {
            var searchHits = ScanCs(searchRoot, line =>
                line.Contains("PaymentSucceededIntegrationEvent", StringComparison.Ordinal)
                || line.Contains("RefundSucceededIntegrationEvent", StringComparison.Ordinal)
                || line.Contains("PaymentAttempt", StringComparison.Ordinal)
                || line.Contains("X-TravelCore-Booking-Access-Token", StringComparison.Ordinal));
            Assert.True(searchHits.Count == 0, "Search must not project Payment transactional data:\n" + string.Join('\n', searchHits));
        }

        var checklist = Path.Combine(RepoRoot, "docs", "plans", "P20-provider-adapter-checklist.md");
        Assert.True(File.Exists(checklist), checklist);
        var checklistText = File.ReadAllText(checklist);
        Assert.Contains("credentials/secrets", checklistText, StringComparison.Ordinal);
        Assert.Contains("callback verification", checklistText, StringComparison.Ordinal);
        Assert.Contains("amount units", checklistText, StringComparison.Ordinal);
        Assert.Contains("sandbox vs production", checklistText, StringComparison.Ordinal);
    }

    private static List<string> ScanCs(string root, Func<string, bool> predicate) =>
        Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                && !p.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path).Select((line, i) => (path, line, i)))
            .Where(x => predicate(x.line))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();
}
