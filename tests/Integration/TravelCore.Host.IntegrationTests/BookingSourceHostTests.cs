using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.Booking.Infrastructure.Services;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class BookingSourceHostTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public BookingSourceHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Host_Wires_Agency_Origin_Contract_And_Keeps_Public_Booking_Api_Out()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var scope = factory.Services.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IAgencyOriginContextQuery>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<BookingCreationService>());
        Assert.IsNotType<BookingCreationService>(
            scope.ServiceProvider.GetRequiredService<IAgencyOriginContextQuery>());

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.GetAsync("/api/booking", ct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
