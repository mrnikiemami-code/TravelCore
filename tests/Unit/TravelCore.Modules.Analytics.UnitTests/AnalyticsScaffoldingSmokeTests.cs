using TravelCore.Modules.Analytics.Contracts;
using TravelCore.Modules.Analytics.Domain;
using TravelCore.Modules.Analytics.Infrastructure;
using TravelCore.Modules.Payment.Contracts;
using Xunit;

namespace TravelCore.Modules.Analytics.UnitTests;

public sealed class AnalyticsScaffoldingSmokeTests
{
    [Fact]
    public void AnalyticsContractsAssembly_IsLoadable()
    {
        var marker = typeof(AnalyticsContractsAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Analytics.Contracts", marker.Namespace);
        Assert.Equal("TravelCore.Modules.Analytics.Contracts", marker.Assembly.GetName().Name);
    }

    [Fact]
    public void AnalyticsDomainAssembly_IsLoadable()
    {
        var marker = typeof(AnalyticsDomainAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Analytics.Domain", marker.Namespace);
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.Analytics.Domain.AnalyticsEventStore"));
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.Analytics.Domain.MixpanelProvider"));
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.Analytics.Domain.AnalyticsWarehouse"));
    }

    [Fact]
    public void OwnershipBoundary_Keeps_T004_Foundation_Only()
    {
        Assert.Equal("Analytics", AnalyticsOwnershipBoundary.OwnerModule);
        Assert.Equal("analytics", AnalyticsOwnershipBoundary.SchemaName);
        Assert.Equal("Analytics != Booking", AnalyticsOwnershipBoundary.AnalyticsIsNotBooking);
        Assert.Equal("Analytics != Payment", AnalyticsOwnershipBoundary.AnalyticsIsNotPayment);
        Assert.Equal("Analytics != Search", AnalyticsOwnershipBoundary.AnalyticsIsNotSearch);
        Assert.Equal("Analytics != SEO", AnalyticsOwnershipBoundary.AnalyticsIsNotSeo);
        Assert.Equal("Analytics != Notification", AnalyticsOwnershipBoundary.AnalyticsIsNotNotification);
        Assert.Equal("Analytics != Observability", AnalyticsOwnershipBoundary.AnalyticsIsNotObservability);
        Assert.False(AnalyticsOwnershipBoundary.OwnsBookingExecution);
        Assert.False(AnalyticsOwnershipBoundary.OwnsPaymentExecution);
        Assert.False(AnalyticsOwnershipBoundary.OwnsSearchRanking);
        Assert.False(AnalyticsOwnershipBoundary.OwnsPlatformTelemetry);
        Assert.True(AnalyticsOwnershipBoundary.SeparateAnalyticsModuleImplemented);
        Assert.True(AnalyticsOwnershipBoundary.SeparateAnalyticsSchemaImplemented);
        Assert.False(AnalyticsOwnershipBoundary.EventTaxonomyBoundaryImplemented);
        Assert.False(AnalyticsOwnershipBoundary.ProviderPortImplemented);
        Assert.False(AnalyticsOwnershipBoundary.ProductTablesImplemented);
        Assert.False(AnalyticsOwnershipBoundary.PublicApiImplemented);
        Assert.False(AnalyticsOwnershipBoundary.ModifiesPaymentTargets);
    }

    [Fact]
    public void PublisherBoundary_Keeps_Dispatch_Ownership_In_Analytics()
    {
        Assert.Equal("Search", AnalyticsPublisherBoundary.SearchPublisherOwner);
        Assert.Equal("Booking", AnalyticsPublisherBoundary.BookingPublisherOwner);
        Assert.Equal("Payment", AnalyticsPublisherBoundary.PaymentPublisherOwner);
        Assert.Equal("Analytics", AnalyticsPublisherBoundary.DispatchOwner);
    }

    [Fact]
    public void AnalyticsDbContext_Owns_Schema_analytics()
    {
        Assert.Equal("analytics", AnalyticsDbContext.SchemaName);
        Assert.Equal(AnalyticsOwnershipBoundary.SchemaName, AnalyticsDbContext.SchemaName);
    }

    [Fact]
    public void PaymentTargetKind_Remains_Closed_To_Three_Kinds()
    {
        var names = Enum.GetNames<PaymentTargetKind>();
        Assert.Equal(3, names.Length);
        Assert.Contains("TourBooking", names);
        Assert.Contains("HotelBooking", names);
        Assert.Contains("FlightBooking", names);
        Assert.DoesNotContain("Analytics", names);
    }
}
