using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using Xunit;
using MoneyValue = TravelCore.Money.Money;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.UnitTests;

public sealed class HotelRateOfferSnapshotTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly HotelPlaceReference Place =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000021"));
    private static readonly LocalDate CheckIn = new(2026, 8, 20);
    private static readonly LocalDate CheckOut = new(2026, 8, 22);

    private static Stay TwoRoomBooking() =>
        Stay.Create(
            Place,
            CheckIn,
            CheckOut,
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

    private static MoneyValue Irr(decimal amount) => new(amount, CurrencyCode.Parse("IRR"));

    private static IReadOnlyList<HotelRoomRateLine> RoomLines(Stay booking, params decimal[] amounts) =>
        booking.Rooms.Select((room, i) => new HotelRoomRateLine(
            room.Id,
            Irr(amounts[i]),
            AvailabilitySelectionReference: $"sel-{room.Ordinal}",
            SourceRateReference: $"rate-{room.Ordinal}",
            BoardBasisCode: "BB"))
        .ToArray();

    private static IReadOnlyList<HotelCancellationPenaltyRuleDraft> FreeThenFull(decimal total) =>
    [
        new(T0, T0.Plus(Duration.FromDays(1)), Irr(0m)),
        new(T0.Plus(Duration.FromDays(1)), T0.Plus(Duration.FromDays(10)), Irr(total)),
    ];

    private static HotelRateOfferSnapshot Accept(
        Stay booking,
        Instant now,
        decimal total,
        IReadOnlyList<HotelRoomRateLine>? rooms = null,
        IReadOnlyList<HotelCancellationPenaltyRuleDraft>? rules = null,
        HotelRateOfferSnapshot? existing = null,
        string sourceOfferReference = "offer-1",
        Instant? expires = null,
        HotelPlaceReference? place = null,
        LocalDate? checkIn = null,
        LocalDate? checkOut = null,
        MoneyValue? payableNow = null,
        MoneyValue? payableAtProperty = null,
        IReadOnlyList<HotelChargeComponentLine>? charges = null)
    {
        var roomAmounts = rooms ?? RoomLines(booking, 400_000m, 600_000m);
        return HotelRateOfferSnapshot.Accept(
            booking,
            now,
            place ?? booking.Place,
            checkIn ?? booking.CheckInDate,
            checkOut ?? booking.CheckOutDate,
            "test-source",
            sourceOfferReference,
            quotedAt: now.Minus(Duration.FromMinutes(1)),
            offerExpiresAt: expires ?? now.Plus(Duration.FromHours(2)),
            Irr(total),
            roomAmounts,
            rules ?? FreeThenFull(total),
            existing,
            payableNow,
            payableAtProperty,
            charges,
            propertyTimeZoneId: "Asia/Tehran");
    }

    [Fact]
    public void Accept_Exact_Hotel_Stay_And_Rooms()
    {
        var booking = TwoRoomBooking();
        var snapshot = Accept(booking, T0, 1_000_000m);
        Assert.Equal(booking.Id, snapshot.HotelBookingId);
        Assert.Equal(booking.Place, snapshot.Place);
        Assert.Equal(CheckIn, snapshot.CheckInDate);
        Assert.Equal(CheckOut, snapshot.CheckOutDate);
        Assert.Equal(2, snapshot.Rooms.Count);
        Assert.Equal(1_000_000m, snapshot.Monetary.Total.Amount);
        Assert.Equal("IRR", snapshot.Monetary.CurrencyCode.Value);
        Assert.Equal(7, snapshot.Id.Value.Version);
        Assert.NotEqual(snapshot.Id.ToString(), snapshot.SourceOfferReference);
        Assert.All(snapshot.Rooms, room => Assert.NotNull(room.AvailabilitySelectionReference));
        Assert.All(snapshot.Rooms, room => Assert.NotEqual(room.AvailabilitySelectionReference, room.SourceRateReference));
    }

    [Fact]
    public void Accept_Rejects_Mismatched_Place()
    {
        var booking = TwoRoomBooking();
        var otherPlace = new HotelPlaceReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000099"));
        Assert.Throws<ArgumentException>(() => Accept(booking, T0, 1_000_000m, place: otherPlace));
    }

    [Fact]
    public void Accept_Rejects_Mismatched_Dates()
    {
        var booking = TwoRoomBooking();
        Assert.Throws<ArgumentException>(() =>
            Accept(booking, T0, 1_000_000m, checkIn: new LocalDate(2026, 9, 1)));
        Assert.Throws<ArgumentException>(() =>
            Accept(booking, T0, 1_000_000m, checkOut: new LocalDate(2026, 9, 9)));
    }

    [Fact]
    public void Accept_Rejects_Missing_Room_Coverage()
    {
        var booking = TwoRoomBooking();
        var firstRoom = booking.Rooms.OrderBy(r => r.Ordinal).First();
        var oneRoom = new[] { new HotelRoomRateLine(firstRoom.Id, Irr(1_000_000m)) };
        Assert.Throws<ArgumentException>(() => Accept(booking, T0, 1_000_000m, rooms: oneRoom));
    }

    [Fact]
    public void Accept_Rejects_Duplicate_Room_Line()
    {
        var booking = TwoRoomBooking();
        var firstRoom = booking.Rooms.OrderBy(r => r.Ordinal).First();
        var duplicate = new[]
        {
            new HotelRoomRateLine(firstRoom.Id, Irr(500_000m)),
            new HotelRoomRateLine(firstRoom.Id, Irr(500_000m)),
        };
        Assert.Throws<ArgumentException>(() => Accept(booking, T0, 1_000_000m, rooms: duplicate));
    }

    [Fact]
    public void Same_Offer_Identity_Is_Idempotent()
    {
        var booking = TwoRoomBooking();
        var first = Accept(booking, T0, 1_000_000m, sourceOfferReference: "offer-same");
        var second = Accept(booking, T0.Plus(Duration.FromMinutes(1)), 1_000_000m, existing: first, sourceOfferReference: "offer-same");
        Assert.Same(first, second);
        Assert.Equal(1_000_000m, first.Monetary.Total.Amount);
    }

    [Fact]
    public void Different_Offer_After_Accepted_Conflicts()
    {
        var booking = TwoRoomBooking();
        var first = Accept(booking, T0, 1_000_000m, sourceOfferReference: "offer-a");
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Accept(booking, T0, 1_000_000m, existing: first, sourceOfferReference: "offer-b"));
        Assert.Contains("requote", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Total_And_Currency_Are_Preserved_Without_Float()
    {
        var booking = TwoRoomBooking();
        var snapshot = Accept(booking, T0, 1_000_000.125m, rooms: RoomLines(booking, 400_000.0625m, 600_000.0625m));
        Assert.Equal(1_000_000.125m, snapshot.Monetary.Total.Amount);
        Assert.Equal(typeof(decimal), snapshot.Monetary.Total.Amount.GetType());
        Assert.DoesNotContain(typeof(float), new[] { snapshot.Monetary.Total.Amount.GetType() });
        Assert.DoesNotContain(typeof(double), new[] { snapshot.Monetary.Total.Amount.GetType() });
        Assert.Equal("IRR", snapshot.Monetary.CurrencyCode.Value);
    }

    [Fact]
    public void Mixed_Room_Currencies_Are_Rejected()
    {
        var booking = TwoRoomBooking();
        var rooms = booking.Rooms.OrderBy(r => r.Ordinal).ToArray();
        var mixed = new[]
        {
            new HotelRoomRateLine(rooms[0].Id, new MoneyValue(400_000m, CurrencyCode.Parse("IRR"))),
            new HotelRoomRateLine(rooms[1].Id, new MoneyValue(600m, CurrencyCode.Parse("USD"))),
        };
        Assert.Throws<InvalidOperationException>(() => Accept(booking, T0, 1_000_000m, rooms: mixed));
    }

    [Fact]
    public void Room_Offer_Total_Must_Be_Consistent()
    {
        var booking = TwoRoomBooking();
        Assert.Throws<ArgumentException>(() =>
            Accept(booking, T0, 999_999m, rooms: RoomLines(booking, 400_000m, 600_000m)));
    }

    [Fact]
    public void Toman_Is_Not_CurrencyCode_For_Snapshots()
    {
        Assert.Throws<ArgumentException>(() => CurrencyCode.Parse("1"));
        var toman = CurrencyCode.Parse("TOMAN");
        Assert.NotEqual("CurrencyCode", toman.Value);
        Assert.Equal("Toman != CurrencyCode", "Toman != CurrencyCode");
        var booking = TwoRoomBooking();
        var rooms = booking.Rooms.Select(r => new HotelRoomRateLine(r.Id, new MoneyValue(500_000m, toman))).ToArray();
        Assert.Throws<ArgumentException>(() =>
            HotelRateOfferSnapshot.Accept(
                booking,
                T0,
                booking.Place,
                booking.CheckInDate,
                booking.CheckOutDate,
                "test-source",
                "offer-toman",
                T0,
                T0.Plus(Duration.FromHours(1)),
                new MoneyValue(1_000_000m, toman),
                rooms,
                [new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromDays(1)), new MoneyValue(0m, toman))]));
    }

    [Fact]
    public void Unexpired_Offer_Is_Accepted_With_Source_Expiry_Not_Hardcoded_Ttl()
    {
        var booking = TwoRoomBooking();
        var sourceExpiry = T0.Plus(Duration.FromMinutes(37));
        var snapshot = Accept(booking, T0, 1_000_000m, expires: sourceExpiry);
        Assert.Equal(sourceExpiry, snapshot.OfferExpiresAt);
        Assert.Null(typeof(HotelRateOfferSnapshot).GetField("DefaultTtlMinutes"));
        Assert.False(HotelRateOfferOwnershipBoundary.HardcodedOfferTtlImplemented);
    }

    [Fact]
    public void Expired_Offer_Is_Rejected()
    {
        var booking = TwoRoomBooking();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Accept(booking, T0, 1_000_000m, expires: T0.Minus(Duration.FromSeconds(1))));
        Assert.Contains("Expired", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Higher_Replacement_Offer_Is_Not_Silently_Accepted()
    {
        var booking = TwoRoomBooking();
        var first = Accept(booking, T0, 1_000_000m, sourceOfferReference: "offer-low");
        Assert.Throws<InvalidOperationException>(() =>
            Accept(
                booking,
                T0,
                1_200_000m,
                rooms: RoomLines(booking, 500_000m, 700_000m),
                existing: first,
                sourceOfferReference: "offer-high"));
        Assert.Equal(1_000_000m, first.Monetary.Total.Amount);
    }

    [Fact]
    public void Lower_Replacement_Offer_Is_Not_Silently_Accepted()
    {
        var booking = TwoRoomBooking();
        var first = Accept(booking, T0, 1_000_000m, sourceOfferReference: "offer-high");
        Assert.Throws<InvalidOperationException>(() =>
            Accept(
                booking,
                T0,
                800_000m,
                rooms: RoomLines(booking, 300_000m, 500_000m),
                existing: first,
                sourceOfferReference: "offer-low"));
        Assert.Equal(1_000_000m, first.Monetary.Total.Amount);
    }

    [Fact]
    public void Accepted_Snapshot_Is_Immutable()
    {
        var booking = TwoRoomBooking();
        var snapshot = Accept(booking, T0, 1_000_000m);
        Assert.DoesNotContain(
            typeof(HotelRateOfferSnapshot).GetMethods()
                .Where(m => m.DeclaringType == typeof(HotelRateOfferSnapshot) && m.IsPublic && !m.IsStatic)
                .Select(m => m.Name),
            name => name.StartsWith("Set", StringComparison.Ordinal));
        Assert.Null(typeof(HotelRateOfferSnapshot).GetMethod("Overwrite"));
        Assert.Equal(1_000_000m, snapshot.Monetary.Total.Amount);
        var amendment = Assert.Throws<InvalidOperationException>(
            booking.GuardAgainstSilentStayAmendmentAfterAcceptedRateOffer);
        Assert.Contains("requote", amendment.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zero_Full_And_Partial_Penalty_Facts_Are_Accepted()
    {
        var booking = TwoRoomBooking();
        var rules = new[]
        {
            new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromHours(6)), Irr(0m)),
            new HotelCancellationPenaltyRuleDraft(T0.Plus(Duration.FromHours(6)), T0.Plus(Duration.FromHours(12)), Irr(250_000m)),
            new HotelCancellationPenaltyRuleDraft(T0.Plus(Duration.FromHours(12)), null, Irr(1_000_000m)),
        };
        var snapshot = Accept(booking, T0, 1_000_000m, rules: rules);
        Assert.Equal(3, snapshot.CancellationPolicy.Rules.Count);
        Assert.Equal(0m, snapshot.CancellationPolicy.Rules[0].Penalty.Amount);
        Assert.Equal(250_000m, snapshot.CancellationPolicy.Rules[1].Penalty.Amount);
        Assert.Equal(1_000_000m, snapshot.CancellationPolicy.Rules[2].Penalty.Amount);
        Assert.Equal(typeof(Instant), snapshot.CancellationPolicy.Rules[0].EffectiveFrom.GetType());
        Assert.Equal("Asia/Tehran", snapshot.CancellationPolicy.PropertyTimeZoneId);
        Assert.False(HotelRateOfferOwnershipBoundary.PartialRefundExecutionImplemented);
    }

    [Fact]
    public void Penalty_Below_Zero_Or_Above_Total_Is_Rejected()
    {
        var booking = TwoRoomBooking();
        Assert.Throws<ArgumentException>(() =>
            Accept(booking, T0, 1_000_000m, rules:
            [
                new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromDays(1)), Irr(-1m)),
            ]));
        Assert.Throws<ArgumentException>(() =>
            Accept(booking, T0, 1_000_000m, rules:
            [
                new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromDays(1)), Irr(1_000_001m)),
            ]));
    }

    [Fact]
    public void Penalty_Currency_Mismatch_Is_Rejected()
    {
        var booking = TwoRoomBooking();
        Assert.Throws<ArgumentException>(() =>
            Accept(booking, T0, 1_000_000m, rules:
            [
                new HotelCancellationPenaltyRuleDraft(
                    T0,
                    T0.Plus(Duration.FromDays(1)),
                    new MoneyValue(0m, CurrencyCode.Parse("USD"))),
            ]));
    }

    [Fact]
    public void Overlapping_Penalty_Windows_Are_Rejected()
    {
        var booking = TwoRoomBooking();
        Assert.Throws<ArgumentException>(() =>
            Accept(booking, T0, 1_000_000m, rules:
            [
                new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromHours(10)), Irr(0m)),
                new HotelCancellationPenaltyRuleDraft(T0.Plus(Duration.FromHours(5)), T0.Plus(Duration.FromHours(12)), Irr(1_000_000m)),
            ]));
    }

    [Fact]
    public void Rate_Request_And_Snapshots_Contain_No_Guest_Pii()
    {
        var booking = TwoRoomBooking();
        var snapshot = Accept(booking, T0, 1_000_000m);
        var types = new[]
        {
            typeof(HotelRateOfferRequest),
            typeof(HotelRateOfferRoomRequest),
            typeof(HotelRateOfferSourceResult),
            typeof(HotelRateOfferSnapshot),
            typeof(HotelRoomRateSnapshot),
            typeof(HotelBookingMonetarySnapshot),
            typeof(HotelChargeComponentSnapshot),
            typeof(HotelCancellationPolicySnapshot),
            typeof(HotelCancellationPenaltyRule),
        };
        string[] forbidden =
        [
            "Email", "Phone", "GivenName", "FamilyName", "Passport", "NationalId", "NationalID",
            "CardNumber", "Card", "GuestName",
        ];
        foreach (var type in types)
        {
            var names = type.GetProperties().Select(p => p.Name).ToArray();
            foreach (var token in forbidden)
            {
                Assert.DoesNotContain(token, names);
            }
        }

        var request = new HotelRateOfferRequest(
            booking.Id.Value,
            booking.Place.PlaceId,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.Rooms.Select(r => new HotelRateOfferRoomRequest(r.Id.Value, r.AdultCount, [])).ToArray());
        Assert.Equal(2, request.Rooms.Count);
        Assert.Equal("test-source", snapshot.SourceKey);
    }
}
