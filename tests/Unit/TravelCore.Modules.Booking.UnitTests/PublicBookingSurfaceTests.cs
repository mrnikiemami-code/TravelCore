using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;
using Xunit;

namespace TravelCore.Modules.Booking.UnitTests;

public sealed class PublicBookingSurfaceTests
{
    [Fact]
    public void Access_Token_Hash_Is_Opaque_And_Deterministic()
    {
        var raw = BookingAccessToken.CreateRaw();
        var hash = BookingAccessToken.Hash(raw);
        Assert.NotEqual(raw, hash);
        Assert.Equal(64, hash.Length);
        Assert.Equal(hash, BookingAccessToken.Hash(raw));
        Assert.NotEqual(hash, BookingAccessToken.Hash(raw + "x"));
        Assert.DoesNotContain(raw, hash, StringComparison.Ordinal);
    }

    [Fact]
    public void Public_Boundary_Keeps_Confirm_Payment_Listing_And_Agency_Forge_Out()
    {
        Assert.True(BookingOwnershipBoundary.PublicBookingSurfaceImplemented);
        Assert.False(PublicBookingCompositionBoundary.ConfirmEndpointImplemented);
        Assert.True(PublicBookingCompositionBoundary.PaymentEndpointImplemented);
        Assert.False(PublicBookingCompositionBoundary.PublicListingImplemented);
        Assert.False(PublicBookingCompositionBoundary.PublicCancellationImplemented);
        Assert.False(PublicBookingCompositionBoundary.AgencyOriginOnPublicInitiationImplemented);
        Assert.Equal("PublicExperience != Booking Source of Truth", PublicBookingCompositionBoundary.PublicExperienceIsNotBookingSourceOfTruth);
        Assert.Equal("Public Booking initiation != Booking confirmation", PublicBookingCompositionBoundary.PublicInitiationIsNotConfirmation);
        Assert.Equal("Pending != Confirmed", PublicBookingCompositionBoundary.PendingIsNotConfirmed);
        Assert.Equal("BookingId != Access Credential", PublicBookingCompositionBoundary.BookingIdIsNotAccessCredential);
        Assert.Null(typeof(BookingAggregate).GetMethod("Confirm"));
        Assert.Equal(
            new[] { BookingStatus.Pending, BookingStatus.Confirmed, BookingStatus.Cancelled },
            Enum.GetValues<BookingStatus>());
    }
}
