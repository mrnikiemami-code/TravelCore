using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Infrastructure;
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
    public async Task Host_Registers_FlightDbContext_Without_Flight_Endpoints()
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
            Assert.Empty(searchResolver.ListConfiguredKeys());
            Assert.Empty(availabilityResolver.ListConfiguredKeys());
            Assert.Empty(offerResolver.ListConfiguredKeys());
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var list = await client.GetAsync("/api/flight", ct);
        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        using var publicList = await client.GetAsync("/api/flight/public", ct);
        Assert.Equal(HttpStatusCode.NotFound, publicList.StatusCode);
        using var post = await client.PostAsync("/api/flight", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
        using var search = await client.GetAsync("/api/flight/search", ct);
        Assert.Equal(HttpStatusCode.NotFound, search.StatusCode);
        using var book = await client.PostAsync("/api/flight/bookings", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, book.StatusCode);
    }
}
