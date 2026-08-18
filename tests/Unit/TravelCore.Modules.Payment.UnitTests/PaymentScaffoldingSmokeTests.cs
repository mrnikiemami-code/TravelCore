using TravelCore.Identifiers;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using Xunit;

namespace TravelCore.Modules.Payment.UnitTests;

public sealed class PaymentScaffoldingSmokeTests
{
    [Fact]
    public void PaymentContractsAssembly_IsLoadable()
    {
        var marker = typeof(PaymentContractsAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Payment.Contracts", marker.Namespace);
        Assert.Equal("TravelCore.Modules.Payment.Contracts", marker.Assembly.GetName().Name);
    }

    [Fact]
    public void PaymentDomainAssembly_IsLoadable()
    {
        var marker = typeof(PaymentDomainAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Payment.Domain", marker.Namespace);
    }

    [Fact]
    public void OwnershipBoundary_Keeps_Peer_SoT_Out_Of_Payment()
    {
        Assert.Equal("Payment", PaymentOwnershipBoundary.OwnerModule);
        Assert.Equal("payment", PaymentOwnershipBoundary.SchemaName);
        Assert.Equal("Booking", PaymentOwnershipBoundary.InitialTarget);
        Assert.Equal("Tour Booking", PaymentOwnershipBoundary.InitialScope);
        Assert.Equal("Booking", PaymentOwnershipBoundary.BookingOwner);
        Assert.Equal("Pricing", PaymentOwnershipBoundary.PricingOwner);
        Assert.Equal("OpaqueLogicalBookingId", PaymentOwnershipBoundary.LogicalReferencePosture);
        Assert.Equal("UUIDv7", PaymentOwnershipBoundary.IdentityConvention);
        Assert.Equal("TravelCore.Money", PaymentOwnershipBoundary.MoneyModel);
        Assert.Equal("NodaTime", PaymentOwnershipBoundary.TemporalModel);
        Assert.Equal("SecureConfigurationNotSourceControl", PaymentOwnershipBoundary.ProviderSecretPosture);
        Assert.Equal("Payment != Booking", PaymentOwnershipBoundary.PaymentIsNotBooking);
        Assert.Equal("Payment != Pricing", PaymentOwnershipBoundary.PaymentIsNotPricing);
        Assert.Equal("Payment != Quote", PaymentOwnershipBoundary.PaymentIsNotQuote);
        Assert.Equal("Payment != BookingMonetarySnapshot", PaymentOwnershipBoundary.PaymentIsNotBookingMonetarySnapshot);
        Assert.Equal("Payment != Bank Settlement", PaymentOwnershipBoundary.PaymentIsNotBankSettlement);
        Assert.Equal("Payment != Accounting Ledger", PaymentOwnershipBoundary.PaymentIsNotAccountingLedger);
        Assert.Equal("Payment != Agency Settlement", PaymentOwnershipBoundary.PaymentIsNotAgencySettlement);
        Assert.Equal("PaymentStatus != BookingStatus", PaymentOwnershipBoundary.PaymentStatusIsNotBookingStatus);
        Assert.Equal("PaymentSucceeded != BookingConfirmed", PaymentOwnershipBoundary.PaymentSucceededIsNotBookingConfirmed);
        Assert.Equal("BookingCancelled != PaymentRefunded", PaymentOwnershipBoundary.BookingCancelledIsNotPaymentRefunded);
        Assert.Equal("Toman != CurrencyCode", PaymentOwnershipBoundary.TomanIsNotCurrencyCode);
        Assert.False(PaymentOwnershipBoundary.OwnsBooking);
        Assert.False(PaymentOwnershipBoundary.OwnsBookingStatus);
        Assert.False(PaymentOwnershipBoundary.OwnsCapacityHold);
        Assert.False(PaymentOwnershipBoundary.OwnsBookingMonetarySnapshot);
        Assert.False(PaymentOwnershipBoundary.OwnsPassengerOrContact);
        Assert.False(PaymentOwnershipBoundary.OwnsPricing);
        Assert.False(PaymentOwnershipBoundary.OwnsQuote);
        Assert.False(PaymentOwnershipBoundary.OwnsTourCatalog);
        Assert.False(PaymentOwnershipBoundary.OwnsBankSettlement);
        Assert.False(PaymentOwnershipBoundary.OwnsAccountingLedger);
        Assert.False(PaymentOwnershipBoundary.OwnsAgencySettlement);
        Assert.True(PaymentOwnershipBoundary.ProductReferencesAreLogicalOnly);
        Assert.Equal("Payment != PaymentAttempt", PaymentOwnershipBoundary.PaymentIsNotPaymentAttempt);
        Assert.Equal("PaymentStatus != PaymentAttemptStatus", PaymentOwnershipBoundary.PaymentStatusIsNotPaymentAttemptStatus);
        Assert.Equal("Failed PaymentAttempt != Failed Payment", PaymentOwnershipBoundary.FailedAttemptIsNotFailedPayment);
        Assert.True(PaymentOwnershipBoundary.PaymentAggregateImplemented);
        Assert.True(PaymentOwnershipBoundary.PaymentStatusImplemented);
        Assert.True(PaymentOwnershipBoundary.PaymentAttemptImplemented);
        Assert.True(PaymentOwnershipBoundary.RefundImplemented);
        Assert.False(PaymentOwnershipBoundary.ProviderAdapterImplemented);
        Assert.True(PaymentOwnershipBoundary.ProviderPortImplemented);
        Assert.True(PaymentOwnershipBoundary.CallbackEndpointImplemented);
        Assert.False(PaymentOwnershipBoundary.PaymentApiImplemented);
        Assert.False(PaymentOwnershipBoundary.PaymentUiImplemented);
        Assert.False(PaymentOwnershipBoundary.BookingConfirmImplemented);
        Assert.False(PaymentOwnershipBoundary.TomanIsCurrencyCode);
        Assert.False(PaymentOwnershipBoundary.CardPanStored);
        Assert.False(PaymentOwnershipBoundary.CardCvvStored);
    }

    [Fact]
    public void BookingReference_Is_Opaque_Logical_Id_Not_A_Booking_Entity()
    {
        var logicalId = Guid.Parse("0198b3e0-0000-7000-8000-000000000020");
        var reference = new BookingReference(logicalId);
        Assert.Equal(logicalId, reference.BookingId);
        Assert.Equal("BookingReference", nameof(BookingReference));
        Assert.False(typeof(BookingReference).IsClass);
        Assert.Throws<ArgumentException>(() => new BookingReference(Guid.Empty));
    }

    [Fact]
    public void Future_Payment_Identities_Use_Platform_Uuid7()
    {
        var id = Uuid7.New();
        Assert.Equal(7, id.Version);
    }

    [Fact]
    public void PaymentDbContext_Owns_Schema_payment()
    {
        Assert.Equal("payment", PaymentDbContext.SchemaName);
        Assert.Equal(PaymentOwnershipBoundary.SchemaName, PaymentDbContext.SchemaName);
    }

    [Fact]
    public void Payment_T006_Has_Refund_Distinct_From_Payment()
    {
        var domain = typeof(PaymentDomainAssemblyMarker).Assembly;
        Assert.NotNull(domain.GetType("TravelCore.Modules.Payment.Domain.Payment"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.Payment.Domain.Refund"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.Payment.Domain.RefundStatus"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.Payment.Domain.RefundAttempt"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.Payment.Domain.RefundAttemptStatus"));
        Assert.Equal(
            new[] { RefundStatus.Pending, RefundStatus.Succeeded },
            Enum.GetValues<RefundStatus>());
        Assert.DoesNotContain("Refunded", Enum.GetNames<PaymentStatus>());
        Assert.Equal("Payment != Refund", PaymentRefundBoundary.PaymentIsNotRefund);
    }
}
