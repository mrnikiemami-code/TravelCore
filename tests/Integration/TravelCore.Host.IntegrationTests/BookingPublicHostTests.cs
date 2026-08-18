using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure.Services;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Pricing.Domain;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class BookingPublicHostTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public BookingPublicHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Public_initiation_creates_Pending_Direct_Booking_with_Quote_hold_and_token()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var seeded = await SeedPublishedDepartureAsync(maxPax: 8, ct);

        var idempotency = Guid.NewGuid().ToString("D");
        using var first = await PostInitiationAsync(client, seeded.DepartureId, idempotency, passengerCount: 2, ct);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        using var firstDoc = JsonDocument.Parse(await first.Content.ReadAsStringAsync(ct));
        var root = firstDoc.RootElement;
        var bookingId = root.GetProperty("bookingId").GetGuid();
        var token = root.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(root.GetProperty("accessTokenIssued").GetBoolean());
        Assert.Equal("Pending", root.GetProperty("status").GetString());
        Assert.Equal("Direct", root.GetProperty("sourceKind").GetString());
        Assert.False(root.GetProperty("confirmed").GetBoolean());
        Assert.Equal(seeded.DepartureId, root.GetProperty("tourDepartureId").GetGuid());
        Assert.Equal(1000m, root.GetProperty("monetary").GetProperty("totalAmount").GetDecimal());
        Assert.Equal("USD", root.GetProperty("monetary").GetProperty("currency").GetString());
        Assert.Equal("Active", root.GetProperty("hold").GetProperty("status").GetString());
        Assert.Equal(2, root.GetProperty("hold").GetProperty("seatCount").GetInt32());
        Assert.False(root.TryGetProperty("paymentStatus", out _));

        using var replay = await PostInitiationAsync(client, seeded.DepartureId, idempotency, passengerCount: 2, ct);
        Assert.Equal(HttpStatusCode.Created, replay.StatusCode);
        using var replayDoc = JsonDocument.Parse(await replay.Content.ReadAsStringAsync(ct));
        Assert.Equal(bookingId, replayDoc.RootElement.GetProperty("bookingId").GetGuid());
        Assert.False(replayDoc.RootElement.GetProperty("accessTokenIssued").GetBoolean());

        using var missingToken = await client.GetAsync($"/api/booking/public/{bookingId:D}", ct);
        Assert.Equal(HttpStatusCode.NotFound, missingToken.StatusCode);

        using var wrongToken = new HttpRequestMessage(HttpMethod.Get, $"/api/booking/public/{bookingId:D}");
        wrongToken.Headers.Add("X-TravelCore-Booking-Access-Token", "not-the-token");
        using var wrong = await client.SendAsync(wrongToken, ct);
        Assert.Equal(HttpStatusCode.NotFound, wrong.StatusCode);

        using var okToken = new HttpRequestMessage(HttpMethod.Get, $"/api/booking/public/{bookingId:D}");
        okToken.Headers.Add("X-TravelCore-Booking-Access-Token", token);
        using var ok = await client.SendAsync(okToken, ct);
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        using var readDoc = JsonDocument.Parse(await ok.Content.ReadAsStringAsync(ct));
        Assert.Equal("Pending", readDoc.RootElement.GetProperty("status").GetString());
        Assert.False(readDoc.RootElement.GetProperty("confirmed").GetBoolean());
        Assert.Equal(2, readDoc.RootElement.GetProperty("passengers").GetArrayLength());

        using var list = await client.GetAsync("/api/booking/public", ct);
        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        using var confirm = await client.PostAsJsonAsync($"/api/booking/public/{bookingId:D}/confirm", new { }, ct);
        Assert.Equal(HttpStatusCode.NotFound, confirm.StatusCode);
    }

    [Fact]
    public async Task Public_Payment_Is_Booking_Scoped_And_Token_Protected()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var seeded = await SeedPublishedDepartureAsync(maxPax: 4, ct);
        using var created = await PostInitiationAsync(
            client,
            seeded.DepartureId,
            Guid.NewGuid().ToString("D"),
            passengerCount: 1,
            ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync(ct));
        var bookingId = createdDoc.RootElement.GetProperty("bookingId").GetGuid();
        var token = createdDoc.RootElement.GetProperty("accessToken").GetString();
        var paymentPath = $"/api/booking/public/{bookingId:D}/payment";
        var initiationPath = paymentPath + "/initiation";

        using var missing = await client.GetAsync(paymentPath, ct);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        using var wrongReq = new HttpRequestMessage(HttpMethod.Get, paymentPath);
        wrongReq.Headers.Add("X-TravelCore-Booking-Access-Token", "not-the-token");
        using var wrong = await client.SendAsync(wrongReq, ct);
        Assert.Equal(HttpStatusCode.NotFound, wrong.StatusCode);

        using var okReq = new HttpRequestMessage(HttpMethod.Get, paymentPath);
        okReq.Headers.Add("X-TravelCore-Booking-Access-Token", token);
        using var ok = await client.SendAsync(okReq, ct);
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        using var okDoc = JsonDocument.Parse(await ok.Content.ReadAsStringAsync(ct));
        Assert.Equal("Pending", okDoc.RootElement.GetProperty("bookingStatus").GetString());
        Assert.Equal("Pending", okDoc.RootElement.GetProperty("paymentStatus").GetString());
        Assert.False(okDoc.RootElement.GetProperty("providerInitiationPossible").GetBoolean());
        Assert.Equal("Unavailable", okDoc.RootElement.GetProperty("safeAction").GetString());
        Assert.Equal(1000m, okDoc.RootElement.GetProperty("amount").GetDecimal());
        Assert.Equal("USD", okDoc.RootElement.GetProperty("currencyCode").GetString());
        var paymentId = okDoc.RootElement.GetProperty("paymentId").GetGuid();

        using var tamper = new HttpRequestMessage(HttpMethod.Post, initiationPath)
        {
            Content = JsonContent.Create(new
            {
                amount = 1m,
                currencyCode = "EUR",
                success = true,
                isPaid = true,
                paymentId = Guid.CreateVersion7(),
                providerKey = "test",
            }),
        };
        tamper.Headers.Add("X-TravelCore-Booking-Access-Token", token);
        tamper.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("D"));
        using var tamperResponse = await client.SendAsync(tamper, ct);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, tamperResponse.StatusCode);

        using var afterTamperReq = new HttpRequestMessage(HttpMethod.Get, paymentPath);
        afterTamperReq.Headers.Add("X-TravelCore-Booking-Access-Token", token);
        using var afterTamper = await client.SendAsync(afterTamperReq, ct);
        using var afterDoc = JsonDocument.Parse(await afterTamper.Content.ReadAsStringAsync(ct));
        Assert.Equal(paymentId, afterDoc.RootElement.GetProperty("paymentId").GetGuid());
        Assert.Equal(1000m, afterDoc.RootElement.GetProperty("amount").GetDecimal());
        Assert.Equal("USD", afterDoc.RootElement.GetProperty("currencyCode").GetString());
        Assert.Equal("Pending", afterDoc.RootElement.GetProperty("paymentStatus").GetString());

        using var paymentLookup = await client.GetAsync($"/api/payment/{paymentId:D}", ct);
        Assert.Equal(HttpStatusCode.NotFound, paymentLookup.StatusCode);
        using var paymentList = await client.GetAsync("/api/payment/public", ct);
        Assert.Equal(HttpStatusCode.NotFound, paymentList.StatusCode);
        using var refund = await client.PostAsync($"/api/booking/public/{bookingId:D}/payment/refund", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, refund.StatusCode);
        using var unknown = new HttpRequestMessage(HttpMethod.Get, $"/api/booking/public/{Guid.CreateVersion7():D}/payment");
        unknown.Headers.Add("X-TravelCore-Booking-Access-Token", token);
        using var unknownResponse = await client.SendAsync(unknown, ct);
        Assert.Equal(HttpStatusCode.NotFound, unknownResponse.StatusCode);
    }

    [Fact]
    public async Task Public_Payment_Cross_User_And_Cancelled_Booking_Are_Rejected()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var owner = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var stranger = factory.CreateClient(new() { AllowAutoRedirect = false });
        var seeded = await SeedPublishedDepartureAsync(maxPax: 4, ct);
        await RegisterAndLoginAsync(owner, $"t007-owner-{Guid.NewGuid():N}@travelcore.test", "Owner-Password-1", ct);
        await RegisterAndLoginAsync(stranger, $"t007-other-{Guid.NewGuid():N}@travelcore.test", "Other-Password-1", ct);

        using var created = await PostInitiationAsync(
            owner,
            seeded.DepartureId,
            Guid.NewGuid().ToString("D"),
            passengerCount: 1,
            ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync(ct));
        var bookingId = createdDoc.RootElement.GetProperty("bookingId").GetGuid();
        var paymentPath = $"/api/booking/public/{bookingId:D}/payment";

        using var ownerRead = await owner.GetAsync(paymentPath, ct);
        Assert.Equal(HttpStatusCode.OK, ownerRead.StatusCode);
        using var strangerRead = await stranger.GetAsync(paymentPath, ct);
        Assert.Equal(HttpStatusCode.NotFound, strangerRead.StatusCode);

        await using (var bookingDb = _fixture.CreateBookingDb())
        {
            await new BookingCancellationService(bookingDb)
                .CancelPendingAsync(BookingId.From(bookingId), SystemClock.Instance.GetCurrentInstant(), ct);
        }

        using var cancelled = await owner.PostAsJsonAsync(
            paymentPath + "/initiation",
            new { idempotencyKey = Guid.NewGuid().ToString("D") },
            ct);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, cancelled.StatusCode);
    }

    [Fact]
    public async Task Public_initiation_rejects_overbook_agency_forge_and_missing_price()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var tight = await SeedPublishedDepartureAsync(maxPax: 1, ct);

        using var overbook = await PostInitiationAsync(
            client,
            tight.DepartureId,
            Guid.NewGuid().ToString("D"),
            passengerCount: 2,
            ct);
        Assert.Equal(HttpStatusCode.Conflict, overbook.StatusCode);

        using var agency = await client.PostAsJsonAsync(
            "/api/booking/public/initiations",
            Body(tight.DepartureId, Guid.NewGuid().ToString("D"), 1, sourceKind: "Agency"),
            ct);
        Assert.Equal(HttpStatusCode.BadRequest, agency.StatusCode);

        var unpublished = await SeedPublishedDepartureAsync(maxPax: 4, ct, publish: false);
        using var draft = await PostInitiationAsync(
            client,
            unpublished.DepartureId,
            Guid.NewGuid().ToString("D"),
            passengerCount: 1,
            ct);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, draft.StatusCode);

        var noPrice = await SeedPublishedDepartureAsync(maxPax: 4, ct, withPrice: false);
        using var missingPrice = await PostInitiationAsync(
            client,
            noPrice.DepartureId,
            Guid.NewGuid().ToString("D"),
            passengerCount: 1,
            ct);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingPrice.StatusCode);
    }

    [Fact]
    public async Task Authenticated_owner_can_read_without_token_and_other_user_cannot()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var owner = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var stranger = factory.CreateClient(new() { AllowAutoRedirect = false });
        var seeded = await SeedPublishedDepartureAsync(maxPax: 4, ct);

        await RegisterAndLoginAsync(owner, $"t008-owner-{Guid.NewGuid():N}@travelcore.test", "Owner-Password-1", ct);
        await RegisterAndLoginAsync(stranger, $"t008-other-{Guid.NewGuid():N}@travelcore.test", "Other-Password-1", ct);

        using var created = await PostInitiationAsync(
            owner,
            seeded.DepartureId,
            Guid.NewGuid().ToString("D"),
            passengerCount: 1,
            ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync(ct));
        var bookingId = createdDoc.RootElement.GetProperty("bookingId").GetGuid();

        using var ownerRead = await owner.GetAsync($"/api/booking/public/{bookingId:D}", ct);
        Assert.Equal(HttpStatusCode.OK, ownerRead.StatusCode);

        using var strangerRead = await stranger.GetAsync($"/api/booking/public/{bookingId:D}", ct);
        Assert.Equal(HttpStatusCode.NotFound, strangerRead.StatusCode);
    }

    private async Task<(Guid DepartureId, Guid ProductId)> SeedPublishedDepartureAsync(
        int maxPax,
        CancellationToken ct,
        bool publish = true,
        bool withPrice = true)
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        Guid departureId;
        Guid productId;
        await using (var tour = _fixture.CreateTourDb())
        {
            var product = TourProduct.CreateExperience(
                $"t8{Guid.NewGuid():N}"[..20],
                "T008 Public Booking Seed",
                now);
            var departure = TourDeparture.Create(product, now);
            departure.SetCapacity(1, maxPax, now);
            if (publish)
            {
                departure.SetStatus(TourDepartureStatus.Published, now);
            }

            tour.TourProducts.Add(product);
            tour.TourDepartures.Add(departure);
            await tour.SaveChangesAsync(ct);
            productId = product.Id.Value;
            departureId = departure.Id.Value;
        }

        if (withPrice)
        {
            await using var pricing = _fixture.CreatePricingDb();
            var price = Price.Create(
                PriceTargetType.TourDepartureValue,
                departureId,
                [
                    new PriceComponentDefinition(
                        PriceComponentKind.Base,
                        PricingMoney.Create(1000m, "USD"),
                        SortOrder: 0,
                        Code: "BASE")
                ]);
            pricing.Prices.Add(price);
            await pricing.SaveChangesAsync(ct);
        }

        return (departureId, productId);
    }

    private static object Body(
        Guid departureId,
        string idempotencyKey,
        int passengerCount,
        string? sourceKind = null)
    {
        var passengers = Enumerable.Range(0, passengerCount)
            .Select(i => new
            {
                givenName = $"Given{i}",
                familyName = "Family",
                category = "Adult"
            })
            .ToArray();
        return new
        {
            tourDepartureId = departureId,
            contact = new { displayName = "Booker", email = "booker@travelcore.test", phone = "+15550001" },
            passengers,
            idempotencyKey,
            sourceKind
        };
    }

    private static Task<HttpResponseMessage> PostInitiationAsync(
        HttpClient client,
        Guid departureId,
        string idempotencyKey,
        int passengerCount,
        CancellationToken ct) =>
        client.PostAsJsonAsync(
            "/api/booking/public/initiations",
            Body(departureId, idempotencyKey, passengerCount),
            ct);

    private static async Task RegisterAndLoginAsync(
        HttpClient client,
        string email,
        string password,
        CancellationToken ct)
    {
        var created = await client.PostAsJsonAsync(
            "/api/identity/accounts/",
            new CreateAccountRequest { Email = email, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var login = await client.PostAsJsonAsync(
            "/api/identity/login",
            new LoginRequest { Email = email, Password = password },
            ct);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }
}
