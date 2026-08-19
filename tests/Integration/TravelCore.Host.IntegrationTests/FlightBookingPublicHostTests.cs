using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Flight.Infrastructure;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Pricing.Domain;
using TravelCore.Modules.Tour.Domain;
using Xunit;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class FlightBookingPublicHostTests
{
    private const string FlightHeader = "X-TravelCore-Flight-Booking-Access-Token";
    private const string HotelHeader = "X-TravelCore-Hotel-Booking-Access-Token";
    private const string TourHeader = "X-TravelCore-Booking-Access-Token";
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly Instant Dep = Instant.FromUtc(2026, 9, 1, 6, 0);
    private static readonly Instant Arr = Instant.FromUtc(2026, 9, 1, 10, 0);

    private readonly IdentityAuthHostFixture _fixture;

    public FlightBookingPublicHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Public_Journey_Is_Token_Protected_Idempotent_And_Non_Enumerating()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        using var search = await client.PostAsJsonAsync(
            "/api/flight-booking/public/search",
            new
            {
                originIata = "IKA",
                destinationIata = "IST",
                tripType = "OneWay",
                departureDate = "2026-09-01",
                returnDate = (string?)null,
                adultCount = 1,
                childCount = 0,
                infantCount = 0,
            },
            ct);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, search.StatusCode);

        var idempotency = Guid.NewGuid().ToString("D");
        using var first = await PostInitiationAsync(client, idempotency, ct);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        using var firstDoc = JsonDocument.Parse(await first.Content.ReadAsStringAsync(ct));
        var flightBookingId = firstDoc.RootElement.GetProperty("flightBookingId").GetGuid();
        var token = firstDoc.RootElement.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(firstDoc.RootElement.GetProperty("accessTokenIssued").GetBoolean());
        Assert.Equal("Pending", firstDoc.RootElement.GetProperty("status").GetString());
        Assert.Equal("NeedsOffer", firstDoc.RootElement.GetProperty("presentationState").GetString());
        Assert.False(firstDoc.RootElement.GetProperty("confirmed").GetBoolean());

        using var replay = await PostInitiationAsync(client, idempotency, ct);
        Assert.Equal(HttpStatusCode.Created, replay.StatusCode);
        using var replayDoc = JsonDocument.Parse(await replay.Content.ReadAsStringAsync(ct));
        Assert.Equal(flightBookingId, replayDoc.RootElement.GetProperty("flightBookingId").GetGuid());
        Assert.False(replayDoc.RootElement.GetProperty("accessTokenIssued").GetBoolean());
        Assert.True(replayDoc.RootElement.GetProperty("accessToken").ValueKind is JsonValueKind.Null or JsonValueKind.Undefined);

        var path = $"/api/flight-booking/public/{flightBookingId:D}";
        using var missing = await client.GetAsync(path, ct);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        using var wrongReq = new HttpRequestMessage(HttpMethod.Get, path);
        wrongReq.Headers.Add(FlightHeader, "not-the-token");
        using var wrong = await client.SendAsync(wrongReq, ct);
        Assert.Equal(HttpStatusCode.NotFound, wrong.StatusCode);

        using var okReq = new HttpRequestMessage(HttpMethod.Get, path);
        okReq.Headers.Add(FlightHeader, token);
        using var ok = await client.SendAsync(okReq, ct);
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        using var okDoc = JsonDocument.Parse(await ok.Content.ReadAsStringAsync(ct));
        Assert.Equal("Pending", okDoc.RootElement.GetProperty("status").GetString());
        Assert.False(okDoc.RootElement.GetProperty("confirmed").GetBoolean());
        Assert.Equal(1, okDoc.RootElement.GetProperty("passengers").GetArrayLength());
        Assert.False(okDoc.RootElement.GetProperty("passengers")[0].TryGetProperty("birthDate", out _));
        Assert.False(okDoc.RootElement.GetProperty("passengers")[0].TryGetProperty("passport", out _));

        using var list = await client.GetAsync("/api/flight-booking/public", ct);
        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        using var bookings = await client.GetAsync("/api/flight-bookings", ct);
        Assert.Equal(HttpStatusCode.NotFound, bookings.StatusCode);
        using var refund = await client.PostAsync(path + "/payment/refund", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, refund.StatusCode);
        using var put = await client.PutAsJsonAsync(path, new { status = "Confirmed" }, ct);
        Assert.True(put.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);

        using var offer = await SendFlightAsync(client, HttpMethod.Post, path + "/offers", token, null, ct);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, offer.StatusCode);
        using var reservation = await SendFlightAsync(client, HttpMethod.Post, path + "/reservations", token, null, ct);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, reservation.StatusCode);

        using var clientA = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var clientB = factory.CreateClient(new() { AllowAutoRedirect = false });
        var concurrentKey = Guid.NewGuid().ToString("D");
        var concurrent = await Task.WhenAll(
            PostInitiationAsync(clientA, concurrentKey, ct),
            PostInitiationAsync(clientB, concurrentKey, ct));
        Assert.All(concurrent, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));
        using var aDoc = JsonDocument.Parse(await concurrent[0].Content.ReadAsStringAsync(ct));
        using var bDoc = JsonDocument.Parse(await concurrent[1].Content.ReadAsStringAsync(ct));
        Assert.Equal(
            aDoc.RootElement.GetProperty("flightBookingId").GetGuid(),
            bDoc.RootElement.GetProperty("flightBookingId").GetGuid());
        concurrent[0].Dispose();
        concurrent[1].Dispose();
    }

    [Fact]
    public async Task Tokens_Are_Independent_And_Ids_Are_Not_Credentials()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        using var flightCreated = await PostInitiationAsync(client, Guid.NewGuid().ToString("D"), ct);
        using var flightDoc = JsonDocument.Parse(await flightCreated.Content.ReadAsStringAsync(ct));
        var flightBookingId = flightDoc.RootElement.GetProperty("flightBookingId").GetGuid();
        var flightToken = flightDoc.RootElement.GetProperty("accessToken").GetString();

        var tour = await SeedPublishedDepartureAsync(ct);
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

        var flightPath = $"/api/flight-booking/public/{flightBookingId:D}";
        var tourPath = $"/api/booking/public/{bookingId:D}";

        using var tourTokenOnFlight = new HttpRequestMessage(HttpMethod.Get, flightPath);
        tourTokenOnFlight.Headers.Add(FlightHeader, tourToken);
        using var tourOnFlight = await client.SendAsync(tourTokenOnFlight, ct);
        Assert.Equal(HttpStatusCode.NotFound, tourOnFlight.StatusCode);

        using var hotelTokenOnFlight = new HttpRequestMessage(HttpMethod.Get, flightPath);
        hotelTokenOnFlight.Headers.Add(FlightHeader, "hotel-shaped-token");
        using var hotelOnFlight = await client.SendAsync(hotelTokenOnFlight, ct);
        Assert.Equal(HttpStatusCode.NotFound, hotelOnFlight.StatusCode);

        using var flightTokenOnTour = new HttpRequestMessage(HttpMethod.Get, tourPath);
        flightTokenOnTour.Headers.Add(TourHeader, flightToken);
        using var flightOnTour = await client.SendAsync(flightTokenOnTour, ct);
        Assert.Equal(HttpStatusCode.NotFound, flightOnTour.StatusCode);

        using var flightViaTourHeader = new HttpRequestMessage(HttpMethod.Get, flightPath);
        flightViaTourHeader.Headers.Add(TourHeader, flightToken);
        using var wrongHeader = await client.SendAsync(flightViaTourHeader, ct);
        Assert.Equal(HttpStatusCode.NotFound, wrongHeader.StatusCode);

        using var flightViaHotelHeader = new HttpRequestMessage(HttpMethod.Get, flightPath);
        flightViaHotelHeader.Headers.Add(HotelHeader, flightToken);
        using var hotelHeader = await client.SendAsync(flightViaHotelHeader, ct);
        Assert.Equal(HttpStatusCode.NotFound, hotelHeader.StatusCode);

        using var flightOk = new HttpRequestMessage(HttpMethod.Get, flightPath);
        flightOk.Headers.Add(FlightHeader, flightToken);
        using var flightOkRes = await client.SendAsync(flightOk, ct);
        Assert.Equal(HttpStatusCode.OK, flightOkRes.StatusCode);

        using var owner = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var stranger = factory.CreateClient(new() { AllowAutoRedirect = false });
        await RegisterAndLoginAsync(owner, $"p22-owner-{Guid.NewGuid():N}@travelcore.test", "Owner-Password-1", ct);
        await RegisterAndLoginAsync(stranger, $"p22-other-{Guid.NewGuid():N}@travelcore.test", "Other-Password-1", ct);
        using var owned = await PostInitiationAsync(owner, Guid.NewGuid().ToString("D"), ct);
        using var ownedDoc = JsonDocument.Parse(await owned.Content.ReadAsStringAsync(ct));
        var ownedId = ownedDoc.RootElement.GetProperty("flightBookingId").GetGuid();
        using var ownerRead = await owner.GetAsync($"/api/flight-booking/public/{ownedId:D}", ct);
        Assert.Equal(HttpStatusCode.OK, ownerRead.StatusCode);
        using var strangerRead = await stranger.GetAsync($"/api/flight-booking/public/{ownedId:D}", ct);
        Assert.Equal(HttpStatusCode.NotFound, strangerRead.StatusCode);
    }

    [Fact]
    public async Task Payment_Ignores_Client_Tamper_And_Provider_Is_Unavailable()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var created = await PostInitiationAsync(client, Guid.NewGuid().ToString("D"), ct);
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync(ct));
        var flightBookingId = createdDoc.RootElement.GetProperty("flightBookingId").GetGuid();
        var token = createdDoc.RootElement.GetProperty("accessToken").GetString();
        var basePath = $"/api/flight-booking/public/{flightBookingId:D}";

        await SeedOfferAndConfirmedReservationAsync(flightBookingId, ct);

        using var paymentGet = await SendFlightAsync(client, HttpMethod.Get, basePath + "/payment", token, null, ct);
        Assert.Equal(HttpStatusCode.OK, paymentGet.StatusCode);
        using var paymentDoc = JsonDocument.Parse(await paymentGet.Content.ReadAsStringAsync(ct));
        Assert.Equal("Pending", paymentDoc.RootElement.GetProperty("paymentStatus").GetString());
        Assert.False(paymentDoc.RootElement.GetProperty("flightBookingConfirmed").GetBoolean());
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
        tamper.Headers.Add(FlightHeader, token!);
        tamper.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("D"));
        using var tamperResponse = await client.SendAsync(tamper, ct);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, tamperResponse.StatusCode);

        using var after = await SendFlightAsync(client, HttpMethod.Get, basePath + "/payment", token, null, ct);
        using var afterDoc = JsonDocument.Parse(await after.Content.ReadAsStringAsync(ct));
        Assert.Equal(paymentId, afterDoc.RootElement.GetProperty("paymentId").GetGuid());
        Assert.Equal(1_000_000m, afterDoc.RootElement.GetProperty("amount").GetDecimal());
        Assert.Equal("Pending", afterDoc.RootElement.GetProperty("paymentStatus").GetString());

        using var paymentLookup = await client.GetAsync($"/api/payment/{paymentId:D}", ct);
        Assert.Equal(HttpStatusCode.NotFound, paymentLookup.StatusCode);
        using var paymentAsBooking = await SendFlightAsync(
            client, HttpMethod.Get, $"/api/flight-booking/public/{paymentId:D}", token, null, ct);
        Assert.Equal(HttpStatusCode.NotFound, paymentAsBooking.StatusCode);

        await SeedSucceededPaymentAsync(flightBookingId, paymentId, ct);
        using var paid = await SendFlightAsync(client, HttpMethod.Get, basePath, token, null, ct);
        using var paidDoc = JsonDocument.Parse(await paid.Content.ReadAsStringAsync(ct));
        Assert.Equal("TicketingPending", paidDoc.RootElement.GetProperty("presentationState").GetString());
        Assert.False(paidDoc.RootElement.GetProperty("confirmed").GetBoolean());
        Assert.Equal("Pending", paidDoc.RootElement.GetProperty("status").GetString());
        Assert.Equal("Succeeded", paidDoc.RootElement.GetProperty("paymentStatus").GetString());
        Assert.NotEqual("Confirmed", paidDoc.RootElement.GetProperty("presentationState").GetString());

        using var ops = await SendFlightAsync(
            client, HttpMethod.Get, $"/api/flight-booking/ops/{flightBookingId:D}", token, null, ct);
        Assert.Equal(HttpStatusCode.NotFound, ops.StatusCode);
        using var admin = await SendFlightAsync(
            client, HttpMethod.Get, $"/api/admin/flight-bookings/{flightBookingId:D}", token, null, ct);
        Assert.Equal(HttpStatusCode.NotFound, admin.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var query = scope.ServiceProvider.GetRequiredService<IFlightOperationalQuery>();
            var read = await query.GetByFlightBookingIdAsync(flightBookingId, ct);
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

        var partial = await SeedConfirmedFlightAsync(partialPenalty: true, ct);
        using var cancel = await SendFlightAsync(
            client,
            HttpMethod.Post,
            $"/api/flight-booking/public/{partial.BookingId:D}/cancellation",
            partial.Token,
            new { idempotencyKey = Guid.NewGuid().ToString("D") },
            ct);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, cancel.StatusCode);
        await using (var db = _fixture.CreateFlightDb())
        {
            var id = FlightBookingId.From(partial.BookingId);
            Assert.Equal(FlightBookingStatus.Confirmed, (await db.FlightBookings.SingleAsync(x => x.Id == id, ct)).Status);
            Assert.Equal(0, await db.FlightBookingCancellations.CountAsync(x => x.FlightBookingId == id, ct));
            Assert.Equal(
                FlightSupplierReservationStatus.Confirmed,
                (await db.FlightSupplierReservations.SingleAsync(x => x.FlightBookingId == id, ct)).Status);
        }

        var timeout = await SeedConfirmedFlightAsync(partialPenalty: false, ct);
        await using (var db = _fixture.CreateFlightDb())
        {
            var id = FlightBookingId.From(timeout.BookingId);
            var snapshot = await db.FlightOfferSnapshots
                .Include(x => x.Monetary)
                .Include(x => x.FareRules)
                .SingleAsync(x => x.FlightBookingId == id, ct);
            var cancellation = FlightBookingCancellation.StartRequested(
                id,
                Guid.CreateVersion7(),
                T0,
                FlightCancellationPenaltyEvaluation.FullRefund(new MoneyValue(1_000_000m, "IRR")));
            cancellation.StartAttempt(FlightSupplierReversalKind.ReservationCancel, T0);
            db.FlightBookingCancellations.Add(cancellation);
            await db.SaveChangesAsync(ct);
            _ = snapshot;
        }

        using var pending = await SendFlightAsync(
            client,
            HttpMethod.Get,
            $"/api/flight-booking/public/{timeout.BookingId:D}",
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

    private async Task SeedOfferAndConfirmedReservationAsync(Guid flightBookingId, CancellationToken ct)
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        await using var db = _fixture.CreateFlightDb();
        var id = FlightBookingId.From(flightBookingId);
        var booking = await db.FlightBookings
            .Include(x => x.Journeys)
            .ThenInclude(x => x.Segments)
            .Include(x => x.Passengers)
            .SingleAsync(x => x.Id == id, ct);
        db.FlightOfferSnapshots.Add(AcceptOffer(booking, partialPenalty: false, now));
        db.FlightSupplierReservations.Add(ConfirmedReservation(booking, now));
        await db.SaveChangesAsync(ct);
    }

    private async Task SeedSucceededPaymentAsync(Guid flightBookingId, Guid paymentId, CancellationToken ct)
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
                new ProviderRequestReference($"req-{flightBookingId:N}"),
                new ProviderTransactionReference($"txn-{flightBookingId:N}"));
        }

        existing.RecordAuthoritativeCollectionSuccess(attempt.Id, now.Plus(Duration.FromSeconds(2)));
        await db.SaveChangesAsync(ct);
    }

    private async Task<(Guid BookingId, string Token)> SeedConfirmedFlightAsync(
        bool partialPenalty,
        CancellationToken ct)
    {
        var raw = FlightBookingAccessToken.CreateRaw();
        Guid bookingId;
        var now = T0;
        await using (var db = _fixture.CreateFlightDb())
        {
            var booking = OneWayBooking();
            var snapshot = AcceptOffer(booking, partialPenalty, now);
            var reservation = ConfirmedReservation(booking, now);
            var tickets = booking.Passengers
                .Select((p, i) =>
                {
                    var ticket = FlightTicket.StartPending(booking.Id, p.Id, "test-source", now);
                    ticket.MarkIssued($"125-{booking.Id.Value:N}-{i + 1:000}", now.Plus(Duration.FromMinutes(3)));
                    return ticket;
                })
                .ToArray();
            var paymentId = Guid.CreateVersion7();
            var evidence = FlightBookingPaymentEvidence.Record(
                booking.Id,
                paymentId,
                snapshot.Monetary.Total.Amount,
                snapshot.Monetary.Total.Currency.Value,
                now.Plus(Duration.FromMinutes(2)));
            booking.ConfirmFromAuthoritativeReservationPaymentAndTickets(
                reservation,
                evidence,
                tickets,
                snapshot.Monetary,
                [],
                now.Plus(Duration.FromMinutes(5)));
            db.FlightBookings.Add(booking);
            db.FlightOfferSnapshots.Add(snapshot);
            db.FlightSupplierReservations.Add(reservation);
            db.FlightTickets.AddRange(tickets);
            db.FlightBookingPaymentEvidence.Add(evidence);
            db.AccessCredentials.Add(
                FlightBookingAccessCredential.Create(booking.Id, FlightBookingAccessToken.Hash(raw), now));
            await db.SaveChangesAsync(ct);
            bookingId = booking.Id.Value;
        }

        return (bookingId, raw);
    }

    private async Task<Guid> SeedPublishedDepartureAsync(CancellationToken ct)
    {
        Guid departureId;
        await using (var tour = _fixture.CreateTourDb())
        {
            var product = TourProduct.CreateExperience($"f8{Guid.NewGuid():N}"[..20], "T008 Flight vs Tour", T0);
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

    private static FlightBookingAggregate OneWayBooking() =>
        FlightBookingAggregate.Create(
            FlightTripType.OneWay,
            [
                new FlightJourneySpecification(
                [
                    new FlightSegmentSpecification(
                        new AirportReference("THR"),
                        new AirportReference("LHR"),
                        Dep,
                        "Asia/Tehran",
                        Arr,
                        "Europe/London",
                        new AirlineReference("TK"),
                        null,
                        "TK800"),
                ]),
            ],
            [new FlightPassengerSpecification("Ada", "Lovelace", FlightPassengerCategory.Adult)]);

    private static FlightOfferSnapshot AcceptOffer(FlightBookingAggregate booking, bool partialPenalty, Instant now) =>
        FlightOfferSnapshot.Accept(
            booking,
            now,
            "test-source",
            $"offer-{booking.Id.Value:N}",
            now.Minus(Duration.FromMinutes(1)),
            now.Plus(Duration.FromHours(2)),
            Irr(800_000m),
            Irr(150_000m),
            Irr(50_000m),
            Irr(1_000_000m),
            Identities(booking),
            new FlightPassengerCount(
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Adult),
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Child),
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Infant)),
            new FlightFareRulesDraft(
                true,
                true,
                now.Plus(Duration.FromDays(1)),
                Irr(partialPenalty ? 100_000m : 0m),
                Irr(80_000m),
                false));

    private static FlightSupplierReservation ConfirmedReservation(FlightBookingAggregate booking, Instant now)
    {
        var reservation = FlightSupplierReservation.StartPending(booking.Id, "test-source", now);
        var attempt = reservation.StartAttempt(now);
        reservation.MarkAttemptInitiated(attempt.Id, now.Plus(Duration.FromSeconds(1)));
        reservation.ConfirmAttempt(
            attempt.Id,
            now.Plus(Duration.FromMinutes(1)),
            $"src-res-{booking.Id.Value:N}",
            "ABC123",
            now.Plus(Duration.FromDays(2)),
            Identities(booking),
            Identities(booking),
            Passengers(booking),
            Passengers(booking));
        return reservation;
    }

    private static IReadOnlyList<FlightOfferSegmentIdentity> Identities(FlightBookingAggregate booking) =>
        booking.Journeys
            .OrderBy(j => j.Ordinal)
            .SelectMany(j => j.Segments
                .OrderBy(s => s.Ordinal)
                .Select(s => new FlightOfferSegmentIdentity(
                    j.Ordinal,
                    s.Ordinal,
                    s.Origin,
                    s.Destination,
                    s.DepartureAt,
                    s.ArrivalAt,
                    s.MarketingCarrier,
                    s.OperatingCarrier,
                    s.FlightNumber)))
            .ToArray();

    private static IReadOnlyList<FlightReservationPassengerFact> Passengers(FlightBookingAggregate booking) =>
        booking.Passengers
            .OrderBy(p => p.Ordinal)
            .Select(p => new FlightReservationPassengerFact(p.GivenName, p.FamilyName, p.Category))
            .ToArray();

    private static MoneyValue Irr(decimal amount) => new(amount, CurrencyCode.Parse("IRR"));

    private static async Task<HttpResponseMessage> PostInitiationAsync(
        HttpClient client,
        string idempotencyKey,
        CancellationToken ct) =>
        await client.PostAsJsonAsync(
            "/api/flight-booking/public/initiations",
            new
            {
                tripType = "OneWay",
                journeys = new[]
                {
                    new
                    {
                        segments = new[]
                        {
                            new
                            {
                                originIata = "THR",
                                destinationIata = "LHR",
                                departureAt = "2026-09-01T06:00:00Z",
                                departureTimeZoneId = "Asia/Tehran",
                                arrivalAt = "2026-09-01T10:00:00Z",
                                arrivalTimeZoneId = "Europe/London",
                                marketingCarrierIata = "TK",
                                operatingCarrierIata = (string?)null,
                                flightNumber = "TK800",
                            },
                        },
                    },
                },
                passengers = new[]
                {
                    new { givenName = "Ada", familyName = "Lovelace", category = "Adult" },
                },
                idempotencyKey,
            },
            ct);

    private static async Task<HttpResponseMessage> SendFlightAsync(
        HttpClient client,
        HttpMethod method,
        string path,
        string? token,
        object? body,
        CancellationToken ct)
    {
        using var req = new HttpRequestMessage(method, path);
        if (!string.IsNullOrWhiteSpace(token))
        {
            req.Headers.Add(FlightHeader, token);
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
