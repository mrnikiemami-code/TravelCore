using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TravelCore.Modules.HotelBooking.Infrastructure;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class HotelBookingFoundationHostTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public HotelBookingFoundationHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Host_Registers_HotelBookingDbContext_Without_HotelBooking_Endpoints()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HotelBookingDbContext>();
            Assert.Equal("hotel_booking", HotelBookingDbContext.SchemaName);
            Assert.Equal("hotel_booking", db.Model.GetDefaultSchema());
            Assert.False(db.Database.HasPendingModelChanges());
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var list = await client.GetAsync("/api/hotel-booking", ct);
        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        using var post = await client.PostAsync("/api/hotel-booking", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
        using var availability = await client.GetAsync("/api/hotel-booking/availability", ct);
        Assert.Equal(HttpStatusCode.NotFound, availability.StatusCode);
        using var reserve = await client.PostAsync("/api/hotel-booking/reserve", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, reserve.StatusCode);
    }
}
