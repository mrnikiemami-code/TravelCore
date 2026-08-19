using TravelCore.Modules.B2B.Contracts;
using TravelCore.Modules.B2B.Domain;
using TravelCore.Modules.B2B.Infrastructure;
using TravelCore.Modules.Payment.Contracts;
using Xunit;

namespace TravelCore.Modules.B2B.UnitTests;

public sealed class B2BScaffoldingSmokeTests
{
    [Fact]
    public void B2BContractsAssembly_IsLoadable()
    {
        var marker = typeof(B2BContractsAssemblyMarker);
        Assert.Equal("TravelCore.Modules.B2B.Contracts", marker.Namespace);
        Assert.Equal("TravelCore.Modules.B2B.Contracts", marker.Assembly.GetName().Name);
    }

    [Fact]
    public void B2BDomainAssembly_IsLoadable()
    {
        var marker = typeof(B2BDomainAssemblyMarker);
        Assert.Equal("TravelCore.Modules.B2B.Domain", marker.Namespace);
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.B2B.Domain.Agency"));
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.B2B.Domain.Contract"));
        Assert.Null(marker.Assembly.GetType("TravelCore.Modules.B2B.Domain.BookingBase"));
    }

    [Fact]
    public void OwnershipBoundary_Keeps_T001_Foundation_Only()
    {
        Assert.Equal("B2B", B2BOwnershipBoundary.OwnerModule);
        Assert.Equal("b2b", B2BOwnershipBoundary.SchemaName);
        Assert.Equal("B2B != Identity", B2BOwnershipBoundary.B2BIsNotIdentity);
        Assert.Equal("B2B != Access", B2BOwnershipBoundary.B2BIsNotAccess);
        Assert.Equal("B2B != Party", B2BOwnershipBoundary.B2BIsNotParty);
        Assert.Equal("B2B != Booking", B2BOwnershipBoundary.B2BIsNotBooking);
        Assert.Equal("B2B != Payment", B2BOwnershipBoundary.B2BIsNotPayment);
        Assert.False(B2BOwnershipBoundary.OwnsIdentityCredentials);
        Assert.False(B2BOwnershipBoundary.OwnsAccessAuthorization);
        Assert.False(B2BOwnershipBoundary.OwnsPartyIdentity);
        Assert.False(B2BOwnershipBoundary.OwnsBookingExecution);
        Assert.False(B2BOwnershipBoundary.OwnsPaymentExecution);
        Assert.False(B2BOwnershipBoundary.GenericBookingAbstractionImplemented);
        Assert.True(B2BOwnershipBoundary.SeparateB2BModuleImplemented);
        Assert.True(B2BOwnershipBoundary.SeparateB2BSchemaImplemented);
        Assert.False(B2BOwnershipBoundary.AgencyEntityImplemented);
        Assert.False(B2BOwnershipBoundary.ContractEntityImplemented);
        Assert.False(B2BOwnershipBoundary.PaymentTargetAdded);
        Assert.False(B2BOwnershipBoundary.PublicApiImplemented);
        Assert.False(B2BOwnershipBoundary.ProductTablesImplemented);
    }

    [Fact]
    public void B2BDbContext_Owns_Schema_b2b()
    {
        Assert.Equal("b2b", B2BDbContext.SchemaName);
        Assert.Equal(B2BOwnershipBoundary.SchemaName, B2BDbContext.SchemaName);
    }

    [Fact]
    public void PaymentTargetKind_Remains_Closed_To_Three_Kinds()
    {
        var names = Enum.GetNames<PaymentTargetKind>();
        Assert.Equal(3, names.Length);
        Assert.Contains("TourBooking", names);
        Assert.Contains("HotelBooking", names);
        Assert.Contains("FlightBooking", names);
        Assert.DoesNotContain("B2B", names);
    }
}
