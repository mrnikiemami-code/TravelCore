using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Infrastructure;
using TravelCore.Modules.Flight.Infrastructure.Services;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class FlightFoundationHostTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public FlightFoundationHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Host_Registers_Flight_Public_Transactional_Routes_Without_Crud()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
            Assert.Equal("flight", FlightDbContext.SchemaName);
            Assert.Equal("flight", db.Model.GetDefaultSchema());
            Assert.False(db.Database.HasPendingModelChanges());
            var searchResolver = scope.ServiceProvider.GetRequiredService<IFlightSearchSourceResolver>();
            var availabilityResolver = scope.ServiceProvider.GetRequiredService<IFlightOfferAvailabilitySourceResolver>();
            var offerResolver = scope.ServiceProvider.GetRequiredService<IFlightOfferSourceResolver>();
            var reservationResolver = scope.ServiceProvider.GetRequiredService<IFlightReservationSourceResolver>();
            var ticketingResolver = scope.ServiceProvider.GetRequiredService<IFlightTicketingSourceResolver>();
            var cancellationResolver = scope.ServiceProvider.GetRequiredService<IFlightCancellationSourceResolver>();
            Assert.Empty(searchResolver.ListConfiguredKeys());
            Assert.Empty(availabilityResolver.ListConfiguredKeys());
            Assert.Empty(offerResolver.ListConfiguredKeys());
            Assert.Empty(reservationResolver.ListConfiguredKeys());
            Assert.Empty(ticketingResolver.ListConfiguredKeys());
            Assert.Empty(cancellationResolver.ListConfiguredKeys());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<FlightSupplierReservationService>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<FlightTicketingService>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<FlightBookingCancellationService>());
            Assert.Null(scope.ServiceProvider.GetService<IFlightCancellationSource>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IFlightBookingPaymentObligationQuery>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IFlightOperationalQuery>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IPublicFlightBookingSearchService>());
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var list = await client.GetAsync("/api/flight", ct);
        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        using var publicList = await client.GetAsync("/api/flight/public", ct);
        Assert.Equal(HttpStatusCode.NotFound, publicList.StatusCode);
        using var bookings = await client.GetAsync("/api/flight-bookings", ct);
        Assert.Equal(HttpStatusCode.NotFound, bookings.StatusCode);
        using var publicBookings = await client.GetAsync("/api/flight-booking/public", ct);
        Assert.Equal(HttpStatusCode.NotFound, publicBookings.StatusCode);
        using var post = await client.PostAsync("/api/flight", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
        using var search = await client.GetAsync("/api/flight/search", ct);
        Assert.Equal(HttpStatusCode.NotFound, search.StatusCode);
        using var book = await client.PostAsync("/api/flight/bookings", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, book.StatusCode);
        using var ops = await client.GetAsync($"/api/flight-booking/ops/{Guid.CreateVersion7():D}", ct);
        Assert.Equal(HttpStatusCode.NotFound, ops.StatusCode);
    }
}
