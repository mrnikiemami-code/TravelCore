using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class HotelBookingCancellationGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void Cancellation_Process_Is_Not_HotelBookingStatus_And_Statuses_Stay_Minimal()
    {
        Assert.Equal(
            HotelBookingCancellationOwnershipBoundary.ProcessIsNotBookingStatus,
            "HotelBookingCancellation != HotelBookingStatus");
        Assert.NotEqual(typeof(HotelBookingCancellation), typeof(HotelBookingStatus));
        Assert.Equal(
            new[] { "Requested", "SupplierCancellationPending", "RefundPending", "Completed" },
            Enum.GetNames<HotelBookingCancellationStatus>());
        Assert.Equal(
            new[] { "Created", "Initiated", "Confirmed", "Failed" },
            Enum.GetNames<HotelSupplierCancellationAttemptStatus>());
        Assert.Equal(
            new[] { "Pending", "Confirmed", "Cancelled" },
            Enum.GetNames<HotelBookingStatus>());
        Assert.Equal(
            new[] { "Pending", "Confirmed", "Cancelled" },
            Enum.GetNames<HotelSupplierReservationStatus>());
        Assert.Equal(
            new[] { "Pending", "Succeeded" },
            Enum.GetNames<PaymentStatus>());
        Assert.Equal(
            new[] { "Pending", "Succeeded" },
            Enum.GetNames<RefundStatus>());
        Assert.Null(typeof(HotelBooking).GetMethod("Cancel"));
        Assert.Null(typeof(HotelBooking).GetMethod("SetCancelled"));
        Assert.Null(typeof(HotelBooking).GetMethod("ForceCancel"));
        Assert.NotNull(typeof(HotelBooking).GetMethod(nameof(HotelBooking.CancelFromAuthoritativeSupplierCancellation)));
        Assert.NotNull(typeof(HotelBooking).GetMethod(nameof(HotelBooking.CancelFromAuthoritativePaymentCompensation)));
        Assert.True(HotelBookingOwnershipBoundary.CancellationModelImplemented);
        Assert.False(HotelBookingCancellationOwnershipBoundary.GenericCancelSurfaceImplemented);
        Assert.False(HotelBookingCancellationOwnershipBoundary.PendingCustomerCancellationImplemented);
        Assert.False(HotelBookingCancellationOwnershipBoundary.PartialRefundImplemented);
        Assert.False(PaymentRefundBoundary.PartialRefundImplemented);
        Assert.Equal("DEFERRED", HotelBookingCancellationOwnershipBoundary.PartialRefund);
        Assert.Equal("DEFERRED", HotelBookingCancellationOwnershipBoundary.Amendments);
        Assert.False(HotelBookingCancellationOwnershipBoundary.AmendmentsImplemented);
        Assert.True(HotelBookingCancellationOwnershipBoundary.PublicCancellationApiImplemented);
        Assert.True(HotelBookingCancellationOwnershipBoundary.PublicCancellationUiImplemented);
        Assert.True(HotelBookingOwnershipBoundary.HotelBookingApiImplemented);
        Assert.True(HotelBookingOwnershipBoundary.HotelBookingUiImplemented);
        Assert.Equal("NONE", HotelBookingCancellationOwnershipBoundary.NamedHotelSupplier);
        Assert.Equal("NONE", HotelBookingCancellationOwnershipBoundary.ProductionHotelReservationSource);
        Assert.Equal("NONE", HotelBookingCancellationOwnershipBoundary.ProductionPaymentProvider);
        Assert.Equal("IHotelReservationSource", HotelReservationOwnershipBoundary.SourcePortName);
        Assert.False(HotelReservationOwnershipBoundary.ProductionFakeReservationSourceImplemented);
        Assert.False(HotelBookingCancellationOwnershipBoundary.ProcessLocalLockIsAuthority);
        Assert.False(HotelBookingCancellationOwnershipBoundary.SharedDbContextImplemented);
        Assert.False(HotelBookingCancellationOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(HotelBookingCancellationOwnershipBoundary.DistributedTransactionImplemented);
        Assert.Equal("NodaTime.Instant", HotelBookingCancellationOwnershipBoundary.EvaluationTimestampType);
        Assert.Equal("HotelCancellationPolicySnapshot", HotelBookingCancellationOwnershipBoundary.PolicySource);
        Assert.Equal(
            HotelRateOfferOwnershipBoundary.CancellationTermsAreNotExecution,
            HotelBookingCancellationOwnershipBoundary.PolicyIsNotExecution);
        Assert.Equal(
            "HotelBookingCancelled != RefundSucceeded",
            HotelBookingCancellationOwnershipBoundary.CancelledIsNotRefundSucceeded);
        Assert.Equal(
            "NetworkTimeout != HotelSupplierCancellationAttempt.Failed",
            HotelBookingCancellationOwnershipBoundary.TimeoutIsNotFailed);
        Assert.False(HotelBookingRefundSuccessOutboxBoundary.EventMeansHotelBookingCancelled);
        Assert.False(HotelBookingCancellationRefundOutboxBoundary.EventAmountIsAuthoritative);
        Assert.NotEqual(
            HotelBookingCompensationOutboxBoundary.MessageType,
            HotelBookingCancellationRefundOutboxBoundary.MessageType);
    }

    [Fact]
    public void No_Public_Cancellation_Api_And_No_Peer_Infrastructure()
    {
        var hotelInfra = Path.Combine(
            RepoRoot, "src", "backend", "Modules", "HotelBooking",
            "TravelCore.Modules.HotelBooking.Infrastructure");
        var hotelText = string.Join('\n', Directory.EnumerateFiles(hotelInfra, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}"))
            .Select(File.ReadAllText));
        Assert.DoesNotContain("SemaphoreSlim", hotelText, StringComparison.Ordinal);
        Assert.DoesNotContain("ConcurrentDictionary", hotelText, StringComparison.Ordinal);
        Assert.Contains("/api/hotel-booking/public", hotelText, StringComparison.Ordinal);
        Assert.Contains("X-TravelCore-Hotel-Booking-Access-Token", hotelText, StringComparison.Ordinal);
        Assert.DoesNotContain("MapGet(\"/api/hotel-bookings", hotelText, StringComparison.Ordinal);

        var frontend = Path.Combine(RepoRoot, "src", "frontend", "web");
        if (Directory.Exists(frontend))
        {
            Assert.DoesNotContain(
                Directory.EnumerateFiles(frontend, "*.*", SearchOption.AllDirectories)
                    .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}"))
                    .Select(p => Path.GetRelativePath(RepoRoot, p)),
                path => path.Contains("hotel-booking", StringComparison.OrdinalIgnoreCase)
                    && path.Contains("amend", StringComparison.OrdinalIgnoreCase));
        }

        var hotelInfraProject = Projects.Single(p => p.Name == "TravelCore.Modules.HotelBooking.Infrastructure");
        var paymentInfraProject = Projects.Single(p => p.Name == "TravelCore.Modules.Payment.Infrastructure");
        Assert.DoesNotContain(
            hotelInfraProject.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name is "TravelCore.Modules.Payment.Infrastructure"
                or "TravelCore.Modules.Payment.Domain"
                or "TravelCore.Modules.Booking.Infrastructure");
        Assert.DoesNotContain(
            paymentInfraProject.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name is "TravelCore.Modules.HotelBooking.Infrastructure"
                or "TravelCore.Modules.HotelBooking.Domain");
    }

    [Fact]
    public void Partial_Refund_And_Amendment_Types_Remain_Absent()
    {
        var hotelRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "HotelBooking");
        var hotelText = string.Join('\n', Directory.EnumerateFiles(hotelRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}")
                && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                && !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Select(File.ReadAllText));
        Assert.DoesNotContain("enum PartialRefund", hotelText, StringComparison.Ordinal);
        Assert.DoesNotContain("class HotelBookingAmendment", hotelText, StringComparison.Ordinal);
        Assert.DoesNotContain("class HotelRebooking", hotelText, StringComparison.Ordinal);
        Assert.False(Regex.IsMatch(hotelText, @"\bTransactionScope\b"));
        Assert.Contains("InitiateCancellationAsync", hotelText, StringComparison.Ordinal);
        Assert.Contains("QueryCancellationStatusAsync", hotelText, StringComparison.Ordinal);
        Assert.Contains("IHotelReservationSource", hotelText, StringComparison.Ordinal);

        var paymentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment");
        var paymentText = string.Join('\n', Directory.EnumerateFiles(paymentRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}"))
            .Select(File.ReadAllText));
        Assert.Contains("PartialRefundImplemented = false", paymentText, StringComparison.Ordinal);
        Assert.DoesNotContain("PartialRefundStatus", paymentText, StringComparison.Ordinal);
    }
}
