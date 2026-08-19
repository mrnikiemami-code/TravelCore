using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P22-T008 / P22-R8: public FlightBooking is a transactional journey, not CRUD.
/// </summary>
public sealed class FlightPublicJourneyGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void Public_Journey_Header_And_Routes_Are_Flight_Specific()
    {
        Assert.Equal("/api/flight-booking/public", PublicFlightBookingCompositionBoundary.PublicApiGroup);
        Assert.Equal(
            "X-TravelCore-Flight-Booking-Access-Token",
            PublicFlightBookingCompositionBoundary.AccessTokenHeader);
        Assert.NotEqual(
            PublicBookingCompositionBoundary.AccessTokenHeader,
            PublicFlightBookingCompositionBoundary.AccessTokenHeader);
        Assert.NotEqual(
            PublicHotelBookingCompositionBoundary.AccessTokenHeader,
            PublicFlightBookingCompositionBoundary.AccessTokenHeader);
        Assert.Equal(
            "FlightBooking access token != Tour Booking access token",
            PublicFlightBookingCompositionBoundary.FlightTokenIsNotTourToken);
        Assert.Equal(
            "FlightBooking access token != HotelBooking access token",
            PublicFlightBookingCompositionBoundary.FlightTokenIsNotHotelToken);
        Assert.Equal(
            "PNR Confirmed != FlightBooking Confirmed",
            PublicFlightBookingCompositionBoundary.PnrConfirmedIsNotFlightConfirmed);
        Assert.False(PublicFlightBookingCompositionBoundary.PublicListingImplemented);
        Assert.False(PublicFlightBookingCompositionBoundary.GenericCrudImplemented);
        Assert.False(PublicFlightBookingCompositionBoundary.PublicRefundCommandImplemented);
        Assert.False(PublicFlightBookingCompositionBoundary.CardCollectionImplemented);
        Assert.False(PublicFlightBookingCompositionBoundary.OperationalHttpRouteImplemented);
        Assert.False(PublicFlightBookingCompositionBoundary.RawTokenUrlExposureImplemented);
        Assert.False(PublicFlightBookingCompositionBoundary.RawTokenLocalStorageImplemented);
        Assert.False(PublicFlightBookingCompositionBoundary.MultiCityImplemented);
        Assert.False(FlightOperationalBoundary.PublicOperationalEndpointImplemented);
        Assert.False(FlightOperationalBoundary.ManualFlightBookingMutationImplemented);
        Assert.Equal("NONE", FlightOwnershipBoundary.NamedFlightSupplier);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionSearchSource);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionAvailabilitySource);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionOfferSource);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionReservationSource);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionTicketingSource);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionCancellationSource);
        Assert.False(FlightOwnershipBoundary.PnrModelImplemented);
        Assert.True(FlightOwnershipBoundary.CancellationModelImplemented);
        Assert.True(FlightOwnershipBoundary.PublicApiImplemented);
        Assert.True(FlightOwnershipBoundary.FrontendImplemented);
        Assert.False(FlightOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(FlightOwnershipBoundary.SharedDbContextImplemented);
        Assert.Equal("NONE", PaymentProviderTrustBoundary.NamedProviderSelected);
        Assert.False(PaymentProviderTrustBoundary.NamedProductionAdapterImplemented);
        Assert.Null(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.PNR"));
        Assert.Null(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.IFlightSupplierGateway"));
        Assert.Null(typeof(FlightContractsAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Contracts.IFlightSupplierGateway"));
    }

    [Fact]
    public void Operational_Query_Has_No_Mutation_Surface()
    {
        var methods = typeof(IFlightOperationalQuery).GetMethods().Select(m => m.Name).ToArray();
        Assert.DoesNotContain("SetStatus", methods);
        Assert.DoesNotContain("ForceConfirm", methods);
        Assert.DoesNotContain("ForceTicket", methods);
        Assert.DoesNotContain("ForceCancel", methods);
        Assert.DoesNotContain("MarkPaid", methods);
        Assert.DoesNotContain("MarkRefunded", methods);
        var dto = typeof(FlightOperationalRead).GetProperties().Select(p => p.Name).ToArray();
        Assert.DoesNotContain("Passport", dto);
        Assert.DoesNotContain("BirthDate", dto);
        Assert.DoesNotContain("AccessToken", dto);
        Assert.DoesNotContain("TokenHash", dto);
        Assert.DoesNotContain("RawPayload", dto);
        Assert.Contains(nameof(FlightOperationalRead.FlightBookingId), dto);
        Assert.Contains(nameof(FlightOperationalRead.TicketSummary), dto);
    }

    [Fact]
    public void Frontend_Transactional_Pages_Are_Noindex_And_Do_Not_Collect_Cards()
    {
        var web = Path.Combine(RepoRoot, "src", "frontend", "web", "src");
        string[] pages =
        [
            Path.Combine(web, "app", "[locale]", "flights", "page.tsx"),
            Path.Combine(web, "app", "[locale]", "flight-bookings", "[flightBookingId]", "page.tsx"),
            Path.Combine(web, "app", "[locale]", "flight-bookings", "[flightBookingId]", "payment", "page.tsx"),
            Path.Combine(
                web,
                "app",
                "[locale]",
                "flight-bookings",
                "[flightBookingId]",
                "payment",
                "return",
                "page.tsx"),
        ];
        foreach (var page in pages)
        {
            Assert.True(File.Exists(page), page);
            var text = File.ReadAllText(page);
            Assert.Contains("index: false", text, StringComparison.Ordinal);
            Assert.DoesNotContain("localStorage", text, StringComparison.Ordinal);
        }

        var feature = Path.Combine(web, "features", "flight-booking");
        var featureText = string.Join(
            '\n',
            Directory.EnumerateFiles(feature, "*.ts*", SearchOption.AllDirectories).Select(File.ReadAllText));
        Assert.Contains("sessionStorage", featureText, StringComparison.Ordinal);
        Assert.DoesNotContain("localStorage", featureText, StringComparison.Ordinal);
        Assert.DoesNotContain("X-TravelCore-Booking-Access-Token", featureText, StringComparison.Ordinal);
        Assert.DoesNotContain("X-TravelCore-Hotel-Booking-Access-Token", featureText, StringComparison.Ordinal);
        Assert.Contains("X-TravelCore-Flight-Booking-Access-Token", featureText, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"cardNumber\"", featureText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("name=\"cvv\"", featureText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("name=\"cvc\"", featureText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dateOfBirth", featureText, StringComparison.Ordinal);
        Assert.DoesNotContain("passport", featureText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("flight-bookings/${result.data.flightBookingId}", featureText, StringComparison.Ordinal);
        Assert.DoesNotContain("accessToken=", featureText, StringComparison.Ordinal);
        Assert.Contains("fa:", featureText, StringComparison.Ordinal);
        Assert.Contains("en:", featureText, StringComparison.Ordinal);
        Assert.Contains("ar:", featureText, StringComparison.Ordinal);
    }

    [Fact]
    public void Public_Endpoints_Do_Not_Enumerate_Or_Mutate_Status()
    {
        var endpoints = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Flight",
            "TravelCore.Modules.Flight.Infrastructure",
            "Endpoints",
            "PublicFlightBookingEndpoints.cs"));
        Assert.Contains("MapPost(\"/search\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapPost(\"/initiations\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapGet(\"/{flightBookingId:guid}\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapPost(\"/{flightBookingId:guid}/offers\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapPost(\"/{flightBookingId:guid}/reservations\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapGet(\"/{flightBookingId:guid}/payment\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapPost(\"/{flightBookingId:guid}/payment/initiation\"", endpoints, StringComparison.Ordinal);
        Assert.Contains("MapPost(\"/{flightBookingId:guid}/cancellation\"", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPut", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPatch", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("/refund", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("ForceConfirm", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("ForceTicket", endpoints, StringComparison.Ordinal);
        Assert.Contains("X-TravelCore-Flight-Booking-Access-Token", File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Flight",
            "TravelCore.Modules.Flight.Contracts",
            "PublicFlightBookingCompositionBoundary.cs")), StringComparison.Ordinal);
        Assert.Null(typeof(FlightBooking).GetMethod("SetStatus"));
        Assert.Null(typeof(FlightBooking).GetMethod("ForceConfirm"));
        Assert.Null(typeof(FlightBooking).GetMethod("ForceTicket"));
        Assert.Null(typeof(FlightBooking).GetMethod("ForceCancel"));
        Assert.Null(typeof(FlightBooking).GetMethod("MarkPaid"));
        Assert.Null(typeof(FlightBooking).GetMethod("MarkRefunded"));
        Assert.False(PaymentRefundBoundary.PartialRefundImplemented);
    }
}
