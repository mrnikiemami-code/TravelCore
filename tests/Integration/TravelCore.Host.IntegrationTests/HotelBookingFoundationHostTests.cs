using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Infrastructure;
using TravelCore.Modules.HotelBooking.Infrastructure.Services;
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
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<HotelAvailabilityHoldService>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<HotelRateOfferAcceptanceService>());
            var resolver = scope.ServiceProvider.GetRequiredService<IHotelAvailabilitySourceResolver>();
            Assert.Empty(resolver.ListConfiguredKeys());
            var rateResolver = scope.ServiceProvider.GetRequiredService<IHotelRateOfferSourceResolver>();
            Assert.Empty(rateResolver.ListConfiguredKeys());
            Assert.Null(scope.ServiceProvider.GetService<IHotelRateOfferSource>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<HotelSupplierReservationService>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<HotelBookingCancellationService>());
            var reservationResolver = scope.ServiceProvider.GetRequiredService<IHotelReservationSourceResolver>();
            Assert.Empty(reservationResolver.ListConfiguredKeys());
            Assert.Null(scope.ServiceProvider.GetService<IHotelReservationSource>());
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var list = await client.GetAsync("/api/hotel-booking", ct);
        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        using var post = await client.PostAsync("/api/hotel-booking", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
        using var availability = await client.GetAsync("/api/hotel-booking/availability", ct);
        Assert.Equal(HttpStatusCode.NotFound, availability.StatusCode);
        using var rates = await client.GetAsync("/api/hotel-booking/rates", ct);
        Assert.Equal(HttpStatusCode.NotFound, rates.StatusCode);
        using var reserve = await client.PostAsync("/api/hotel-booking/reserve", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, reserve.StatusCode);
        using var reservations = await client.GetAsync("/api/hotel-booking/reservations", ct);
        Assert.Equal(HttpStatusCode.NotFound, reservations.StatusCode);
        using var confirm = await client.PostAsync("/api/hotel-booking/confirm", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, confirm.StatusCode);
        using var pay = await client.PostAsync("/api/hotel-booking/payment", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, pay.StatusCode);
        using var cancel = await client.PostAsync("/api/hotel-booking/cancel", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, cancel.StatusCode);
        using var cancellations = await client.GetAsync("/api/hotel-booking/cancellations", ct);
        Assert.Equal(HttpStatusCode.NotFound, cancellations.StatusCode);
        using var generic = await client.PostAsync(
            "/api/payment/hotel-booking/0198b3e0-0000-7000-8000-000000000021", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, generic.StatusCode);
    }
}
