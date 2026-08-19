using TravelCore.Modules.Notification.Contracts;
using TravelCore.Modules.Notification.Domain;
using TravelCore.Modules.Notification.Infrastructure;
using TravelCore.Modules.Payment.Contracts;
using Xunit;

namespace TravelCore.Modules.Notification.UnitTests;

public sealed class NotificationScaffoldingSmokeTests
{
    [Fact]
    public void NotificationContractsAssembly_IsLoadable()
    {
        var marker = typeof(NotificationContractsAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Notification.Contracts", marker.Namespace);
        Assert.Equal("TravelCore.Modules.Notification.Contracts", marker.Assembly.GetName().Name);
    }

    [Fact]
    public void NotificationDomainAssembly_IsLoadable()
    {
        var marker = typeof(NotificationDomainAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Notification.Domain", marker.Namespace);
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.Notification.Domain.NotificationDelivery"));
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.Notification.Domain.EmailProvider"));
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.Notification.Domain.NotificationTemplate"));
    }

    [Fact]
    public void OwnershipBoundary_Keeps_T004_Foundation_Only()
    {
        Assert.Equal("Notification", NotificationOwnershipBoundary.OwnerModule);
        Assert.Equal("notification", NotificationOwnershipBoundary.SchemaName);
        Assert.Equal("Notification != Identity", NotificationOwnershipBoundary.NotificationIsNotIdentity);
        Assert.Equal("Notification != Access", NotificationOwnershipBoundary.NotificationIsNotAccess);
        Assert.Equal("Notification != Party", NotificationOwnershipBoundary.NotificationIsNotParty);
        Assert.Equal("Notification != Booking", NotificationOwnershipBoundary.NotificationIsNotBooking);
        Assert.Equal("Notification != Payment", NotificationOwnershipBoundary.NotificationIsNotPayment);
        Assert.Equal("Notification != TripPlanner", NotificationOwnershipBoundary.NotificationIsNotTripPlanner);
        Assert.Equal("Notification != B2B", NotificationOwnershipBoundary.NotificationIsNotB2B);
        Assert.False(NotificationOwnershipBoundary.OwnsIdentityCredentials);
        Assert.False(NotificationOwnershipBoundary.OwnsAccessAuthorization);
        Assert.False(NotificationOwnershipBoundary.OwnsPartyIdentity);
        Assert.False(NotificationOwnershipBoundary.OwnsBookingExecution);
        Assert.False(NotificationOwnershipBoundary.OwnsPaymentExecution);
        Assert.False(NotificationOwnershipBoundary.OwnsTripPlannerFacts);
        Assert.False(NotificationOwnershipBoundary.OwnsB2BCommerce);
        Assert.True(NotificationOwnershipBoundary.SeparateNotificationModuleImplemented);
        Assert.True(NotificationOwnershipBoundary.SeparateNotificationSchemaImplemented);
        Assert.False(NotificationOwnershipBoundary.ProviderImplemented);
        Assert.False(NotificationOwnershipBoundary.ProductTablesImplemented);
        Assert.False(NotificationOwnershipBoundary.PublicApiImplemented);
        Assert.False(NotificationOwnershipBoundary.ModifiesPaymentTargets);
    }

    [Fact]
    public void PublisherBoundary_Keeps_Delivery_Ownership_In_Notification()
    {
        Assert.Equal("Booking", NotificationPublisherBoundary.BookingPublisherOwner);
        Assert.Equal("Payment", NotificationPublisherBoundary.PaymentPublisherOwner);
        Assert.Equal("TripPlanner", NotificationPublisherBoundary.TripPlannerPublisherOwner);
        Assert.Equal("Notification", NotificationPublisherBoundary.DeliveryOwner);
    }

    [Fact]
    public void NotificationDbContext_Owns_Schema_notification()
    {
        Assert.Equal("notification", NotificationDbContext.SchemaName);
        Assert.Equal(NotificationOwnershipBoundary.SchemaName, NotificationDbContext.SchemaName);
    }

    [Fact]
    public void PaymentTargetKind_Remains_Closed_To_Three_Kinds()
    {
        var names = Enum.GetNames<PaymentTargetKind>();
        Assert.Equal(3, names.Length);
        Assert.Contains("TourBooking", names);
        Assert.Contains("HotelBooking", names);
        Assert.Contains("FlightBooking", names);
        Assert.DoesNotContain("Notification", names);
    }
}
