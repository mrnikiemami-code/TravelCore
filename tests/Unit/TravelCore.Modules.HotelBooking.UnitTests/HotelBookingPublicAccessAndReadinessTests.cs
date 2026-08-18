using NodaTime;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.HotelBooking.Infrastructure.Services;
using TravelCore.Modules.Payment.Contracts;
using Xunit;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.UnitTests;

public sealed class HotelBookingPublicAccessAndReadinessTests
{
    [Fact]
    public void Access_Token_Hash_Is_Sha256_Hex_And_Raw_Is_High_Entropy()
    {
        var raw = HotelBookingAccessToken.CreateRaw();
        Assert.True(raw.Length >= 32);
        var hash = HotelBookingAccessToken.Hash(raw);
        Assert.Equal(64, hash.Length);
        Assert.Equal(hash, HotelBookingAccessToken.Hash(raw));
        Assert.NotEqual(hash, HotelBookingAccessToken.Hash(HotelBookingAccessToken.CreateRaw()));
        Assert.DoesNotContain(raw, hash, StringComparison.Ordinal);
    }

    [Fact]
    public void Credential_Persists_Hash_Not_Raw_Token()
    {
        var bookingId = HotelBookingId.New();
        var raw = HotelBookingAccessToken.CreateRaw();
        var credential = HotelBookingAccessCredential.Create(
            bookingId,
            HotelBookingAccessToken.Hash(raw),
            Instant.FromUtc(2026, 8, 18, 12, 0));
        Assert.Equal(bookingId, credential.HotelBookingId);
        Assert.Equal(HotelBookingAccessToken.Hash(raw), credential.TokenHash);
        Assert.DoesNotContain(raw, credential.TokenHash, StringComparison.Ordinal);
        Assert.Null(typeof(HotelBookingAccessCredential).GetProperty("RawToken"));
        Assert.Null(typeof(HotelBookingAccessCredential).GetProperty("Token"));
    }

    [Fact]
    public void Source_Catalog_Rejects_Duplicate_Keys_And_Does_Not_Infer_Capabilities()
    {
        var first = new StubSource("alpha", enabled: true, [HotelSourceCapability.AvailabilityHold]);
        var catalog = new HotelSourceCatalog([first]);
        Assert.Single(catalog.List());
        Assert.Equal("alpha", catalog.Find("alpha")!.SourceKey);
        Assert.Null(catalog.Find("missing"));
        Assert.Throws<InvalidOperationException>(() =>
            new HotelSourceCatalog([first, new StubSource("alpha", true, [HotelSourceCapability.RateQuote])]));
        Assert.Empty(new HotelSourceCatalog([]).List());
        Assert.False(HotelSourceReadinessBoundary.CapabilityInferredFromSourceName);
        Assert.True(HotelSourceReadinessBoundary.ZeroProductionSourcesValid);
        var disabled = new HotelSourceCatalog(
            [new StubSource("beta", enabled: false, [HotelSourceCapability.RateQuote])]);
        Assert.False(disabled.Find("beta")!.Enabled);
    }

    [Fact]
    public void Operational_Dto_Omits_Pii_And_Secrets()
    {
        var names = typeof(HotelBookingOperationalRead).GetProperties().Select(p => p.Name).ToHashSet();
        Assert.DoesNotContain("Email", names);
        Assert.DoesNotContain("Phone", names);
        Assert.DoesNotContain("GivenName", names);
        Assert.DoesNotContain("Passport", names);
        Assert.DoesNotContain("AccessToken", names);
        Assert.DoesNotContain("ProviderSecret", names);
        Assert.Contains("Occupancy", names);
        Assert.Contains("ReconciliationSummary", names);
    }

    [Fact]
    public void Journey_Progression_Does_Not_Accept_Client_Occupancy()
    {
        foreach (var name in new[]
        {
            nameof(IPublicHotelBookingJourneyService.RequestAvailabilityAsync),
            nameof(IPublicHotelBookingJourneyService.RequestRateOfferAsync),
        })
        {
            var method = typeof(IPublicHotelBookingJourneyService).GetMethod(name);
            Assert.NotNull(method);
            Assert.DoesNotContain(
                method!.GetParameters().Select(p => p.Name),
                p => p is "rooms" or "occupancy" or "guests" or "amount" or "currencyCode");
        }
    }

    [Fact]
    public void Presentation_States_Are_Read_Model_Only_And_Payment_Succeeded_Is_Not_Confirmed()
    {
        Assert.Equal(
            new[] { "Pending", "Confirmed", "Cancelled" },
            Enum.GetNames<HotelBookingStatus>());
        var now = Instant.FromUtc(2026, 8, 18, 12, 0);
        var booking = Stay.Create(
            new HotelPlaceReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000021")),
            new LocalDate(2026, 9, 1),
            new LocalDate(2026, 9, 3),
            HotelBookingContactSnapshot.Create(email: "lead@example.com"),
            [
                new RoomReservationSpecification(
                [
                    new HotelBookingGuestSpecification("Ada", "Lovelace", HotelGuestCategory.Adult, true),
                ]),
            ]);
        var payment = new PublicPaymentRead(
            Guid.CreateVersion7(),
            "Succeeded",
            1_000_000m,
            "IRR",
            ProviderInitiationPossible: false,
            LatestAttemptStatus: "Succeeded",
            RefundStatus: null,
            SafeAction: "Succeeded",
            RedirectUri: null);
        var received = PublicHotelBookingMapper.ToRead(
            new PublicHotelBookingFacts(booking, null, null, null, null, [], payment, now));
        Assert.Equal(PublicHotelBookingPresentationStates.PaymentReceived, received.PresentationState);
        Assert.False(received.Confirmed);
        Assert.Equal("Pending", received.Status);
        Assert.Contains("confirmation", received.SafeMessage, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class StubSource(
        string sourceKey,
        bool enabled,
        IReadOnlyList<HotelSourceCapability> capabilities) : IDeclaredHotelSourceCapabilities
    {
        public string SourceKey { get; } = sourceKey;
        public bool Enabled { get; } = enabled;
        public string? DisplayName { get; } = sourceKey;
        public IReadOnlyList<HotelSourceCapability> Capabilities { get; } = capabilities;
    }
}
