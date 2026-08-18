using TravelCore.Modules.Payment.Contracts;
using Xunit;

namespace TravelCore.Modules.Payment.UnitTests;

public sealed class PaymentPublicSurfaceTests
{
    [Fact]
    public void Public_Payment_Is_Booking_Scoped_And_Not_Client_Authoritative()
    {
        Assert.Equal("/api/booking/public/{bookingId}/payment", PublicPaymentCompositionBoundary.StatusRoute);
        Assert.Equal(
            "/api/booking/public/{bookingId}/payment/initiation",
            PublicPaymentCompositionBoundary.InitiationRoute);
        Assert.Equal("BookingId != authorization", PublicPaymentCompositionBoundary.BookingIdIsNotAuthorization);
        Assert.Equal("PaymentId != Access Credential", PublicPaymentCompositionBoundary.PaymentIdIsNotAccessCredential);
        Assert.Equal("BrowserReturn != PaymentSuccess", PublicPaymentCompositionBoundary.BrowserReturnIsNotPaymentSuccess);
        Assert.False(PublicPaymentCompositionBoundary.PublicRefundApiImplemented);
        Assert.False(PublicPaymentCompositionBoundary.PublicPaymentListImplemented);
        Assert.False(PublicPaymentCompositionBoundary.GenericPaymentLookupImplemented);
        Assert.False(PublicPaymentCompositionBoundary.ClientAmountAuthorityImplemented);
        Assert.False(PublicPaymentCompositionBoundary.ClientSuccessAuthorityImplemented);
        Assert.False(PublicPaymentCompositionBoundary.CardCollectionImplemented);
        Assert.False(PaymentProviderTrustBoundary.NamedProductionAdapterImplemented);
        Assert.Equal("NONE", PaymentProviderTrustBoundary.NamedProviderSelected);
        Assert.False(PaymentOwnershipBoundary.PaymentApiImplemented);
        Assert.True(typeof(PublicPaymentInitiationRequest)
            .GetProperties()
            .Select(x => x.Name)
            .SequenceEqual(["IdempotencyKey"]));
    }
}
