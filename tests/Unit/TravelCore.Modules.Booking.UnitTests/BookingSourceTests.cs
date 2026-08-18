using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Modules.Booking.UnitTests;

public sealed class BookingSourceTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 3, 0);
    private static readonly TourDepartureReference Departure =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000701"));
    private static readonly Guid ProfileId = Guid.Parse("0198b3e0-0000-7000-8000-000000000702");
    private static readonly Guid OfferId = Guid.Parse("0198b3e0-0000-7000-8000-000000000703");

    [Fact]
    public void Create_Without_Agency_Context_Is_Direct()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        Assert.Equal(BookingSourceKind.Direct, booking.Source.Kind);
        Assert.Null(booking.Source.AgencyProfile);
        Assert.Null(booking.Source.AgencyOffer);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.True(BookingSourceBoundary.DirectAndAgencyUseSameAggregate);
        Assert.False(BookingSourceBoundary.AgencyBookingAggregateImplemented);
        Assert.Null(typeof(BookingAggregate).GetMethod("SetSource"));
        Assert.Null(typeof(BookingAggregate).GetMethod("Confirm"));
        Assert.Equal(
            new[] { BookingSourceKind.Direct, BookingSourceKind.Agency },
            Enum.GetValues<BookingSourceKind>());
    }

    [Fact]
    public void Direct_Source_Rejects_AgencyProfileReference()
    {
        Assert.Throws<InvalidOperationException>(() =>
            BookingSourceContext.Create(
                BookingSourceKind.Direct,
                new AgencyProfileReference(ProfileId)));
    }

    [Fact]
    public void Direct_Source_Rejects_AgencyOfferReference()
    {
        Assert.Throws<InvalidOperationException>(() =>
            BookingSourceContext.Create(
                BookingSourceKind.Direct,
                agencyOffer: new AgencyOfferReference(OfferId)));
    }

    [Fact]
    public void Agency_Source_Requires_AgencyProfileReference()
    {
        Assert.Throws<InvalidOperationException>(() =>
            BookingSourceContext.Create(BookingSourceKind.Agency));
        Assert.Equal(
            "AgencyOfferReference is optional; AgencyProfileReference is required for Agency source",
            BookingSourceBoundary.AgencyOfferReferenceRequirement);
    }

    [Fact]
    public void Agency_Source_May_Preserve_Optional_AgencyOfferReference()
    {
        var withOffer = BookingAggregate.Create(
            Departure,
            Now,
            BookingSourceContext.ForAgency(
                new AgencyProfileReference(ProfileId),
                new AgencyOfferReference(OfferId)));
        Assert.Equal(BookingSourceKind.Agency, withOffer.Source.Kind);
        Assert.Equal(ProfileId, withOffer.Source.AgencyProfile!.Value.AgencyProfileId);
        Assert.Equal(OfferId, withOffer.Source.AgencyOffer!.Value.AgencyOfferId);
        Assert.Equal(BookingStatus.Pending, withOffer.Status);

        var withoutOffer = BookingAggregate.Create(
            Departure,
            Now,
            BookingSourceContext.ForAgency(new AgencyProfileReference(ProfileId)));
        Assert.Equal(BookingSourceKind.Agency, withoutOffer.Source.Kind);
        Assert.Null(withoutOffer.Source.AgencyOffer);
        Assert.Equal(typeof(BookingAggregate), withOffer.GetType());
        Assert.Equal(typeof(BookingAggregate), withoutOffer.GetType());
    }

    [Fact]
    public void Source_Context_Cannot_Be_Arbitrarily_Changed()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        Assert.Null(typeof(BookingAggregate).GetMethod("SetSource"));
        Assert.Null(typeof(BookingAggregate).GetMethod("SetSourceKind"));
        Assert.False(BookingSourceBoundary.SourceMutationImplemented);
        Assert.Equal(BookingSourceKind.Direct, booking.Source.Kind);
    }

    [Fact]
    public void BookingStatus_Is_Unaffected_By_Source_Kind()
    {
        var agency = BookingAggregate.Create(
            Departure,
            Now,
            BookingSourceContext.ForAgency(new AgencyProfileReference(ProfileId)));
        Assert.Equal(BookingStatus.Pending, agency.Status);
        agency.CancelPending(Now.Plus(Duration.FromMinutes(1)));
        Assert.Equal(BookingStatus.Cancelled, agency.Status);
        Assert.Equal(BookingSourceKind.Agency, agency.Source.Kind);
        Assert.Equal(
            new[] { "Pending", "Confirmed", "Cancelled" },
            Enum.GetNames<BookingStatus>());
        Assert.DoesNotContain("AwaitingAgency", Enum.GetNames<BookingStatus>());
        Assert.DoesNotContain("AgencyAccepted", Enum.GetNames<BookingStatus>());
        Assert.DoesNotContain("AgencyRejected", Enum.GetNames<BookingStatus>());
        Assert.Equal("BookingSourceKind != BookingStatus", BookingSourceBoundary.BookingSourceKindIsNotBookingStatus);
        Assert.False(BookingSourceBoundary.AgencyAcceptanceLifecycleImplemented);
    }

    [Fact]
    public void Source_Context_Does_Not_Alter_Monetary_Snapshot_Semantics()
    {
        var booking = BookingAggregate.Create(
            Departure,
            Now,
            BookingSourceContext.ForAgency(
                new AgencyProfileReference(ProfileId),
                new AgencyOfferReference(OfferId)));
        Assert.Null(booking.MonetarySnapshot);
        Assert.False(BookingSourceBoundary.AgencyPriceOverrideImplemented);
        Assert.Equal("AgencyOffer != Quote", BookingSourceBoundary.AgencyOfferIsNotQuote);
        Assert.Equal("Agency context != Pricing Authority", BookingSourceBoundary.AgencyContextIsNotPricingAuthority);
        Assert.True(BookingOwnershipBoundary.QuoteIntegrationImplemented);
        Assert.False(BookingOwnershipBoundary.OwnsPricing);
        Assert.Null(typeof(BookingAggregate).GetMethod("SetPrice"));
    }
}
