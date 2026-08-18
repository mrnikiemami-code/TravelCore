using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class HotelBookingPaymentIntegrationGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void Payment_Targets_Are_Closed_Tour_And_Hotel_Only()
    {
        Assert.Equal(
            new[] { PaymentTargetKind.TourBooking, PaymentTargetKind.HotelBooking },
            Enum.GetValues<PaymentTargetKind>());
        Assert.Equal(1, (int)PaymentTargetKind.TourBooking);
        Assert.Equal(2, (int)PaymentTargetKind.HotelBooking);
        Assert.False(PaymentOwnershipBoundary.GeneralizedTargetTypeImplemented);
        Assert.Null(typeof(Payment).GetProperty("TargetType"));
        Assert.False(PaymentRefundBoundary.PartialRefundImplemented);
        Assert.False(HotelBookingOwnershipBoundary.OwnsPayment);
        Assert.True(HotelBookingOwnershipBoundary.PaymentIntegrationImplemented);
        Assert.False(HotelBookingOwnershipBoundary.HotelBookingApiImplemented);
        Assert.False(HotelBookingOwnershipBoundary.HotelBookingUiImplemented);
        Assert.False(HotelBookingOwnershipBoundary.SharedDbContextImplemented);
        Assert.False(HotelBookingOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(PaymentOwnershipBoundary.SharedDbContextImplemented);
        Assert.False(PaymentOwnershipBoundary.PeerSchemaForeignKeyImplemented);
    }

    [Fact]
    public void HotelBooking_Confirmation_Has_No_Generic_Confirm_Or_Cancel()
    {
        Assert.Null(typeof(HotelBooking).GetMethod("Confirm"));
        Assert.Null(typeof(HotelBooking).GetMethod("SetConfirmed"));
        Assert.Null(typeof(HotelBooking).GetMethod("ForceConfirm"));
        Assert.Null(typeof(HotelBooking).GetMethod("Cancel"));
        Assert.Null(typeof(HotelBooking).GetMethod("SetCancelled"));
        Assert.Null(typeof(HotelBooking).GetMethod("ForceCancel"));
        Assert.NotNull(typeof(HotelBooking).GetMethod(nameof(HotelBooking.ConfirmFromAuthoritativePaymentAndSupplierEvidence)));
        Assert.NotNull(typeof(HotelBooking).GetMethod(nameof(HotelBooking.CancelFromAuthoritativePaymentCompensation)));
        Assert.NotNull(typeof(HotelBooking).GetMethod(nameof(HotelBooking.CancelFromAuthoritativeSupplierCancellation)));
        Assert.Equal(
            new[] { "Pending", "Confirmed", "Cancelled" },
            Enum.GetNames<HotelBookingStatus>());
    }

    [Fact]
    public void Hotel_And_Tour_Payment_Events_Are_Isolated()
    {
        Assert.NotEqual(
            PaymentSuccessOutboxBoundary.MessageType,
            HotelBookingPaymentSuccessOutboxBoundary.MessageType);
        Assert.NotEqual(
            RefundSuccessOutboxBoundary.MessageType,
            HotelBookingRefundSuccessOutboxBoundary.MessageType);
        Assert.NotEqual(
            BookingCompensationOutboxBoundary.MessageType,
            HotelBookingCompensationOutboxBoundary.MessageType);
        Assert.False(HotelBookingPaymentSuccessOutboxBoundary.EventMeansHotelBookingConfirmed);
        Assert.False(HotelBookingRefundSuccessOutboxBoundary.EventMeansHotelBookingCancelled);
        Assert.NotEqual(
            HotelBookingCompensationOutboxBoundary.MessageType,
            HotelBookingCancellationRefundOutboxBoundary.MessageType);
        Assert.False(HotelBookingCancellationRefundOutboxBoundary.EventAmountIsAuthoritative);
    }

    [Fact]
    public void No_Public_HotelBooking_Payment_Api_Or_Generic_Target_Route()
    {
        var hotelInfra = Path.Combine(
            RepoRoot, "src", "backend", "Modules", "HotelBooking",
            "TravelCore.Modules.HotelBooking.Infrastructure");
        var paymentInfra = Path.Combine(
            RepoRoot, "src", "backend", "Modules", "Payment",
            "TravelCore.Modules.Payment.Infrastructure");
        var hotelText = string.Join('\n', Directory.EnumerateFiles(hotelInfra, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}"))
            .Select(File.ReadAllText));
        var paymentText = string.Join('\n', Directory.EnumerateFiles(paymentInfra, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}"))
            .Select(File.ReadAllText));
        Assert.DoesNotContain("MapGet", hotelText, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPost", hotelText, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/hotel-booking", paymentText, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/payment/{targetType}/{targetId}", paymentText, StringComparison.Ordinal);
        Assert.DoesNotContain("IHotelRateOfferSource", paymentText, StringComparison.Ordinal);
        Assert.False(Regex.IsMatch(paymentText, @"\bstring\s+TargetType\b"));
    }

    [Fact]
    public void Peer_Infrastructure_And_Domain_Remain_Forbidden()
    {
        var paymentInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Payment.Infrastructure");
        var hotelInfra = Projects.Single(p => p.Name == "TravelCore.Modules.HotelBooking.Infrastructure");
        Assert.Contains(
            paymentInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Modules.HotelBooking.Contracts");
        Assert.Contains(
            hotelInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Modules.Payment.Contracts");
        Assert.DoesNotContain(
            paymentInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name is "TravelCore.Modules.HotelBooking.Infrastructure"
                or "TravelCore.Modules.HotelBooking.Domain");
        Assert.DoesNotContain(
            hotelInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name is "TravelCore.Modules.Payment.Infrastructure"
                or "TravelCore.Modules.Payment.Domain");
    }
}
