using TravelCore.Modules.DynamicPackage.Contracts;
using TravelCore.Modules.DynamicPackage.Domain;
using TravelCore.Modules.DynamicPackage.Infrastructure;
using TravelCore.Modules.Payment.Contracts;
using Xunit;

namespace TravelCore.Modules.DynamicPackage.UnitTests;

public sealed class DynamicPackageScaffoldingSmokeTests
{
    [Fact]
    public void DynamicPackageContractsAssembly_IsLoadable()
    {
        var marker = typeof(DynamicPackageContractsAssemblyMarker);
        Assert.Equal("TravelCore.Modules.DynamicPackage.Contracts", marker.Namespace);
        Assert.Equal("TravelCore.Modules.DynamicPackage.Contracts", marker.Assembly.GetName().Name);
    }

    [Fact]
    public void DynamicPackageDomainAssembly_IsLoadable()
    {
        var marker = typeof(DynamicPackageDomainAssemblyMarker);
        Assert.Equal("TravelCore.Modules.DynamicPackage.Domain", marker.Namespace);
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.DynamicPackage.Domain.DynamicPackageBooking"));
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.DynamicPackage.Domain.BookingBase"));
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.DynamicPackage.Domain.GenericBookingAggregate"));
    }

    [Fact]
    public void OwnershipBoundary_Keeps_T001_Foundation_Only()
    {
        Assert.Equal("DynamicPackage", DynamicPackageOwnershipBoundary.OwnerModule);
        Assert.Equal("dynamic_package", DynamicPackageOwnershipBoundary.SchemaName);
        Assert.Equal("DynamicPackage", DynamicPackageOwnershipBoundary.TransactionAggregateOwner);
        Assert.Equal("DynamicPackageBooking", DynamicPackageOwnershipBoundary.TransactionAggregateName);
        Assert.Equal("NONE", DynamicPackageOwnershipBoundary.ProductionCompositionSource);
        Assert.Equal("NONE", DynamicPackageOwnershipBoundary.ProductionOrchestrationSource);
        Assert.Equal("DynamicPackage != Tour", DynamicPackageOwnershipBoundary.DynamicPackageIsNotTour);
        Assert.Equal("DynamicPackage != Tour Booking", DynamicPackageOwnershipBoundary.DynamicPackageIsNotTourBooking);
        Assert.Equal("DynamicPackage != Flight", DynamicPackageOwnershipBoundary.DynamicPackageIsNotFlight);
        Assert.Equal("DynamicPackage != HotelBooking", DynamicPackageOwnershipBoundary.DynamicPackageIsNotHotelBooking);
        Assert.Equal(
            "DynamicPackageBooking != FlightBooking",
            DynamicPackageOwnershipBoundary.DynamicPackageBookingIsNotFlightBooking);
        Assert.Equal(
            "DynamicPackageBooking != HotelBooking",
            DynamicPackageOwnershipBoundary.DynamicPackageBookingIsNotHotelBooking);
        Assert.False(DynamicPackageOwnershipBoundary.OwnsTourPackageTransport);
        Assert.False(DynamicPackageOwnershipBoundary.OwnsTourBooking);
        Assert.False(DynamicPackageOwnershipBoundary.OwnsFlightBookingExecution);
        Assert.False(DynamicPackageOwnershipBoundary.OwnsHotelBookingExecution);
        Assert.False(DynamicPackageOwnershipBoundary.OwnsPayment);
        Assert.False(DynamicPackageOwnershipBoundary.GenericBookingAbstractionImplemented);
        Assert.True(DynamicPackageOwnershipBoundary.SeparateDynamicPackageModuleImplemented);
        Assert.True(DynamicPackageOwnershipBoundary.SeparateDynamicPackageSchemaImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.DynamicPackageBookingAggregateImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.CompositionModelImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.OrchestrationModelImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.PaymentIntegrationImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.PublicApiImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.FrontendImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.SupplierSdkImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.SharedDbContextImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.ProductTablesImplemented);
    }

    [Fact]
    public void DynamicPackageDbContext_Owns_Schema_dynamic_package()
    {
        Assert.Equal("dynamic_package", DynamicPackageDbContext.SchemaName);
        Assert.Equal(DynamicPackageOwnershipBoundary.SchemaName, DynamicPackageDbContext.SchemaName);
    }

    [Fact]
    public void PaymentTargetKind_Remains_Closed_To_Three_Kinds()
    {
        var names = Enum.GetNames<PaymentTargetKind>();
        Assert.Equal(3, names.Length);
        Assert.Contains("TourBooking", names);
        Assert.Contains("HotelBooking", names);
        Assert.Contains("FlightBooking", names);
        Assert.DoesNotContain("DynamicPackageBooking", names);
    }
}
