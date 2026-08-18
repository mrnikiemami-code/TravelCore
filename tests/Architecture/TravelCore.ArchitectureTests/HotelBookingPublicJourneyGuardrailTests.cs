using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P21-T008 / P21-R8: public HotelBooking is a transactional journey, not CRUD.
/// </summary>
public sealed class HotelBookingPublicJourneyGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void Public_Journey_Header_And_Routes_Are_HotelBooking_Specific()
    {
        Assert.Equal("/api/hotel-booking/public", PublicHotelBookingCompositionBoundary.PublicApiGroup);
        Assert.Equal(
            "X-TravelCore-Hotel-Booking-Access-Token",
            PublicHotelBookingCompositionBoundary.AccessTokenHeader);
        Assert.NotEqual(
            PublicBookingCompositionBoundary.AccessTokenHeader,
            PublicHotelBookingCompositionBoundary.AccessTokenHeader);
        Assert.Equal(
            "HotelBooking access token != Tour Booking access token",
            PublicHotelBookingCompositionBoundary.HotelTokenIsNotTourToken);
        Assert.False(PublicHotelBookingCompositionBoundary.PublicListingImplemented);
        Assert.False(PublicHotelBookingCompositionBoundary.GenericCrudImplemented);
        Assert.False(PublicHotelBookingCompositionBoundary.PublicRefundCommandImplemented);
        Assert.False(PublicHotelBookingCompositionBoundary.CardCollectionImplemented);
        Assert.False(PublicHotelBookingCompositionBoundary.OperationalHttpRouteImplemented);
        Assert.False(PublicHotelBookingCompositionBoundary.RawTokenUrlExposureImplemented);
        Assert.False(HotelBookingOperationalBoundary.PublicOperationalEndpointImplemented);
        Assert.False(HotelBookingOperationalBoundary.ManualHotelBookingMutationImplemented);
        Assert.Equal("NONE", HotelSourceReadinessBoundary.NamedHotelSupplier);
        Assert.Equal("NONE", HotelSourceReadinessBoundary.ProductionAvailabilitySource);
        Assert.Equal("NONE", HotelSourceReadinessBoundary.ProductionRateSource);
        Assert.Equal("NONE", HotelSourceReadinessBoundary.ProductionReservationSource);
        Assert.Equal("NONE", HotelSourceReadinessBoundary.ProductionPaymentProvider);
        Assert.True(HotelSourceReadinessBoundary.ZeroProductionSourcesValid);
        Assert.False(HotelSourceReadinessBoundary.SmartRoutingImplemented);
        Assert.False(HotelSourceReadinessBoundary.AutomaticFailoverImplemented);
        Assert.False(HotelSourceReadinessBoundary.CapabilityInferredFromSourceName);
        Assert.True(HotelBookingOwnershipBoundary.OwnsPayment == false);
        Assert.True(HotelBookingOwnershipBoundary.HotelBookingApiImplemented);
        Assert.True(HotelBookingOwnershipBoundary.HotelBookingUiImplemented);
    }

    [Fact]
    public void Operational_Query_Has_No_Mutation_Surface()
    {
        var methods = typeof(IHotelBookingOperationalQuery).GetMethods().Select(m => m.Name).ToArray();
        Assert.DoesNotContain("SetStatus", methods);
        Assert.DoesNotContain("ForceConfirm", methods);
        Assert.DoesNotContain("ForceCancel", methods);
        Assert.DoesNotContain("MarkPaid", methods);
        Assert.DoesNotContain("MarkRefunded", methods);
        Assert.DoesNotContain("MarkSupplierConfirmed", methods);
        var dto = typeof(HotelBookingOperationalRead).GetProperties().Select(p => p.Name).ToArray();
        Assert.DoesNotContain("Passport", dto);
        Assert.DoesNotContain("NationalId", dto);
        Assert.DoesNotContain("AccessToken", dto);
        Assert.DoesNotContain("TokenHash", dto);
        Assert.DoesNotContain("RawPayload", dto);
        Assert.Contains(nameof(HotelBookingOperationalRead.HotelBookingId), dto);
        Assert.Contains(nameof(HotelBookingOperationalRead.PlaceId), dto);
    }

    [Fact]
    public void Frontend_Transactional_Pages_Are_Noindex_And_Do_Not_Collect_Cards()
    {
        var web = Path.Combine(RepoRoot, "src", "frontend", "web", "src");
        string[] pages =
        [
            Path.Combine(web, "app", "[locale]", "places", "[slug]", "book", "page.tsx"),
            Path.Combine(web, "app", "[locale]", "hotel-bookings", "[hotelBookingId]", "page.tsx"),
            Path.Combine(web, "app", "[locale]", "hotel-bookings", "[hotelBookingId]", "payment", "page.tsx"),
            Path.Combine(web, "app", "[locale]", "hotel-bookings", "[hotelBookingId]", "payment", "return", "page.tsx"),
        ];
        foreach (var page in pages)
        {
            Assert.True(File.Exists(page), page);
            var text = File.ReadAllText(page);
            Assert.Contains("index: false", text, StringComparison.Ordinal);
            Assert.DoesNotContain("localStorage", text, StringComparison.Ordinal);
        }

        var feature = Path.Combine(web, "features", "hotel-booking");
        var featureText = string.Join(
            '\n',
            Directory.EnumerateFiles(feature, "*.ts*", SearchOption.AllDirectories).Select(File.ReadAllText));
        Assert.Contains("sessionStorage", featureText, StringComparison.Ordinal);
        Assert.DoesNotContain("localStorage", featureText, StringComparison.Ordinal);
        Assert.DoesNotContain("X-TravelCore-Booking-Access-Token", featureText, StringComparison.Ordinal);
        Assert.Contains("X-TravelCore-Hotel-Booking-Access-Token", featureText, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"cardNumber\"", featureText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("name=\"cvv\"", featureText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("name=\"cvc\"", featureText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AgeAtCheckIn", featureText, StringComparison.Ordinal);
        Assert.DoesNotContain("dateOfBirth", featureText, StringComparison.Ordinal);
        Assert.Contains("hotel-bookings/${result.data.hotelBookingId}", featureText, StringComparison.Ordinal);
        Assert.DoesNotContain("accessToken=", featureText, StringComparison.Ordinal);
    }

    [Fact]
    public void Public_Endpoints_Do_Not_Enumerate_Or_Mutate_Status()
    {
        var endpoints = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "HotelBooking",
            "TravelCore.Modules.HotelBooking.Infrastructure",
            "Endpoints",
            "PublicHotelBookingEndpoints.cs"));
        Assert.Contains("MapPost(\"/initiations\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapGet(\"/{hotelBookingId:guid}\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapPost(\"/{hotelBookingId:guid}/availability\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapPost(\"/{hotelBookingId:guid}/rate-offers\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapGet(\"/{hotelBookingId:guid}/payment\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapPost(\"/{hotelBookingId:guid}/payment/initiation\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapPost(\"/{hotelBookingId:guid}/cancellation\"", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPut", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPatch", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("/refund", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("ForceConfirm", endpoints, StringComparison.Ordinal);
        Assert.Contains("X-TravelCore-Hotel-Booking-Access-Token", File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "HotelBooking",
            "TravelCore.Modules.HotelBooking.Contracts",
            "PublicHotelBookingCompositionBoundary.cs")), StringComparison.Ordinal);
        Assert.Null(typeof(HotelBooking).GetMethod("SetStatus"));
        Assert.Null(typeof(HotelBooking).GetMethod("ForceConfirm"));
        Assert.Null(typeof(HotelBooking).GetMethod("ForceCancel"));
        Assert.Null(typeof(HotelBooking).GetMethod("MarkPaid"));
        Assert.Null(typeof(HotelBooking).GetMethod("MarkRefunded"));
        Assert.False(PaymentRefundBoundary.PartialRefundImplemented);
    }
}
