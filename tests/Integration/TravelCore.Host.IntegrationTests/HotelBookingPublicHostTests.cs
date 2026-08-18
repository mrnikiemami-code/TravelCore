using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.HotelBooking.Infrastructure;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Place.Domain;
using TravelCore.Modules.Pricing.Domain;
using TravelCore.Modules.Tour.Domain;
using Xunit;
using MoneyValue = TravelCore.Money.Money;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class HotelBookingPublicHostTests
{
    private const string HotelHeader = "X-TravelCore-Hotel-Booking-Access-Token";
    private const string TourHeader = "X-TravelCore-Booking-Access-Token";
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);

    private readonly IdentityAuthHostFixture _fixture;

    public HotelBookingPublicHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Public_Journey_Is_Token_Protected_Idempotent_And_Non_Enumerating()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var placeId = await SeedActiveHotelPlaceAsync(ct);

        var idempotency = Guid.NewGuid().ToString("D");
        using var first = await PostInitiationAsync(client, placeId, idempotency, ct);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        using var firstDoc = JsonDocument.Parse(await first.Content.ReadAsStringAsync(ct));
        var hotelBookingId = firstDoc.RootElement.GetProperty("hotelBookingId").GetGuid();
        var token = firstDoc.RootElement.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(firstDoc.RootElement.GetProperty("accessTokenIssued").GetBoolean());
        Assert.Equal("Pending", firstDoc.RootElement.GetProperty("status").GetString());
        Assert.Equal("NeedsAvailability", firstDoc.RootElement.GetProperty("presentationState").GetString());
        Assert.False(firstDoc.RootElement.GetProperty("confirmed").GetBoolean());

        using var replay = await PostInitiationAsync(client, placeId, idempotency, ct);
        Assert.Equal(HttpStatusCode.Created, replay.StatusCode);
        using var replayDoc = JsonDocument.Parse(await replay.Content.ReadAsStringAsync(ct));
        Assert.Equal(hotelBookingId, replayDoc.RootElement.GetProperty("hotelBookingId").GetGuid());
        Assert.False(replayDoc.RootElement.GetProperty("accessTokenIssued").GetBoolean());
        Assert.True(replayDoc.RootElement.GetProperty("accessToken").ValueKind is JsonValueKind.Null or JsonValueKind.Undefined);

        var path = $"/api/hotel-booking/public/{hotelBookingId:D}";
        using var missing = await client.GetAsync(path, ct);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        using var wrongReq = new HttpRequestMessage(HttpMethod.Get, path);
        wrongReq.Headers.Add(HotelHeader, "not-the-token");
        using var wrong = await client.SendAsync(wrongReq, ct);
        Assert.Equal(HttpStatusCode.NotFound, wrong.StatusCode);

        using var okReq = new HttpRequestMessage(HttpMethod.Get, path);
        okReq.Headers.Add(HotelHeader, token);
        using var ok = await client.SendAsync(okReq, ct);
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        using var okDoc = JsonDocument.Parse(await ok.Content.ReadAsStringAsync(ct));
        Assert.Equal("Pending", okDoc.RootElement.GetProperty("status").GetString());
        Assert.False(okDoc.RootElement.GetProperty("confirmed").GetBoolean());
        Assert.Equal(1, okDoc.RootElement.GetProperty("rooms").GetArrayLength());

        using var list = await client.GetAsync("/api/hotel-booking/public", ct);
        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        using var bookings = await client.GetAsync("/api/hotel-bookings", ct);
        Assert.Equal(HttpStatusCode.NotFound, bookings.StatusCode);
        using var refund = await client.PostAsync(path + "/payment/refund", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, refund.StatusCode);
        using var put = await client.PutAsJsonAsync(path, new { status = "Confirmed" }, ct);
        Assert.True(
            put.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);

        using var clientA = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var clientB = factory.CreateClient(new() { AllowAutoRedirect = false });
        var concurrentKey = Guid.NewGuid().ToString("D");
        var concurrent = await Task.WhenAll(
            PostInitiationAsync(clientA, placeId, concurrentKey, ct),
            PostInitiationAsync(clientB, placeId, concurrentKey, ct));
        Assert.All(concurrent, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));
        using var aDoc = JsonDocument.Parse(await concurrent[0].Content.ReadAsStringAsync(ct));
        using var bDoc = JsonDocument.Parse(await concurrent[1].Content.ReadAsStringAsync(ct));
        Assert.Equal(
            aDoc.RootElement.GetProperty("hotelBookingId").GetGuid(),
            bDoc.RootElement.GetProperty("hotelBookingId").GetGuid());
        concurrent[0].Dispose();
        concurrent[1].Dispose();
    }

    [Fact]
    public async Task Tokens_Are_Independent_And_Ids_Are_Not_Credentials()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var placeId = await SeedActiveHotelPlaceAsync(ct);
        var tour = await SeedPublishedDepartureAsync(ct);

        using var hotelCreated = await PostInitiationAsync(client, placeId, Guid.NewGuid().ToString("D"), ct);
        using var hotelDoc = JsonDocument.Parse(await hotelCreated.Content.ReadAsStringAsync(ct));
        var hotelBookingId = hotelDoc.RootElement.GetProperty("hotelBookingId").GetGuid();
        var hotelToken = hotelDoc.RootElement.GetProperty("accessToken").GetString();

        using var tourCreated = await client.PostAsJsonAsync(
            "/api/booking/public/initiations",
            new
            {
                tourDepartureId = tour,
                contact = new { displayName = "Booker", email = "booker@travelcore.test", phone = "+15550001" },
                passengers = new[] { new { givenName = "Ada", familyName = "Lovelace", category = "Adult" } },
                idempotencyKey = Guid.NewGuid().ToString("D"),
            },
            ct);
        Assert.Equal(HttpStatusCode.Created, tourCreated.StatusCode);
        using var tourDoc = JsonDocument.Parse(await tourCreated.Content.ReadAsStringAsync(ct));
        var bookingId = tourDoc.RootElement.GetProperty("bookingId").GetGuid();
        var tourToken = tourDoc.RootElement.GetProperty("accessToken").GetString();

        var hotelPath = $"/api/hotel-booking/public/{hotelBookingId:D}";
        var tourPath = $"/api/booking/public/{bookingId:D}";

        using var tourTokenOnHotel = new HttpRequestMessage(HttpMethod.Get, hotelPath);
        tourTokenOnHotel.Headers.Add(HotelHeader, tourToken);
        using var tourOnHotel = await client.SendAsync(tourTokenOnHotel, ct);
        Assert.Equal(HttpStatusCode.NotFound, tourOnHotel.StatusCode);

        using var hotelTokenOnTour = new HttpRequestMessage(HttpMethod.Get, tourPath);
        hotelTokenOnTour.Headers.Add(TourHeader, hotelToken);
        using var hotelOnTour = await client.SendAsync(hotelTokenOnTour, ct);
        Assert.Equal(HttpStatusCode.NotFound, hotelOnTour.StatusCode);

        using var hotelTokenViaTourHeader = new HttpRequestMessage(HttpMethod.Get, hotelPath);
        hotelTokenViaTourHeader.Headers.Add(TourHeader, hotelToken);
        using var wrongHeader = await client.SendAsync(hotelTokenViaTourHeader, ct);
        Assert.Equal(HttpStatusCode.NotFound, wrongHeader.StatusCode);

        using var hotelOk = new HttpRequestMessage(HttpMethod.Get, hotelPath);
        hotelOk.Headers.Add(HotelHeader, hotelToken);
        using var hotelOkRes = await client.SendAsync(hotelOk, ct);
        Assert.Equal(HttpStatusCode.OK, hotelOkRes.StatusCode);

        using var owner = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var stranger = factory.CreateClient(new() { AllowAutoRedirect = false });
        await RegisterAndLoginAsync(owner, $"t008-owner-{Guid.NewGuid():N}@travelcore.test", "Owner-Password-1", ct);
        await RegisterAndLoginAsync(stranger, $"t008-other-{Guid.NewGuid():N}@travelcore.test", "Other-Password-1", ct);
        using var owned = await PostInitiationAsync(owner, placeId, Guid.NewGuid().ToString("D"), ct);
        using var ownedDoc = JsonDocument.Parse(await owned.Content.ReadAsStringAsync(ct));
        var ownedId = ownedDoc.RootElement.GetProperty("hotelBookingId").GetGuid();
        using var ownerRead = await owner.GetAsync($"/api/hotel-booking/public/{ownedId:D}", ct);
        Assert.Equal(HttpStatusCode.OK, ownerRead.StatusCode);
        using var strangerRead = await stranger.GetAsync($"/api/hotel-booking/public/{ownedId:D}", ct);
        Assert.Equal(HttpStatusCode.NotFound, strangerRead.StatusCode);
    }

    [Fact]
    public async Task Zero_Sources_Are_Truthful_And_Payment_Ignores_Client_Tamper()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var placeId = await SeedActiveHotelPlaceAsync(ct);
        using var created = await PostInitiationAsync(client, placeId, Guid.NewGuid().ToString("D"), ct);
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync(ct));
        var hotelBookingId = createdDoc.RootElement.GetProperty("hotelBookingId").GetGuid();
        var token = createdDoc.RootElement.GetProperty("accessToken").GetString();
        var basePath = $"/api/hotel-booking/public/{hotelBookingId:D}";

        using var availability = await SendHotelAsync(client, HttpMethod.Post, basePath + "/availability", token, null, ct);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, availability.StatusCode);
        using var rate = await SendHotelAsync(client, HttpMethod.Post, basePath + "/rate-offers", token, null, ct);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, rate.StatusCode);

        await using (var db = _fixture.CreateHotelBookingDb())
        {
            var holds = await db.HotelAvailabilityHolds.CountAsync(x => x.HotelBookingId == HotelBookingId.From(hotelBookingId), ct);
            var snapshots = await db.HotelRateOfferSnapshots.CountAsync(x => x.HotelBookingId == HotelBookingId.From(hotelBookingId), ct);
            Assert.Equal(0, holds);
            Assert.Equal(0, snapshots);
        }

        await SeedActiveHoldAndRateAsync(hotelBookingId, ct);

        using var paymentGet = await SendHotelAsync(client, HttpMethod.Get, basePath + "/payment", token, null, ct);
        Assert.Equal(HttpStatusCode.OK, paymentGet.StatusCode);
        using var paymentDoc = JsonDocument.Parse(await paymentGet.Content.ReadAsStringAsync(ct));
        Assert.Equal("Pending", paymentDoc.RootElement.GetProperty("paymentStatus").GetString());
        Assert.False(paymentDoc.RootElement.GetProperty("hotelBookingConfirmed").GetBoolean());
        Assert.False(paymentDoc.RootElement.GetProperty("providerInitiationPossible").GetBoolean());
        Assert.Equal("Unavailable", paymentDoc.RootElement.GetProperty("safeAction").GetString());
        Assert.True(
            paymentDoc.RootElement.TryGetProperty("redirectUri", out var redirect)
            && redirect.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined);
        var paymentId = paymentDoc.RootElement.GetProperty("paymentId").GetGuid();
        Assert.Equal(1_000_000m, paymentDoc.RootElement.GetProperty("amount").GetDecimal());
        Assert.Equal("IRR", paymentDoc.RootElement.GetProperty("currencyCode").GetString());

        using var tamper = new HttpRequestMessage(HttpMethod.Post, basePath + "/payment/initiation")
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
        tamper.Headers.Add(HotelHeader, token!);
        tamper.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("D"));
        using var tamperResponse = await client.SendAsync(tamper, ct);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, tamperResponse.StatusCode);

        using var after = await SendHotelAsync(client, HttpMethod.Get, basePath + "/payment", token, null, ct);
        using var afterDoc = JsonDocument.Parse(await after.Content.ReadAsStringAsync(ct));
        Assert.Equal(paymentId, afterDoc.RootElement.GetProperty("paymentId").GetGuid());
        Assert.Equal(1_000_000m, afterDoc.RootElement.GetProperty("amount").GetDecimal());
        Assert.Equal("IRR", afterDoc.RootElement.GetProperty("currencyCode").GetString());
        Assert.Equal("Pending", afterDoc.RootElement.GetProperty("paymentStatus").GetString());
        Assert.True(
            afterDoc.RootElement.TryGetProperty("redirectUri", out var afterRedirect)
            && afterRedirect.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined);

        using var paymentLookup = await client.GetAsync($"/api/payment/{paymentId:D}", ct);
        Assert.Equal(HttpStatusCode.NotFound, paymentLookup.StatusCode);
        using var paymentAsBooking = await SendHotelAsync(
            client, HttpMethod.Get, $"/api/hotel-booking/public/{paymentId:D}", token, null, ct);
        Assert.Equal(HttpStatusCode.NotFound, paymentAsBooking.StatusCode);

        await SeedSucceededPaymentAsync(hotelBookingId, paymentId, ct);
        using var paid = await SendHotelAsync(client, HttpMethod.Get, basePath, token, null, ct);
        using var paidDoc = JsonDocument.Parse(await paid.Content.ReadAsStringAsync(ct));
        Assert.Equal("PaymentReceived", paidDoc.RootElement.GetProperty("presentationState").GetString());
        Assert.False(paidDoc.RootElement.GetProperty("confirmed").GetBoolean());
        Assert.Equal("Pending", paidDoc.RootElement.GetProperty("status").GetString());
        Assert.Equal("Succeeded", paidDoc.RootElement.GetProperty("paymentStatus").GetString());

        using var ops = await SendHotelAsync(
            client, HttpMethod.Get, $"/api/hotel-booking/ops/{hotelBookingId:D}", token, null, ct);
        Assert.Equal(HttpStatusCode.NotFound, ops.StatusCode);
        using var admin = await SendHotelAsync(
            client, HttpMethod.Get, $"/api/admin/hotel-bookings/{hotelBookingId:D}", token, null, ct);
        Assert.Equal(HttpStatusCode.NotFound, admin.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var query = scope.ServiceProvider.GetRequiredService<IHotelBookingOperationalQuery>();
            var read = await query.GetByHotelBookingIdAsync(hotelBookingId, ct);
            Assert.NotNull(read);
            Assert.Null(read!.GetType().GetProperty("AccessToken"));
        }
    }

    [Fact]
    public async Task Partial_Penalty_Cancel_Is_Blocked_And_Timeout_Presents_Pending()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var partial = await SeedConfirmedStayAsync(partialPenalty: true, ct);
        using var cancel = await SendHotelAsync(
            client,
            HttpMethod.Post,
            $"/api/hotel-booking/public/{partial.BookingId:D}/cancellation",
            partial.Token,
            new { idempotencyKey = Guid.NewGuid().ToString("D") },
            ct);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, cancel.StatusCode);
        await using (var db = _fixture.CreateHotelBookingDb())
        {
            var id = HotelBookingId.From(partial.BookingId);
            Assert.Equal(HotelBookingStatus.Confirmed, (await db.HotelBookings.SingleAsync(x => x.Id == id, ct)).Status);
            Assert.Equal(0, await db.HotelBookingCancellations.CountAsync(x => x.HotelBookingId == id, ct));
            Assert.Equal(
                HotelSupplierReservationStatus.Confirmed,
                (await db.HotelSupplierReservations.SingleAsync(x => x.HotelBookingId == id, ct)).Status);
        }

        var timeout = await SeedConfirmedStayAsync(partialPenalty: false, ct);
        await using (var db = _fixture.CreateHotelBookingDb())
        {
            var id = HotelBookingId.From(timeout.BookingId);
            var cancellation = HotelBookingCancellation.StartRequested(
                id,
                Guid.CreateVersion7(),
                T0,
                HotelCancellationPenaltyEvaluation.FullRefund(new MoneyValue(1_000_000m, "IRR")));
            cancellation.StartAttempt(T0);
            db.HotelBookingCancellations.Add(cancellation);
            await db.SaveChangesAsync(ct);
        }

        using var pending = await SendHotelAsync(
            client,
            HttpMethod.Get,
            $"/api/hotel-booking/public/{timeout.BookingId:D}",
            timeout.Token,
            null,
            ct);
        Assert.Equal(HttpStatusCode.OK, pending.StatusCode);
        using var pendingDoc = JsonDocument.Parse(await pending.Content.ReadAsStringAsync(ct));
        Assert.Equal("Confirmed", pendingDoc.RootElement.GetProperty("status").GetString());
        Assert.Equal("CancellationPending", pendingDoc.RootElement.GetProperty("presentationState").GetString());
        Assert.NotEqual("Cancelled", pendingDoc.RootElement.GetProperty("presentationState").GetString());
        Assert.True(pendingDoc.RootElement.GetProperty("confirmed").GetBoolean());
    }

    private async Task<Guid> SeedActiveHotelPlaceAsync(CancellationToken ct)
    {
        await using var db = _fixture.CreatePlaceDb();
        var place = Place.CreateHotel($"htl{Guid.NewGuid():N}"[..20], "T008 Public Hotel", T0, starRating: 4);
        place.SetCatalogStatus(PlaceCatalogStatus.Active, T0);
        db.Places.Add(place);
        await db.SaveChangesAsync(ct);
        return place.Id.Value;
    }

    private async Task<Guid> SeedPublishedDepartureAsync(CancellationToken ct)
    {
        Guid departureId;
        await using (var tour = _fixture.CreateTourDb())
        {
            var product = TourProduct.CreateExperience($"t8{Guid.NewGuid():N}"[..20], "T008 Hotel vs Tour", T0);
            var departure = TourDeparture.Create(product, T0);
            departure.SetCapacity(1, 8, T0);
            departure.SetStatus(TourDepartureStatus.Published, T0);
            tour.TourProducts.Add(product);
            tour.TourDepartures.Add(departure);
            await tour.SaveChangesAsync(ct);
            departureId = departure.Id.Value;
        }

        await using var pricing = _fixture.CreatePricingDb();
        pricing.Prices.Add(Price.Create(
            PriceTargetType.TourDepartureValue,
            departureId,
            [
                new PriceComponentDefinition(
                    PriceComponentKind.Base,
                    PricingMoney.Create(1000m, "USD"),
                    SortOrder: 0,
                    Code: "BASE"),
            ]));
        await pricing.SaveChangesAsync(ct);
        return departureId;
    }

    private async Task SeedActiveHoldAndRateAsync(Guid hotelBookingId, CancellationToken ct)
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        await using var db = _fixture.CreateHotelBookingDb();
        var id = HotelBookingId.From(hotelBookingId);
        var booking = await db.HotelBookings
            .Include(x => x.Rooms)
            .SingleAsync(x => x.Id == id, ct);
        var hold = HotelAvailabilityHold.StartRequested(
            booking.Id,
            "test-source",
            now,
            booking.Rooms.Select(r => r.Id).ToArray());
        hold.Activate(
            now,
            now.Plus(Duration.FromHours(2)),
            "hold-1",
            booking.Rooms.ToDictionary(r => r.Id, r => $"sel-{r.Ordinal}"));
        db.HotelAvailabilityHolds.Add(hold);
        db.HotelRateOfferSnapshots.Add(AcceptOffer(booking, partialPenalty: false, now));
        await db.SaveChangesAsync(ct);
    }

    private async Task SeedSucceededPaymentAsync(Guid hotelBookingId, Guid paymentId, CancellationToken ct)
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        await using var db = _fixture.CreatePaymentDb();
        var existing = await db.Payments
            .Include(x => x.Attempts)
            .Include(x => x.ExecutionSnapshot)
            .SingleAsync(x => x.Id == PaymentId.From(paymentId), ct);
        if (existing.Status == PaymentStatus.Succeeded)
        {
            return;
        }

        if (existing.ExecutionSnapshot is null)
        {
            existing.BindExecutionSnapshot(
                Guid.CreateVersion7(),
                new MoneyValue(1_000_000m, "IRR"),
                now);
        }

        var attempt = existing.Attempts.FirstOrDefault() ?? existing.CreateAttempt(now);
        if (attempt.Status == PaymentAttemptStatus.Created)
        {
            existing.RecordProviderInitiation(
                attempt.Id,
                now.Plus(Duration.FromSeconds(1)),
                new ProviderKey("test"),
                new ProviderRequestReference($"req-{hotelBookingId:N}"),
                new ProviderTransactionReference($"txn-{hotelBookingId:N}"));
        }

        existing.RecordAuthoritativeCollectionSuccess(attempt.Id, now.Plus(Duration.FromSeconds(2)));
        await db.SaveChangesAsync(ct);
    }

    private async Task<(Guid BookingId, string Token)> SeedConfirmedStayAsync(
        bool partialPenalty,
        CancellationToken ct)
    {
        var raw = HotelBookingAccessToken.CreateRaw();
        Guid bookingId;
        await using (var db = _fixture.CreateHotelBookingDb())
        {
            var booking = Stay.Create(
                new HotelPlaceReference(Guid.CreateVersion7()),
                new LocalDate(2026, 8, 18),
                new LocalDate(2026, 8, 20),
                HotelBookingContactSnapshot.Create(email: "lead@example.com"),
                [
                    new RoomReservationSpecification(
                    [
                        new HotelBookingGuestSpecification("Ada", "Lovelace", HotelGuestCategory.Adult, true),
                    ]),
                    new RoomReservationSpecification(
                    [
                        new HotelBookingGuestSpecification("Alan", "Turing", HotelGuestCategory.Adult, false),
                    ]),
                ]);
            var snapshot = AcceptOffer(booking, partialPenalty, T0);
            var reservation = HotelSupplierReservation.StartPending(booking.Id, "test-source", T0);
            var attempt = reservation.StartAttempt(T0);
            reservation.ConfirmAttempt(
                attempt.Id,
                T0.Plus(Duration.FromMinutes(1)),
                $"src-res-{booking.Id.Value:N}",
                "CNF-1",
                booking.Rooms.Select(r => r.Id).ToArray(),
                booking.Rooms.Select(r => r.Id).ToArray());
            var paymentId = Guid.CreateVersion7();
            var evidence = HotelBookingPaymentEvidence.Record(booking.Id, paymentId, 1_000_000m, "IRR", T0);
            booking.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
                reservation,
                evidence,
                T0.Plus(Duration.FromMinutes(2)),
                booking.Place,
                booking.CheckInDate,
                booking.CheckOutDate,
                booking.Rooms.Select(r => r.Id).ToArray(),
                snapshot.Monetary.Total,
                true,
                snapshot.Monetary,
                []);
            db.HotelBookings.Add(booking);
            db.HotelRateOfferSnapshots.Add(snapshot);
            db.HotelSupplierReservations.Add(reservation);
            db.HotelBookingPaymentEvidence.Add(evidence);
            db.AccessCredentials.Add(
                HotelBookingAccessCredential.Create(booking.Id, HotelBookingAccessToken.Hash(raw), T0));
            await db.SaveChangesAsync(ct);
            bookingId = booking.Id.Value;
        }

        return (bookingId, raw);
    }

    private static HotelRateOfferSnapshot AcceptOffer(Stay booking, bool partialPenalty, Instant now)
    {
        var irr = CurrencyCode.Parse("IRR");
        var rooms = booking.Rooms.OrderBy(r => r.Ordinal).ToArray();
        IReadOnlyList<HotelRoomRateLine> lines = rooms.Length == 1
            ?
            [
                new HotelRoomRateLine(rooms[0].Id, new MoneyValue(1_000_000m, irr), "sel-1", "rate-1", "BB"),
            ]
            :
            [
                new HotelRoomRateLine(rooms[0].Id, new MoneyValue(400_000m, irr), "sel-1", "rate-1", "BB"),
                new HotelRoomRateLine(rooms[1].Id, new MoneyValue(600_000m, irr), "sel-2", "rate-2", "BB"),
            ];
        IReadOnlyList<HotelCancellationPenaltyRuleDraft> rules = partialPenalty
            ?
            [
                new HotelCancellationPenaltyRuleDraft(now, null, new MoneyValue(200_000m, irr)),
            ]
            :
            [
                new HotelCancellationPenaltyRuleDraft(now, now.Plus(Duration.FromDays(1)), new MoneyValue(0m, irr)),
                new HotelCancellationPenaltyRuleDraft(now.Plus(Duration.FromDays(1)), null, new MoneyValue(1_000_000m, irr)),
            ];
        return HotelRateOfferSnapshot.Accept(
            booking,
            now,
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            "test-source",
            $"offer-{booking.Id.Value:N}",
            now.Minus(Duration.FromMinutes(1)),
            now.Plus(Duration.FromHours(2)),
            new MoneyValue(1_000_000m, irr),
            lines,
            rules,
            propertyTimeZoneId: "Asia/Tehran");
    }

    private static async Task<HttpResponseMessage> PostInitiationAsync(
        HttpClient client,
        Guid placeId,
        string idempotencyKey,
        CancellationToken ct) =>
        await client.PostAsJsonAsync(
            "/api/hotel-booking/public/initiations",
            new
            {
                placeId,
                checkInDate = "2026-09-01",
                checkOutDate = "2026-09-03",
                contact = new { email = "lead@travelcore.test", phone = "+15550001" },
                rooms = new[]
                {
                    new
                    {
                        guests = new[]
                        {
                            new
                            {
                                givenName = "Ada",
                                familyName = "Lovelace",
                                category = "Adult",
                                isLeadGuest = true,
                                ageAtCheckInYears = (int?)null,
                            },
                        },
                    },
                },
                idempotencyKey,
            },
            ct);

    private static async Task<HttpResponseMessage> SendHotelAsync(
        HttpClient client,
        HttpMethod method,
        string path,
        string? token,
        object? body,
        CancellationToken ct)
    {
        var req = new HttpRequestMessage(method, path);
        if (!string.IsNullOrWhiteSpace(token))
        {
            req.Headers.Add(HotelHeader, token);
        }

        if (body is not null)
        {
            req.Content = JsonContent.Create(body);
        }

        return await client.SendAsync(req, ct);
    }

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
